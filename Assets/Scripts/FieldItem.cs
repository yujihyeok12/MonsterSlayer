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

/*
========================================================
[📝 FieldItem.cs 설명서 (고기와 자석)]
1. 이 스크립트의 역할:
   - 몬스터가 낮은 확률로 떨어뜨리는 '즉시 회복 고기(Heal)'와 '전체 화면 자석(Magnet)' 아이템입니다.

2. 주요 변수:
   - type: 이 아이템이 고기인지 자석인지 구분합니다.
   - healAmount: 고기일 경우 채워줄 체력량

3. 주요 함수:
   - OnTriggerEnter2D(): 플레이어가 이 아이템을 밟았을 때 발동합니다. 고기면 플레이어 체력을 채워주고, 자석이면 맵에 있는 모든 보석(ExpGem)에게 "당장 플레이어한테 날아가!" 라고 명령(StartGlobalMagnet)을 내립니다.
========================================================
*/