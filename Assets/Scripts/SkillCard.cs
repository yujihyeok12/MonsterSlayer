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
