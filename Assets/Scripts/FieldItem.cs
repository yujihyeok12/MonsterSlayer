using UnityEngine;

public class FieldItem : MonoBehaviour
{
    public enum ItemType { Heal, Magnet }

    [Header("아이템 설정")]
    public ItemType type;
    public float healAmount = 30f; 

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "player" || collision.CompareTag("Player"))
        {
            if (type == ItemType.Heal)
            {
                GameManager.instance.player.Heal(healAmount);
                SoundManager.instance.PlaySFX(SoundManager.SFX.GetHeal);
            }
            else if (type == ItemType.Magnet)
            {
                ExpGem[] allGems = FindObjectsOfType<ExpGem>();
                foreach (ExpGem gem in allGems)
                {
                    gem.StartGlobalMagnet();
                }
            }

            Destroy(gameObject);
        }
    }
}
