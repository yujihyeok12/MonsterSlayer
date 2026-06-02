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

