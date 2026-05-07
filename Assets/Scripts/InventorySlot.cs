using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;    
    public Text levelText;     

    public void SetupSlot(Sprite icon, int level)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = new Color(1, 1, 1, 1);
        }
        if (levelText != null)
        {
            levelText.text = "Lv." + level;
            levelText.gameObject.SetActive(true);
        }
    }

    public void SetupTreasureSlot(Sprite icon, int count)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = new Color(1, 1, 1, 1); 
        }
        if (levelText != null)
        {
            levelText.text = "x" + count;
            levelText.gameObject.SetActive(true);
        }
    }

    public void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
        }
        if (levelText != null)
        {
            levelText.text = "";
            levelText.gameObject.SetActive(false);
        }
    }
}

/*
========================================================
[ InventorySlot.cs 상세 설명서 (인벤토리 1칸 UI)]
1. 스크립트 역할:
   - 게임 화면 하단이나 일시정지 창에 보이는 '내가 먹은 아이템 목록' 중 딱 1칸의 그림과 글씨를 통제합니다.

2. 주요 함수:
   - SetupSlot(icon, level): 일반 무기/패시브용입니다. 전달받은 이미지를 띄우고 텍스트를 "Lv.2" 형태로 갱신합니다.
   - SetupTreasureSlot(icon, count): 보물상자 아이템용입니다. 레벨이 아니라 개수이므로 "x 2" 형태로 갱신합니다.
   - ClearSlot(): 빈칸으로 만들 때 씁니다. 아이콘의 투명도(Alpha)를 0으로 만들어서 그림을 안 보이게 하고 글씨도 지워버립니다.
========================================================
*/