using UnityEngine;
using System.Collections.Generic;

public class LightningWeapon : MonoBehaviour
{
    [Header("번개 스탯")]
    public float damage = 10f;       
    public int count = 1;           
    public float range = 5f;         
    public float attackInterval = 1f; 

    [Header("연결")]
    public Transform player;        
    public GameObject lightningPrefab; 


    public float yOffset = 1.5f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= attackInterval)
        {
            StrikeLightning();
            timer = 0f;
        }
    }

    void StrikeLightning()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.position, range);
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach (Collider2D coll in colliders)
        {
            if (coll.CompareTag("Enemy"))
            {
                Enemy e = coll.GetComponent<Enemy>();
                if (e != null && e.health > 0)
                {
                    enemiesInRange.Add(e);
                }
            }
        }

        if (enemiesInRange.Count == 0) return;

        SoundManager.instance.PlaySFX(SoundManager.SFX.Lightning);

        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            Enemy temp = enemiesInRange[i];
            int randomIndex = Random.Range(i, enemiesInRange.Count);
            enemiesInRange[i] = enemiesInRange[randomIndex];
            enemiesInRange[randomIndex] = temp;
        }

        int strikeCount = Mathf.Min(count, enemiesInRange.Count);

        for (int i = 0; i < strikeCount; i++)
        {
            Enemy target = enemiesInRange[i];

            Vector3 spawnPos = target.transform.position + new Vector3(0, yOffset, 0);

            GameObject bolt = Instantiate(lightningPrefab, spawnPos, Quaternion.identity);

            LightningStrike strikeScript = bolt.GetComponent<LightningStrike>();
            if (strikeScript != null)
            {
                strikeScript.damage = damage;
                strikeScript.targetEnemy = target;
            }
        }
    }
}

/*
========================================================
[ LightningWeapon.cs 상세 설명서 (번개 마법 매니저)]
1. 스크립트 역할:
   - 번개를 쏘는 마법사 본체입니다. 일정 쿨타임마다 주변 몬스터를 스캔해서 랜덤으로 번개를 꽂습니다.

2. 핵심 작동 흐름 및 함수:
   - StrikeLightning() [🌟스캔 및 색인]:
     1) OverlapCircleAll로 내 사거리(range) 안에 있는 모든 적을 찾습니다.
     2) 찾은 적들을 리스트(enemiesInRange)에 담은 뒤, for문을 돌려서 리스트의 순서를 무작위로 뒤죽박죽 섞어버립니다. (골고루 번개를 맞게 하기 위함)
     3) 섞인 리스트의 맨 앞쪽부터 내가 쏠 수 있는 번개 개수(count)만큼 타겟을 골라 번개(LightningStrike) 프리팹을 타겟 머리 위(yOffset)에 소환하고 데미지와 타겟 정보를 전달해 줍니다.
========================================================
*/
