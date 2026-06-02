using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI 패널 설정")]
    public GameObject mainPanel;
    public GameObject characterSelectPanel;
    public GameObject growthPanel;
    public GameObject characterStatPanel;

    [Header("재화 UI")]
    public Text goldText;
    private int currentGold;

    [Header("캐릭터 버튼 세팅")]
    public Button[] selectButtons;
    public Text[] priceTexts;

    [Header("--- 성장 시스템 UI ---")]
    public Text growthRemainPointText;
    public Text buyPointCostText;

    public GameObject lockedCharPopup;

    private int selectedGrowthIndex = 0;
    private const int MAX_POINTS = 20;

    [Header("--- 스탯 레벨 텍스트 ---")]
    public Text hpLevelText;
    public Text speedLevelText;
    public Text armorLevelText;
    public Text magnetLevelText;

    private const int MAX_STAT_LEVEL = 5;

    [Header("--- 선택된 캐릭터 이미지 ---")]
    public Image selectedCharacterDisplay;
    public Sprite[] characterPortraits;

    [Header("--- 포인트 구매 팝업 ---")]
    public GameObject pointBuyConfirmPanel;
    public Text confirmCostText;
    private int currentCalculatedCost = 0;

    [Header("팝업 UI")]
    public GameObject notEnoughGoldPanel;

    [Header("--- 최고 기록 UI ---")]
    public Text bestRecordText;  

    private int[] charPrices = { 0, 0, 2000 };

    void Start()
    {
        mainPanel.SetActive(true);
        characterSelectPanel.SetActive(false);

        currentGold = PlayerPrefs.GetInt("TotalGold", 0);
        UpdateGoldUI();
        PlayerPrefs.SetInt("CharUnlocked_0", 1);
        UpdateCharacterUI();
        DisplayBestRecords();

        if (SoundManager.instance != null) SoundManager.instance.PlayBGM(false);
    }

    void DisplayBestRecords()
    {
        if (bestRecordText == null) return;

        string rankString = "명예의 전당\n\n";

        for (int i = 1; i <= 3; i++)
        {
            float t = PlayerPrefs.GetFloat("Rank" + i + "_Time", 0f);
            int k = PlayerPrefs.GetInt("Rank" + i + "_Kills", 0);

            if (t == 0 && k == 0)
            {
                rankString += $"{i}위: 기록 없음\n";
            }
            else
            {
                int min = Mathf.FloorToInt(t / 60);
                int sec = Mathf.FloorToInt(t % 60);
                rankString += $"{i}위: {min:D2}:{sec:D2} / {k} Kills\n";
            }
        }

        bestRecordText.text = rankString;
    }

    public void OpenCharacterSelect()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        mainPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
    }

    public void CloseCharacterSelect()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        characterSelectPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void UpdateGoldUI()
    {
        if (goldText != null) goldText.text = currentGold.ToString() + " G";
    }

    void UpdateCharacterUI()
    {
        for (int i = 0; i < selectButtons.Length; i++)
        {
            bool isUnlocked = PlayerPrefs.GetInt("CharUnlocked_" + i, 0) == 1;
            int index = i;

            selectButtons[i].onClick.RemoveAllListeners();

            if (isUnlocked)
            {
                priceTexts[i].text = "선택 (시작)";
                selectButtons[i].onClick.AddListener(() => StartGameWithCharacter(index));
            }
            else
            {
                priceTexts[i].text = charPrices[i] + " G (구매)";
                selectButtons[i].onClick.AddListener(() => BuyCharacter(index));
            }
        }
    }

    public void BuyCharacter(int index)
    {
        if (currentGold >= charPrices[index])
        {
            currentGold -= charPrices[index];
            PlayerPrefs.SetInt("TotalGold", currentGold);
            PlayerPrefs.SetInt("CharUnlocked_" + index, 1);
            PlayerPrefs.Save();

            if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

            UpdateGoldUI();
            UpdateCharacterUI();
        }
        else
        {
            if (notEnoughGoldPanel != null) notEnoughGoldPanel.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        if (notEnoughGoldPanel != null) notEnoughGoldPanel.SetActive(false);
    }

    public void CloseLockedCharPopup()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        if (lockedCharPopup != null) lockedCharPopup.SetActive(false);
    }

    public void StartGameWithCharacter(int index)
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        PlayerPrefs.SetInt("SelectedCharacter", index);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }

    public void OpenGrowthPanel()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        mainPanel.SetActive(false);
        growthPanel.SetActive(true);
    }

    public void CloseGrowthPanel()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        growthPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void SelectGrowthCharacter(int index)
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        bool isUnlocked = PlayerPrefs.GetInt("CharUnlocked_" + index, 0) == 1;
        if (index == 0) isUnlocked = true;

        if (!isUnlocked)
        {
            if (lockedCharPopup != null) lockedCharPopup.SetActive(true);
            return;
        }

        selectedGrowthIndex = index;

        if (growthPanel != null) growthPanel.SetActive(false);
        if (characterStatPanel != null) characterStatPanel.SetActive(true);

        UpdateGrowthUI();
    }

    public void UpdateGrowthUI()
    {
        int boughtPoints = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_BoughtPoints", 0);

        int hp = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_HP", 0);
        int speed = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_Speed", 0);
        int armor = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_Armor", 0);
        int magnet = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_Magnet", 0);

        int usedPoints = hp + speed + armor + magnet;
        int remainPoints = boughtPoints - usedPoints;

        if (growthRemainPointText != null) growthRemainPointText.text = "보유 포인트: " + remainPoints;

        if (buyPointCostText != null) buyPointCostText.text = "보유 골드: " + currentGold + " G";

        if (hpLevelText != null) hpLevelText.text = "Lv." + hp;
        if (speedLevelText != null) speedLevelText.text = "Lv." + speed;
        if (armorLevelText != null) armorLevelText.text = "Lv." + armor;
        if (magnetLevelText != null) magnetLevelText.text = "Lv." + magnet;

        if (selectedCharacterDisplay != null && characterPortraits.Length > selectedGrowthIndex)
        {
            selectedCharacterDisplay.sprite = characterPortraits[selectedGrowthIndex];
        }
    }

    public void CloseCharacterStatPanel()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        if (characterStatPanel != null) characterStatPanel.SetActive(false);
        if (growthPanel != null) growthPanel.SetActive(true);
    }

    public void IncreaseStat(int statType)
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        int boughtPoints = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_BoughtPoints", 0);
        int usedPoints =
            PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_HP", 0) +
            PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_Speed", 0) +
            PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_Armor", 0) +
            PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_Magnet", 0);

        int remainPoints = boughtPoints - usedPoints;

        if (remainPoints <= 0) return;

        string key = GetStatKey(statType);
        int currentStatLevel = PlayerPrefs.GetInt(key, 0);

        if (currentStatLevel >= MAX_STAT_LEVEL) return;

        PlayerPrefs.SetInt(key, currentStatLevel + 1);
        PlayerPrefs.Save();

        UpdateGrowthUI();
    }

    public void DecreaseStat(int statType)
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        string key = GetStatKey(statType);
        int currentStatLevel = PlayerPrefs.GetInt(key, 0);

        if (currentStatLevel <= 0) return;

        PlayerPrefs.SetInt(key, currentStatLevel - 1);
        PlayerPrefs.Save();

        UpdateGrowthUI();
    }

    private string GetStatKey(int statType)
    {
        string statName = "";
        switch (statType)
        {
            case 0: statName = "_HP"; break;
            case 1: statName = "_Speed"; break;
            case 2: statName = "_Armor"; break;
            case 3: statName = "_Magnet"; break;
        }
        return "Char_" + selectedGrowthIndex + statName;
    }

    public void OpenBuyConfirmPopup()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        int boughtPoints = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_BoughtPoints", 0);

        if (boughtPoints >= MAX_POINTS) return;

        currentCalculatedCost = 100 + (boughtPoints * 100);

        if (confirmCostText != null)
        {
            confirmCostText.text = currentCalculatedCost + " G를 사용하여\n스탯 포인트를 구매하시겠습니까?";
        }

        if (pointBuyConfirmPanel != null) pointBuyConfirmPanel.SetActive(true);
    }

    public void ConfirmBuyPoint()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

        if (currentGold >= currentCalculatedCost)
        {
            currentGold -= currentCalculatedCost;
            int boughtPoints = PlayerPrefs.GetInt("Char_" + selectedGrowthIndex + "_BoughtPoints", 0);
            boughtPoints++;

            PlayerPrefs.SetInt("TotalGold", currentGold);
            PlayerPrefs.SetInt("Char_" + selectedGrowthIndex + "_BoughtPoints", boughtPoints);
            PlayerPrefs.Save();

            if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.LevelUp);

            UpdateGoldUI();
            UpdateGrowthUI();

            if (pointBuyConfirmPanel != null) pointBuyConfirmPanel.SetActive(false);
        }
        else
        {
            if (pointBuyConfirmPanel != null) pointBuyConfirmPanel.SetActive(false);
            if (notEnoughGoldPanel != null) notEnoughGoldPanel.SetActive(true);
        }
    }

    public void CancelBuyPoint()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);
        if (pointBuyConfirmPanel != null) pointBuyConfirmPanel.SetActive(false);
    }

    public void QuitGame()
    {
        if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.Click);

#if UNITY_EDITOR
        Debug.Log("게임 종료");
#else
        Application.Quit();
#endif
    }
}