using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    [Header("무기 설정")]
    public float damage = 5f; 

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (enemy != null)
            {
                float finalDamage = damage * GameManager.instance.player.damageMultiplier;
                enemy.TakeDamage(finalDamage);
            }
        }
    }
}
/*
========================================================
[WeaponHit.cs 설명서 (무기 타격 판정)]
1. 이 스크립트의 역할:
   - 실제로 몬스터와 부딪혀서 아프게 때리는 '칼날' 역할입니다. 회전검 각각의 프리팹에 붙어있습니다.

2. 주요 변수:
   - damage: 이 무기가 몬스터에게 줄 데미지

3. 주요 함수:
   - OnTriggerEnter2D(): 유니티의 충돌 감지 함수입니다. 내 칼날에 무언가 닿았을 때 실행됩니다.

4. 작동 흐름:
   - 빙글빙글 돌다가 태그가 "Enemy"인 몬스터와 닿으면 -> 몬스터 스크립트를 가져와서 -> TakeDamage() 함수를 실행해 피를 깎습니다.
========================================================
*/