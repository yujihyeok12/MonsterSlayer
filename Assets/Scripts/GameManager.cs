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

        if (charStats != null && charStats.Length > selectedCharIndex)
        {
            CharacterStat myStat = charStats[selectedCharIndex];

            player.maxHealth = myStat.maxHealth;
            player.speed = myStat.speed;
            player.armor = myStat.armor;
            player.magnetRange = myStat.magnetRange;

            player.currentHealth = player.maxHealth;
            if (player.hpSlider != null)
            {
                player.hpSlider.maxValue = player.maxHealth;
                player.hpSlider.value = player.currentHealth;
            }
        }

        if (SoundManager.instance != null) SoundManager.instance.PlayBGM(false);

        if (charStats != null && charStats.Length > selectedCharIndex)
        {
            CharacterStat myStat = charStats[selectedCharIndex];

            int bonusHpLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_HP", 0);
            int bonusSpeedLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_Speed", 0);
            int bonusArmorLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_Armor", 0);
            int bonusMagnetLevel = PlayerPrefs.GetInt("Char_" + selectedCharIndex + "_Magnet", 0);

            player.maxHealth = myStat.maxHealth + (bonusHpLevel * 20f);       // 1렙당 체력 20 증가
            player.speed = myStat.speed + (bonusSpeedLevel * 0.5f);           // 1렙당 이속 0.5 증가
            player.armor = myStat.armor + (bonusArmorLevel * 1f);             // 1렙당 방어력 1 증가
            player.magnetRange = myStat.magnetRange + (bonusMagnetLevel * 0.5f); // 1렙당 자석범위 0.5 증가

            player.currentHealth = player.maxHealth;
            if (player.hpSlider != null)
            {
                player.hpSlider.maxValue = player.maxHealth;
                player.hpSlider.value = player.currentHealth;
            }

            Debug.Log($"적용된 스탯 - 체력: {player.maxHealth}, 이속: {player.speed}, 아머: {player.armor}, 자석: {player.magnetRange}");
        }

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
        Time.timeScale = 0f; 
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.GameLose);
    }

    public void GameClear()
    {
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
        if (level >= maxLevel) return;

        float finalExp = amount * expMultiplier;
        exp += finalExp;

        while (level < maxLevel && exp >= nextExp[level - 1])
        {
            exp -= nextExp[level - 1];
            level++;
            pendingLevelUps++;
        }
        if (pendingLevelUps > 0 && !levelUpPanel.activeSelf)
        {
            ShowLevelUpUI();
        }
        UpdateUI();
    }

    void UpdateUI()
    {
        if (levelText != null) levelText.text = "Lv." + level;

        if (expSlider != null)
        {
            if (level < maxLevel) expSlider.value = exp / nextExp[level - 1];
            else expSlider.value = 1f;
        }
    }

    void ShowLevelUpUI()
    {
        int currentWeapons = GetCurrentWeaponCount();
        int currentPassives = GetCurrentPassiveCount();

        List<ItemData> availableItems = new List<ItemData>();

        foreach (ItemData item in allItems)
        {
            if (itemLevels[item.itemType] >= item.maxLevel) continue;

            // 패시브인지 무기인지 판별
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

        if (availableItems.Count == 0)
        {
            return;
        }

        levelUpPanel.SetActive(true);
        SoundManager.instance.PlaySFX(SoundManager.SFX.LevelUp);
        Time.timeScale = 0f;

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

    public void OnSkillSelected(int cardIndex)
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        ItemData chosenItem = selectedCards[cardIndex];
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

        pendingLevelUps--;

        if (pendingLevelUps > 0)
        {
            ShowLevelUpUI();
        }
        else
        {
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        UpdateInventoryUI();
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
                player.maxHealth += value; 
                player.Heal(value);       
                break;
            case ItemData.ItemType.Passive_Magnet:
                player.magnetRange += value; 
                break;
            case ItemData.ItemType.Passive_Exp:
                expMultiplier += value; 
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

/*
====================================================================================================
[ GameManager.cs 완벽 해설서 (게임 코어/총괄 매니저)]

이 스크립트는 게임의 '두뇌'이자 '사장님'입니다. 
경험치, 레벨업, 스탯 적용, UI 갱신, 아이템 뽑기 확률, 보물상자 로직, 그리고 시간 정지 연출까지 
게임의 모든 굵직한 톱니바퀴가 맞물려 돌아가는 중심축입니다.

----------------------------------------------------------------------------------------------------
[1. 핵심 데이터 관리 (Data Management)]
이 게임의 뱀서류(로그라이트) 빌드 시스템이 돌아가는 핵심 원리입니다.

- itemLevels (Dictionary): 
  내가 현재 어떤 능력을 몇 레벨까지 찍었는지 기록하는 '메인 장부'입니다. 
  예를 들어 <Orbit_Count, 3>, <Orbit_Damage, 2> 이런 식으로 저장됩니다.
- acquiredWeapons / acquiredPassives (List):
  UI 하단 인벤토리에 순서대로 띄워주기 위해, 내가 '최초로 획득한 무기/패시브의 본체 타입'을 순서대로 담아두는 가방입니다.
- charStats (배열): 기사, 마법사 등 캐릭터별 고유 기본 스탯(체력, 이속, 자석 범위 등)을 인스펙터에서 설정해두는 곳입니다.

----------------------------------------------------------------------------------------------------
[2. 주요 작동 흐름 및 함수 상세 (Core Flows)]

👉 A. 게임 시작 및 초기화 (Start)
1. PlayerPrefs 연동: 로비에서 투자한 골드(TotalGold)와 영구 스탯 강화 수치(Char_0_HP 등)를 불러와서 캐릭터의 기본 스탯에 더해줍니다.
2. 무기 세팅: 로비에서 고른 캐릭터 번호에 맞춰 시작 무기를 쥐여줍니다. (기사=회전검, 도적=단검 등)
3. UI 초기화: 시작하자마자 보물상자 슬롯 등을 다 꺼두고, GameStartRoutine을 돌려 2초간 화면을 멈춘 채 "게임 시작" 문구를 띄웁니다.

👉 B. 경험치와 레벨업 시스템
1. InitExpTable(): 레벨 구간마다 필요한 경험치 통을 늘려줍니다. (1~30렙은 150씩, 31~70렙은 200씩, 그 이상은 250씩 증가)
2. GetExp(amount): 보석을 먹을 때마다 expMultiplier(경험치 증가 패시브)를 곱해서 더합니다. 만약 경험치가 꽉 차면 레벨을 올리고 ShowLevelUpUI()를 부릅니다.
3. ShowLevelUpUI() 🌟[이 게임에서 가장 복잡하고 중요한 로직]:
   - 무작위 3개의 카드를 뽑는 함수입니다. 하지만 아무거나 뽑지 않고 깐깐한 '필터링'을 거칩니다.
   - [필터 1] 이미 만렙(maxLevel)을 찍은 스탯은 후보에서 뺍니다.
   - [필터 2] 내가 가진 무기/패시브가 이미 3개(최대치) 꽉 찼다면, 아예 새로운 종류의 무기/패시브 카드는 후보에서 뺍니다. (먹던 것만 마저 강화하라는 뜻)
   - [필터 3] 특정 무기(예: 회전검)의 하위 스탯(데미지, 속도 등)의 레벨 총합이 25렙을 넘으면 더 이상 안 나오게 막습니다.
   - 이 필터를 모두 통과한 녀석들 중에서만 랜덤으로 3개를 뽑아 화면(uiCards)에 띄워주고 시간을 멈춥니다(Time.timeScale = 0).

👉 C. 스킬 선택 및 적용 로직
1. OnSkillSelected(cardIndex): 유저가 카드를 고르면 장부(itemLevels)에 레벨을 +1 해주고, 가방에 없는 거면 새로 담아줍니다.
2. ApplyItemEffect() 🌟[실제 스탯 갱신]:
   - 고른 카드가 '회전검 개수(Orbit_Count)'라면 프리팹을 실제로 복사(Instantiate)해서 플레이어 주변에 추가로 달아줍니다.
   - 고른 카드가 '스피드(Passive_Boots)'라면 즉시 player.speed 값을 올려줍니다.
3. 헬퍼 함수들 (GetWeaponTotalLevel, GetWeaponBaseType):
   - '회전검'이라는 하나의 무기를 완성하려면 (개수+데미지+속도) 3개의 스크립터블 오브젝트(SO)가 필요합니다. 이 3개의 레벨을 합쳐서(TotalLevel) 화면 하단 UI 슬롯 1칸에 합산해서 보여주기 위해 묶어주는 역할을 합니다.

👉 D. 보물상자 시스템
1. OpenTreasureChest(): 몬스터가 1% 확률로 떨군 상자를 먹으면 실행됩니다. 
   - 일반 레벨업과 달리 5% 확률로 대박 아이템(해골)이 나오거나, 95% 확률로 일반 스탯업 아이템(하트, 부츠 등)이 뽑히게 룰렛을 돌립니다.
2. AcquireTreasure(): 뽑힌 보상에 따라 배율(multiplier)을 영구적으로 올려줍니다.
3. UpdateTreasureUI() 🌟[하노이 탑 중앙 정렬 UI]:
   - 먹은 아이템이 1개면 중앙, 2개면 양옆으로 쫙 퍼지는 예쁜 UI를 만들기 위해, 무조건 '모든 칸을 비활성화(SetActive(false))'한 다음, 내가 먹은 개수만큼만 딱 켜줍니다(SetActive(true)). (이렇게 해야 유니티 Layout Group이 알아서 중앙으로 모아줍니다)

👉 E. 카메라 및 특수 연출 (시간 정지 연출법)
- BossWarningRoutine() & ReviveRoutine():
  시간이 정지된 상태(Time.timeScale = 0)에서는 일반적인 Time.deltaTime이나 애니메이션이 작동하지 않습니다. 
  그래서 이 코루틴들 안에서는 현실의 시간인 `Time.unscaledDeltaTime`을 사용해서 카메라를 스르륵 이동시키거나 화면을 어둡게 만드는 시네마틱 연출을 구현했습니다.

----------------------------------------------------------------------------------------------------
[핵심 요약]
- 새로운 무기를 추가하고 싶다면? 
  -> ItemData에 열거형(Enum)을 추가하고, 이 스크립트의 ApplyItemEffect()와 GetWeaponTotalLevel() 등에 case를 추가해주면 끝납니다.
- 밸런스를 고치고 싶다면?
  -> 무기별 수치는 유니티 에디터의 Scriptable Object에서, 레벨업 요구량은 InitExpTable()에서 수정하세요.
====================================================================================================
*/