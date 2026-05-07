using UnityEngine;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour
{
    public Image iconImage;
    public Text levelText;
    public Text descText;
    public void SetupCard(ItemData data, int totalLevel, int statLevel)
    {
        iconImage.sprite = data.itemIcon;

        if (totalLevel == 0)
        {
            levelText.text = "NEW!";
            descText.text = data.baseDesc;
        }
        else
        {
            if (statLevel >= data.maxLevel)
            {
                levelText.text = "MAX";
                descText.text = "최대 레벨에 도달했습니다.";
            }
            else
            {
                levelText.text = "Lv." + totalLevel + " -> Lv." + (totalLevel + 1);

                if (statLevel == 0)
                {
                    descText.text = data.baseDesc;
                }
                else if (data.levelUpDescs.Length > statLevel - 1)
                {
                    descText.text = data.levelUpDescs[statLevel - 1];
                }
                else
                {
                    descText.text = "스탯이 강화됩니다!";
                }
            }
        }
    }
}

/*
========================================================
[ SkillCard.cs 상세 설명서 (레벨업 카드 UI)]
1. 스크립트 역할:
   - 레벨업 시 화면에 뜨는 3장의 랜덤 선택지 카드 각각의 디자인(아이콘, 글씨)을 세팅해 주는 스크립트입니다.

2. 주요 작동 흐름 (SetupCard):
   - GameManager가 이 아이템의 정보(ItemData), 나의 전체 무기 레벨(totalLevel), 이 스탯의 현재 레벨(statLevel)을 던져줍니다.
   - 처음 먹는 거면(totalLevel == 0): "NEW!" 라고 띄우고 기본 설명(baseDesc)을 보여줍니다.
   - 만렙이면(statLevel >= maxLevel): "MAX" 라고 띄우고 만렙 도달 텍스트를 보여줍니다.
   - 그 외 일반 렙업: "Lv.1 -> Lv.2" 형태로 레벨을 갱신하고, ItemData에 배열로 적어둔 레벨업 설명글(levelUpDescs) 중 내 레벨에 맞는 줄을 찾아서 띄워줍니다.
========================================================
*/