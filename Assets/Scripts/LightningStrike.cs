using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public Enemy targetEnemy;

    void Start()
    {
        if (targetEnemy != null)
        {
            float finalDamage = damage * GameManager.instance.player.damageMultiplier;
            targetEnemy.TakeDamage(finalDamage);
        }

        Destroy(gameObject, 0.5f);
    }
}

/*
========================================================
[ LightningStrike.cs 상세 설명서 (떨어지는 번개 본체)]
1. 스크립트 역할:
   - 타겟의 머리 위에 생성되어 벼락을 내리꽂는 실제 '투사체' 역할을 합니다.

2. 주요 변수 및 흐름:
   - targetEnemy & damage: 번개를 소환하는 쪽(LightningWeapon)에서 생성 직후 이 변수들에 타겟과 데미지를 강제로 집어넣어 줍니다.
   - Start(): 맵에 생성되자마자 타겟에게 데미지 배율(용사의 검 등)을 곱한 최종 데미지를 꽂아 넣고, 0.5초(번개 이펙트가 끝날 시간) 뒤에 자기 자신을 파괴합니다.
========================================================
*/