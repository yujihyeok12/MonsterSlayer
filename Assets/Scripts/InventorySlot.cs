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