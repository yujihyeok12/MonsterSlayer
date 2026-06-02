using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("--- 게임 정보 ---")]
    public float gameTime;
    public int killCount;
    public int level = 1;
    public float exp;
    public int gold = 0;
    public int pendingLevelUps = 0;

    [Header("--- 레벨 시스템 설정 ---")]
    public int maxLevel = 105;
    public float startExp = 100f;
    public float[] nextExp;

    [Header("--- 아이템 관리 ---")]
    public ItemData[] allItems; 
    public SkillCard[] uiCards; 
    private ItemData[] selectedCards = new ItemData[3]; 

    public Dictionary<ItemData.ItemType, int> itemLevels = new Dictionary<ItemData.ItemType, int>();

    [Header("--- 실제 무기 & 스탯 연결 ---")]
    public Player player;                   
    public BabyDragon babyDragon;           
    public Weapon orbitWeapon;              
    public GameObject orbitKnifePrefab;     
    public GameObject flyingSwordPrefab;    
    public LightningWeapon lightningWeapon;
    public ThrowingDaggerWeapon daggerWeapon;
    public SwordAuraWeapon auraWeapon;
    public float playerBaseSpeed = 5f;      
    public float vampireHealAmount = 0f;    

    [Header("--- 패시브 스탯 ---")]
    public float expMultiplier = 1.0f;

    [Header("--- 캐릭터 시작 설정 ---")]
    public ItemData.ItemType startingWeapon = ItemData.ItemType.Orbit_Count;

    [Header("--- 인벤토리 UI ---")]
    public InventorySlot[] weaponSlots;  
    public InventorySlot[] passiveSlots; 

    [Header("--- 캐릭터 외형 설정 ---")]
    public SpriteRenderer playerSpriter;
    public RuntimeAnimatorController[] playerAnimators;

    [Header("--- 게임 흐름 UI 패널 ---")]
    public GameObject startMessagePanel; 
    public GameObject gameOverPanel;     
    public GameObject gameClearPanel;    

    [Header("--- 일시정지 UI 패널 ---")]
    public GameObject pausePanel;
    private bool isPaused = false;

    [Header("--- 보스 경고 UI ---")]
    public GameObject bossWarningPanel;

    [Header("--- 보물상자 UI ---")]
    public GameObject treasurePanel;    
    public Image treasureIcon;          
    public Text treasureName;           
    public Text treasureDesc;           
    public Sprite[] treasureSprites;   

    private int currentTreasureIndex = -1; 

    [Header("--- 보물상자 획득 현황 UI ---")]
    public InventorySlot[] treasureSlots; 
    private int[] treasureCounts = new int[7];

    [Header("--- 구사일생(해골) 연출 UI ---")]
    public GameObject reviveDarkPanel;
    public bool isMonsterFreeze = false; 

    public bool isCinematic = false;

    [Header("--- 무한 모드 연출 UI ---")]
    public GameObject infiniteModeImagePanel;

    [Header("--- 무한 모드 및 한계 돌파 ---")]
    public ItemData[] limitBreakItems;
    private float lastNextExp;

    [System.Serializable]
    public class CharacterStat
    {
        public string charName;     
        public float maxHealth;     
        public float speed;         
        public float armor;         
        public float magnetRange;   
    }

    [Header("--- 캐릭터별 기본 스탯 ---")]
    public CharacterStat[] charStats;

    private List<ItemData.ItemType> acquiredWeapons = new List<ItemData.ItemType>();
    private List<ItemData.ItemType> acquiredPassives = new List<ItemData.ItemType>();

    [Header("--- 연결할 UI 객체들 ---")]
    public Slider expSlider;
    public Text levelText;
    public Text timeText;
    public Text killText;
    public Text goldText;
    public GameObject levelUpPanel;

    void Awake()
    {
        instance = this;

        foreach (ItemData.ItemType type in System.Enum.GetValues(typeof(ItemData.ItemType)))
        {
            itemLevels[type] = 0;
        }
    }

    void Start()
    {
        gold = PlayerPrefs.GetInt("TotalGold", 0);
        if (goldText != null) goldText.text = gold.ToString();

        InitExpTable();
        UpdateUI();

        for (int i = 0; i < weaponSlots.Length; i++) if (weaponSlots[i] != null) weaponSlots[i].ClearSlot();
        for (int i = 0; i < passiveSlots.Length; i++) if (passiveSlots[i] != null) passiveSlots[i].ClearSlot();
        for (int i = 0; i < treasureSlots.Length; i++)
        {
            if (treasureSlots[i] != null) treasureSlots[i].gameObject.SetActive(false);
        }

        int selectedCharIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        if (selectedCharIndex == 0) startingWeapon = ItemData.ItemType.Orbit_Count;
        else if (selectedCharIndex == 1) startingWeapon = ItemData.ItemType.Dagger_Count;
        else if (selectedCharIndex == 2) startingWeapon = ItemData.ItemType.Lightning_Count;

        GiveStartingWeapon();

        if (playerSpriter != null && playerAnimators.Length > selectedCharIndex)
        {
            Animator playerAnim = playerSpriter.GetComponent<Animator>();
            if (playerAnim != null) playerAnim.runtimeAnimatorController = playerAnimators[selectedCharIndex];
        }

        if (SoundManager.instance != null) SoundManager.instance.PlayBGM(false);

        if (charStats != null && charStats.Length > selectedCharIndex)
        {
            CharacterStat myStat = charStats[selectedCharIndex];

            int bonusHpLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_HP", 0);
            int bonusSpeedLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_Speed", 0);
            int bonusArmorLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_Armor", 0);
            int bonusMagnetLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_Magnet", 0);

            player.speed = myStat.speed + (bonusSpeedLevel * 0.5f);
            player.armor = myStat.armor + (bonusArmorLevel * 1f);
            player.magnetRange = myStat.magnetRange + (bonusMagnetLevel * 0.5f);

            player.baseMaxHealth = myStat.maxHealth + (bonusHpLevel * 20f);

            player.UpdateMaxHealth();

            player.currentHealth = player.maxHealth;
            player.UpdateHpUI();

            Debug.Log($"적용된 스탯 - 찐체력: {player.baseMaxHealth}, 이속: {player.speed}, 아머: {player.armor}, 자석: {player.magnetRange}");
        }

        lastNextExp = nextExp[maxLevel - 1];
        StartCoroutine(GameStartRoutine());
    }

    void Update()
    {
        gameTime += Time.deltaTime;
        int min = Mathf.FloorToInt(gameTime / 60);
        int sec = Mathf.FloorToInt(gameTime % 60);
        timeText.text = string.Format("{0:D2}:{1:D2}", min, sec);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Equals))
        {
            AddGold(9999);
        }
    }
    private struct Record
    {
        public float time;
        public int kills;
        public Record(float t, int k) { time = t; kills = k; }
    }
    public void SaveBestRecords()
    {
        List<Record> records = new List<Record>();

        records.Add(new Record(gameTime, killCount));

        for (int i = 1; i <= 3; i++)
        {
            float t = PlayerPrefs.GetFloat("Rank" + i + "_Time", 0f);
            int k = PlayerPrefs.GetInt("Rank" + i + "_Kills", 0);
            if (t > 0 || k > 0) records.Add(new Record(t, k));
        }

        records.Sort((a, b) => {
            if (b.time.CompareTo(a.time) != 0) return b.time.CompareTo(a.time);
            return b.kills.CompareTo(a.kills);
        });

        for (int i = 0; i < 3 && i < records.Count; i++)
        {
            PlayerPrefs.SetFloat("Rank" + (i + 1) + "_Time", records[i].time);
            PlayerPrefs.SetInt("Rank" + (i + 1) + "_Kills", records[i].kills);
        }

        PlayerPrefs.DeleteKey("BestTime");
        PlayerPrefs.DeleteKey("BestKills");

        PlayerPrefs.Save();
    }
    public void ContinueToInfiniteMode()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        StartCoroutine(InfiniteTransitionRoutine());
    }

    IEnumerator InfiniteTransitionRoutine()
    {
        if (gameClearPanel != null) gameClearPanel.SetActive(false);

        if (infiniteModeImagePanel != null) infiniteModeImagePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);

        if (infiniteModeImagePanel != null) infiniteModeImagePanel.SetActive(false);

        Spawner.instance.StartInfiniteMode(); 
        Time.timeScale = 1f; 
    }
    IEnumerator GameStartRoutine()
    {
        Time.timeScale = 0f;

        if (startMessagePanel != null) startMessagePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(2f);

        if (startMessagePanel != null) startMessagePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        SaveBestRecords();

        Time.timeScale = 0f; 
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.GameLose);
    }

    public void GameClear()
    {
        SaveBestRecords();

        Time.timeScale = 0f;
        if (gameClearPanel != null) gameClearPanel.SetActive(true);
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.GameWin);
    }

    public void TogglePause()
    {
        if (levelUpPanel.activeSelf ||
           (startMessagePanel != null && startMessagePanel.activeSelf) ||
           (gameOverPanel != null && gameOverPanel.activeSelf) ||
           (gameClearPanel != null && gameClearPanel.activeSelf) ||
           (treasurePanel != null && treasurePanel.activeSelf) ||
           (infiniteModeImagePanel != null && infiniteModeImagePanel.activeSelf) || 
           isCinematic)
        {
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToLobby()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("LobbyScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void GiveStartingWeapon()
    {
        ItemData.ItemType baseType = GetWeaponBaseType(startingWeapon);

        itemLevels[startingWeapon] = 1;

        if (!acquiredWeapons.Contains(baseType))
        {
            acquiredWeapons.Add(baseType);
        }

        foreach (ItemData item in allItems)
        {
            if (item.itemType == startingWeapon)
            {
                ApplyItemEffect(startingWeapon, item.values[0], 1);
                break;
            }
        }

        UpdateInventoryUI();
    }
    void InitExpTable()
    {
        nextExp = new float[maxLevel];
        nextExp[0] = startExp; // 기본 100으로 시작

        for (int i = 1; i < nextExp.Length; i++)
        {
            float increment = 0f;

            if (i < 30)
            {
                increment = 150f; 
            }
            else if (i < 70)
            {
                increment = 200f;  
            }
            else
            {
                increment = 250f;
            }

            nextExp[i] = nextExp[i - 1] + increment;
        }
    }

    public void AddKill()
    {
        killCount++;
        if (killText != null) killText.text = killCount.ToString();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        if (goldText != null) goldText.text = gold.ToString();
        PlayerPrefs.SetInt("TotalGold", gold);
        PlayerPrefs.Save();
    }

    public void GetExp(float amount)
    {
        float finalExp = amount * expMultiplier;
        exp += finalExp;

        float targetExp = (level >= maxLevel) ? lastNextExp : nextExp[level - 1];

        while (exp >= targetExp)
        {
            exp -= targetExp;
            level++; 
            pendingLevelUps++;

            targetExp = (level >= maxLevel) ? lastNextExp : nextExp[level - 1];
        }

        if (pendingLevelUps > 0 && !levelUpPanel.activeSelf) ShowLevelUpUI();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (levelText != null) levelText.text = "Lv." + level;

        if (expSlider != null)
        {
            float targetExp = (level >= maxLevel) ? lastNextExp : nextExp[level - 1];
            expSlider.value = exp / targetExp;
        }
    }

    // 🌟 1. 카드 선택 함수 (더블 클릭 방지 및 시각적 리셋 추가)
    public void OnSkillSelected(int cardIndex)
    {
        if (pendingLevelUps <= 0) return; // 미친듯한 연타로 인한 더블 클릭 완벽 방어!

        SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        ItemData chosenItem = selectedCards[cardIndex];

        bool isLimitBreak = (chosenItem.itemType == ItemData.ItemType.LimitBreak_Damage ||
                             chosenItem.itemType == ItemData.ItemType.LimitBreak_Heal ||
                             chosenItem.itemType == ItemData.ItemType.LimitBreak_Gold ||
                             chosenItem.itemType == ItemData.ItemType.LimitBreak_Armor ||
                             chosenItem.itemType == ItemData.ItemType.LimitBreak_MaxHp);

        if (isLimitBreak)
        {
            ApplyItemEffect(chosenItem.itemType, 0f, 0);
        }
        else
        {
            ItemData.ItemType baseType = GetWeaponBaseType(chosenItem.itemType);
            bool isPassive = (chosenItem.itemType == ItemData.ItemType.Passive_Boots ||
                              chosenItem.itemType == ItemData.ItemType.Passive_Armor ||
                              chosenItem.itemType == ItemData.ItemType.Passive_Vampire ||
                              chosenItem.itemType == ItemData.ItemType.Passive_MaxHp ||
                              chosenItem.itemType == ItemData.ItemType.Passive_Magnet ||
                              chosenItem.itemType == ItemData.ItemType.Passive_Exp);

            if (isPassive)
            {
                if (itemLevels[chosenItem.itemType] == 0 && !acquiredPassives.Contains(chosenItem.itemType))
                    acquiredPassives.Add(chosenItem.itemType);
            }
            else
            {
                if (itemLevels[baseType] == 0 && !acquiredWeapons.Contains(baseType))
                    acquiredWeapons.Add(baseType);
            }

            itemLevels[chosenItem.itemType]++;
            int newLevel = itemLevels[chosenItem.itemType];
            float statValue = chosenItem.values[newLevel - 1];

            ApplyItemEffect(chosenItem.itemType, statValue, newLevel);
        }

        pendingLevelUps--;
        levelUpPanel.SetActive(false); // 🌟 핵심: 창을 잠시 닫아서 카드가 씹히는 걸 시각적/물리적으로 막음

        if (pendingLevelUps > 0)
        {
            StartCoroutine(NextLevelUpRoutine()); // 0.1초 뒤에 다음 창 열기
        }
        else
        {
            Time.timeScale = 1f;
        }

        UpdateInventoryUI();
    }

    // 🌟 2. 부드러운 연속 레벨업을 위한 짧은 대기 코루틴
    IEnumerator NextLevelUpRoutine()
    {
        yield return new WaitForSecondsRealtime(0.1f); // 0.1초 동안 화면이 번쩍이며 다음 렙업 인지시킴
        ShowLevelUpUI();
    }

    // 🌟 3. 레벨업 패널 띄우기 (만렙 건너뛰기 버그 해결)
    void ShowLevelUpUI()
    {
        // 🌟 핵심: 한 번에 폭업을 했더라도, '현재 처리 중인 패널의 진짜 레벨'을 역산해서 보여줍니다.
        int currentPanelLevel = level - pendingLevelUps + 1;

        int currentWeapons = GetCurrentWeaponCount();
        int currentPassives = GetCurrentPassiveCount();

        List<ItemData> availableItems = new List<ItemData>();

        foreach (ItemData item in allItems)
        {
            if (itemLevels[item.itemType] >= item.maxLevel) continue;

            bool isPassive = (item.itemType == ItemData.ItemType.Passive_Boots ||
                              item.itemType == ItemData.ItemType.Passive_Armor ||
                              item.itemType == ItemData.ItemType.Passive_Vampire ||
                              item.itemType == ItemData.ItemType.Passive_MaxHp ||
                              item.itemType == ItemData.ItemType.Passive_Magnet ||
                              item.itemType == ItemData.ItemType.Passive_Exp);

            if (isPassive)
            {
                if (itemLevels[item.itemType] >= 10) continue;
                if (itemLevels[item.itemType] == 0 && currentPassives >= 3) continue;
            }
            else
            {
                if (GetWeaponTotalLevel(item.itemType) >= 25) continue;

                ItemData.ItemType baseType = GetWeaponBaseType(item.itemType);

                if (itemLevels[baseType] == 0)
                {
                    if (item.itemType != baseType) continue;
                    if (currentWeapons >= 3) continue;
                }
            }

            availableItems.Add(item);
        }

        // 🌟 만렙에 도달했거나, 일반 아이템을 모조리 다 찍어서 더 이상 나올 게 없으면 한계 돌파를 띄웁니다!
        bool isLimitBreakTime = (currentPanelLevel >= maxLevel) || (availableItems.Count == 0);

        levelUpPanel.SetActive(true);
        SoundManager.instance.PlaySFX(SoundManager.SFX.LevelUp);
        Time.timeScale = 0f;

        if (isLimitBreakTime && limitBreakItems != null && limitBreakItems.Length >= 3)
        {
            List<ItemData> lbPool = new List<ItemData>(limitBreakItems);
            for (int i = 0; i < 3; i++)
            {
                int randIndex = Random.Range(0, lbPool.Count);
                ItemData chosenLB = lbPool[randIndex];

                selectedCards[i] = chosenLB;
                uiCards[i].SetupCard(chosenLB, 0, 0);
                uiCards[i].gameObject.SetActive(true);

                lbPool.RemoveAt(randIndex);
            }
            return;
        }

        // 일반 아이템 띄우기
        for (int i = 0; i < 3; i++)
        {
            if (availableItems.Count > 0)
            {
                int randIndex = Random.Range(0, availableItems.Count);
                ItemData chosenData = availableItems[randIndex];
                selectedCards[i] = chosenData;
                availableItems.RemoveAt(randIndex);

                int statLvl = itemLevels[chosenData.itemType];
                int totalLvl = GetWeaponTotalLevel(chosenData.itemType);

                uiCards[i].SetupCard(chosenData, totalLvl, statLvl);
                uiCards[i].gameObject.SetActive(true);
            }
            else
            {
                uiCards[i].gameObject.SetActive(false);
            }
        }
    }

    void ApplyItemEffect(ItemData.ItemType type, float value, int level)
    {
        switch (type)
        {
            // --- 1. 회전검 (Orbit) ---
            case ItemData.ItemType.Orbit_Count:
                if (!orbitWeapon.gameObject.activeSelf) orbitWeapon.gameObject.SetActive(true);

                int currentOrbitCount = orbitWeapon.transform.childCount;
                int targetOrbitCount = (int)value;
                for (int i = currentOrbitCount; i < targetOrbitCount; i++)
                {
                    Instantiate(orbitKnifePrefab, orbitWeapon.transform);
                }
                orbitWeapon.ArrangeWeapons(); 
                break;
            case ItemData.ItemType.Orbit_Damage:
                foreach (WeaponHit hit in orbitWeapon.GetComponentsInChildren<WeaponHit>())
                {
                    hit.damage = value; 
                }
                break;
            case ItemData.ItemType.Orbit_Speed:
                orbitWeapon.speed = value;
                break;

            // --- 2. 이기어검 (Flying Sword) ---
            case ItemData.ItemType.Flying_Count:
                FlyingSword[] existingSwords = FindObjectsOfType<FlyingSword>();
                int currentFlyingCount = existingSwords.Length;
                int targetFlyingCount = (int)value;
                for (int i = currentFlyingCount; i < targetFlyingCount; i++)
                {
                    Instantiate(flyingSwordPrefab, player.transform.position, Quaternion.identity);
                }
                FlyingSword.RefreshAllSwords();
                break;
            case ItemData.ItemType.Flying_Damage:
                foreach (FlyingSword sword in FindObjectsOfType<FlyingSword>()) sword.damage = value;
                break;
            case ItemData.ItemType.Flying_Duration:
                foreach (FlyingSword sword in FindObjectsOfType<FlyingSword>()) sword.chaseTimeLimit = value;
                break;

            // --- 3. 새끼용 (Baby Dragon) ---
            case ItemData.ItemType.Dragon_Count:
                if (!babyDragon.gameObject.activeSelf) babyDragon.gameObject.SetActive(true);
                babyDragon.projectileCount = (int)value; 
                break;
            case ItemData.ItemType.Dragon_Damage:
                babyDragon.damage = value;
                break;
            case ItemData.ItemType.Dragon_Speed:
                babyDragon.fireRate = value;
                break;

            // --- 4. 번개 (Lightning) ---
            case ItemData.ItemType.Lightning_Count:
                if (!lightningWeapon.gameObject.activeSelf) lightningWeapon.gameObject.SetActive(true);
                lightningWeapon.count = (int)value; 
                break;
            case ItemData.ItemType.Lightning_Damage:
                lightningWeapon.damage = value;     
                break;
            case ItemData.ItemType.Lightning_Range:
                lightningWeapon.range = value;     
                break;

                //투척단검
            case ItemData.ItemType.Dagger_Count:
                if (!daggerWeapon.gameObject.activeSelf) daggerWeapon.gameObject.SetActive(true);
                daggerWeapon.count = (int)value; 
                break;
            case ItemData.ItemType.Dagger_Damage:
                daggerWeapon.damage = value;     
                break;
            case ItemData.ItemType.Dagger_Speed:
                daggerWeapon.fireRate = value;   
                break;

            // --- 6. 검기 (Sword Aura) ---
            case ItemData.ItemType.Aura_Size:
                if (!auraWeapon.gameObject.activeSelf) auraWeapon.gameObject.SetActive(true);
                auraWeapon.sizeMultiplier = value;
                break;
            case ItemData.ItemType.Aura_Damage:
                auraWeapon.damage = value;
                break;
            case ItemData.ItemType.Aura_Distance:
                auraWeapon.maxDistance = value;
                break;

            // --- 4. 패시브 스탯 --- 
            case ItemData.ItemType.Passive_Boots:
                player.speed += value; 
                break;
            case ItemData.ItemType.Passive_Armor:
                player.armor += value;
                break;
            case ItemData.ItemType.Passive_Vampire:
                vampireHealAmount += value; 
                break;
            case ItemData.ItemType.Passive_MaxHp:
                player.flatMaxHpBonus += value;
                player.UpdateMaxHealth(); 
                break;
            case ItemData.ItemType.Passive_Magnet:
                player.magnetRange += value; 
                break;
            case ItemData.ItemType.Passive_Exp:
                expMultiplier += value; 
                break;

            case ItemData.ItemType.LimitBreak_Damage:
                player.damageMultiplier += 0.01f; // 데미지 배율 1% 증가 (0.01f)
                break;

            case ItemData.ItemType.LimitBreak_Heal:
                player.Heal(player.maxHealth * 0.5f); // 최대 체력의 50% 회복
                break;

            case ItemData.ItemType.LimitBreak_Gold:
                AddGold(100); // 골드 100 획득
                break;

            case ItemData.ItemType.LimitBreak_Armor:
                player.armorMultiplier += 0.01f; // 방어력 배율 1% 증가
                break;

            case ItemData.ItemType.LimitBreak_MaxHp:
                player.maxHpMultiplier += 0.01f; // 최대 체력 배율 1% 증가
                player.UpdateMaxHealth();        // 체력 수치 갱신
                break;
        }
    }

    public int GetWeaponTotalLevel(ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemData.ItemType.Orbit_Count:
            case ItemData.ItemType.Orbit_Damage:
            case ItemData.ItemType.Orbit_Speed:
                return itemLevels[ItemData.ItemType.Orbit_Count] + itemLevels[ItemData.ItemType.Orbit_Damage] + itemLevels[ItemData.ItemType.Orbit_Speed];

            case ItemData.ItemType.Flying_Count:
            case ItemData.ItemType.Flying_Damage:
            case ItemData.ItemType.Flying_Duration:
                return itemLevels[ItemData.ItemType.Flying_Count] + itemLevels[ItemData.ItemType.Flying_Damage] + itemLevels[ItemData.ItemType.Flying_Duration];

            case ItemData.ItemType.Dragon_Count:
            case ItemData.ItemType.Dragon_Damage:
            case ItemData.ItemType.Dragon_Speed:
                return itemLevels[ItemData.ItemType.Dragon_Count] + itemLevels[ItemData.ItemType.Dragon_Damage] + itemLevels[ItemData.ItemType.Dragon_Speed];

            case ItemData.ItemType.Lightning_Count:
            case ItemData.ItemType.Lightning_Damage:
            case ItemData.ItemType.Lightning_Range:
                return itemLevels[ItemData.ItemType.Lightning_Count] + itemLevels[ItemData.ItemType.Lightning_Damage] + itemLevels[ItemData.ItemType.Lightning_Range];

            case ItemData.ItemType.Dagger_Count:
            case ItemData.ItemType.Dagger_Damage:
            case ItemData.ItemType.Dagger_Speed:
                return itemLevels[ItemData.ItemType.Dagger_Count] + itemLevels[ItemData.ItemType.Dagger_Damage] + itemLevels[ItemData.ItemType.Dagger_Speed];

            case ItemData.ItemType.Aura_Size:
            case ItemData.ItemType.Aura_Damage:
            case ItemData.ItemType.Aura_Distance:
                return itemLevels[ItemData.ItemType.Aura_Size] + itemLevels[ItemData.ItemType.Aura_Damage] + itemLevels[ItemData.ItemType.Aura_Distance];

            default:
                return itemLevels[type]; 
        }
    }

    int GetCurrentWeaponCount()
    {
        int count = 0;
        if (itemLevels[ItemData.ItemType.Orbit_Count] > 0) count++;
        if (itemLevels[ItemData.ItemType.Flying_Count] > 0) count++;
        if (itemLevels[ItemData.ItemType.Dragon_Count] > 0) count++;
        if (itemLevels[ItemData.ItemType.Lightning_Count] > 0) count++;
        if (itemLevels[ItemData.ItemType.Dagger_Count] > 0) count++;
        if (itemLevels[ItemData.ItemType.Aura_Size] > 0) count++;
        return count;
    }

    int GetCurrentPassiveCount()
    {
        int count = 0;
        if (itemLevels[ItemData.ItemType.Passive_Boots] > 0) count++;
        if (itemLevels[ItemData.ItemType.Passive_Armor] > 0) count++;
        if (itemLevels[ItemData.ItemType.Passive_Vampire] > 0) count++;
        if (itemLevels[ItemData.ItemType.Passive_MaxHp] > 0) count++;
        if (itemLevels[ItemData.ItemType.Passive_Magnet] > 0) count++;
        if (itemLevels[ItemData.ItemType.Passive_Exp] > 0) count++;
        return count;
    }

    ItemData.ItemType GetWeaponBaseType(ItemData.ItemType type)
    {
        switch (type)
        {
            case ItemData.ItemType.Orbit_Count: case ItemData.ItemType.Orbit_Damage: case ItemData.ItemType.Orbit_Speed: return ItemData.ItemType.Orbit_Count;
            case ItemData.ItemType.Flying_Count: case ItemData.ItemType.Flying_Damage: case ItemData.ItemType.Flying_Duration: return ItemData.ItemType.Flying_Count;
            case ItemData.ItemType.Dragon_Count: case ItemData.ItemType.Dragon_Damage: case ItemData.ItemType.Dragon_Speed: return ItemData.ItemType.Dragon_Count;
            case ItemData.ItemType.Lightning_Count: case ItemData.ItemType.Lightning_Damage: case ItemData.ItemType.Lightning_Range: return ItemData.ItemType.Lightning_Count;
            case ItemData.ItemType.Dagger_Count: case ItemData.ItemType.Dagger_Damage: case ItemData.ItemType.Dagger_Speed: return ItemData.ItemType.Dagger_Count;
            case ItemData.ItemType.Aura_Size: case ItemData.ItemType.Aura_Damage: case ItemData.ItemType.Aura_Distance: return ItemData.ItemType.Aura_Size;
            default: return type; 
        }
    }

    void UpdateInventoryUI()
    {
        for (int i = 0; i < weaponSlots.Length; i++) if (weaponSlots[i] != null) weaponSlots[i].ClearSlot();
        for (int i = 0; i < passiveSlots.Length; i++) if (passiveSlots[i] != null) passiveSlots[i].ClearSlot();

        for (int i = 0; i < acquiredWeapons.Count; i++)
        {
            if (i >= weaponSlots.Length) break;

            ItemData.ItemType baseType = acquiredWeapons[i];
            int totalLevel = GetWeaponTotalLevel(baseType);
            Sprite icon = GetItemIcon(baseType);

            weaponSlots[i].SetupSlot(icon, totalLevel); 
        }

        for (int i = 0; i < acquiredPassives.Count; i++)
        {
            if (i >= passiveSlots.Length) break;

            ItemData.ItemType type = acquiredPassives[i];
            int level = itemLevels[type];
            Sprite icon = GetItemIcon(type);

            passiveSlots[i].SetupSlot(icon, level); 
        }
    }

    Sprite GetItemIcon(ItemData.ItemType type)
    {
        foreach (ItemData item in allItems)
        {
            if (item.itemType == type) return item.itemIcon;
        }
        return null;
    }

    public void ShowBossWarning(Transform bossTransform)
    {
        StartCoroutine(BossWarningRoutine(bossTransform));
    }

    IEnumerator BossWarningRoutine(Transform bossTransform)
    {
        isCinematic = true;
        Time.timeScale = 0f;
        if (bossWarningPanel != null) bossWarningPanel.SetActive(true);

        Camera mainCam = Camera.main;
        Vector3 originalCamPos = mainCam.transform.position; 
        Vector3 bossCamPos = new Vector3(bossTransform.position.x, bossTransform.position.y, originalCamPos.z); 

        float moveDuration = 0.5f;
        float percent = 0f;

        while (percent < 1f)
        {
            percent += Time.unscaledDeltaTime / moveDuration;

            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            mainCam.transform.position = Vector3.Lerp(originalCamPos, bossCamPos, smoothPercent);
            yield return null; 
        }

        yield return new WaitForSecondsRealtime(1f);

        float returnDuration = 0.5f;
        percent = 0f;

        while (percent < 1f)
        {
            percent += Time.unscaledDeltaTime / returnDuration;
            float smoothPercent = Mathf.SmoothStep(0f, 1f, percent);

            mainCam.transform.position = Vector3.Lerp(bossCamPos, originalCamPos, smoothPercent);
            yield return null;
        }

        if (bossWarningPanel != null) bossWarningPanel.SetActive(false);
        isCinematic = false;
        Time.timeScale = 1f;
    }

    public void ShowReviveEffect()
    {
        StartCoroutine(ReviveRoutine());
    }

    IEnumerator ReviveRoutine()
    {
        isMonsterFreeze = true;
        if (reviveDarkPanel != null) reviveDarkPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        isMonsterFreeze = false;
        if (reviveDarkPanel != null) reviveDarkPanel.SetActive(false);
    }

    public void OpenTreasureChest()
    {

        Time.timeScale = 0f; 
        isCinematic = true;  

        float dropChance = Random.Range(0f, 100f);

        if (dropChance <= 5f)
        {
            currentTreasureIndex = 5;
        }
        else
        {
            int[] normalItems = new int[] { 0, 1, 2, 3, 4, 6 };
            int randomIndex = Random.Range(0, normalItems.Length);
            currentTreasureIndex = normalItems[randomIndex];
        }

        if (treasureSprites != null && treasureSprites.Length > currentTreasureIndex)
        {
            treasureIcon.sprite = treasureSprites[currentTreasureIndex];
        }

        switch (currentTreasureIndex)
        {
            case 0:
                treasureName.text = "용사의 검";
                treasureDesc.text = "데미지 10% 상승";
                break;
            case 1:
                treasureName.text = "용사의 방패";
                treasureDesc.text = "방어력 10% 상승";
                break;
            case 2:
                treasureName.text = "용사의 갑옷";
                treasureDesc.text = "최대 체력 10% 상승";
                break;
            case 3:
                treasureName.text = "하트";
                treasureDesc.text = "최대 체력 25 상승";
                break;
            case 4:
                treasureName.text = "빵";
                treasureDesc.text = "5초마다 HP 1 회복";
                break;
            case 5:
                treasureName.text = "해골";
                treasureDesc.text = "치명적 피해 방어";
                break;
            case 6: 
                treasureName.text = "용사의 부츠";
                treasureDesc.text = "이동속도 10% 상승";
                break;
        }

        treasurePanel.SetActive(true);
    }

    public void AcquireTreasure()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        switch (currentTreasureIndex)
        {
            case 0:
                player.damageMultiplier += 0.1f; 
                break;
            case 1:
                player.armorMultiplier += 0.1f;  
                break;
            case 2:
                player.maxHpMultiplier += 0.1f;  
                player.UpdateMaxHealth();        
                break;
            case 3:
                player.flatMaxHpBonus += 25f;    
                player.UpdateMaxHealth();        
                break;
            case 4:
                player.hpRegenAmount += 1f;      
                break;
            case 5:
                player.reviveCount += 1;         
                break;
            case 6:
                player.speedMultiplier += 0.05f; 
                break;
        }

        treasureCounts[currentTreasureIndex]++;
        UpdateTreasureUI();

        treasurePanel.SetActive(false);
        Time.timeScale = 1f;
        isCinematic = false;
    }

    public void UseSkullItem()
    {
        if (treasureCounts[5] > 0) 
        {
            treasureCounts[5]--; 
            UpdateTreasureUI();  
        }
    }

    void UpdateTreasureUI()
    {
        for (int i = 0; i < treasureSlots.Length; i++)
        {
            if (treasureSlots[i] != null)
                treasureSlots[i].gameObject.SetActive(false); 
        }

        int currentSlotIndex = 0;
        for (int i = 0; i < 7; i++)
        {
            if (treasureCounts[i] > 0)
            {
                if (currentSlotIndex < treasureSlots.Length)
                {
                    treasureSlots[currentSlotIndex].gameObject.SetActive(true);
                    treasureSlots[currentSlotIndex].SetupTreasureSlot(treasureSprites[i], treasureCounts[i]);
                    currentSlotIndex++;
                }
            }
        }
    }
}
