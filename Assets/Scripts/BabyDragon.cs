using UnityEngine;

public class BabyDragon : MonoBehaviour
{
    [Header("발사체(불덩이) 설정")]
    public GameObject fireballPrefab; 
    public float damage = 5f;
    public float projectileSpeed = 10f; 
    public int projectileCount = 1;     
    public float spreadAngle = 30f;     

    [Header("전투 설정")]
    public float fireRate = 1f;       
    public float detectRadius = 8f;   

    [Header("등 뒤 호위 설정")]
    public float backDistance = 1.2f;
    public float heightOffset = 0.5f; 

    private Transform player;
    private float timer = 0f;

    void Start()
    {
        player = GameObject.Find("player").transform;
    }

    void Update()
    {
        if (player == null) return;

        float playerDir = Mathf.Sign(player.localScale.x);
        Vector3 waitPos = player.position + new Vector3(-playerDir * backDistance, heightOffset, 0);

        transform.position = Vector3.Lerp(transform.position, waitPos, 8f * Time.deltaTime);

        transform.localScale = new Vector3(playerDir, 1, 1);

        timer += Time.deltaTime;
        if (timer >= 1f / fireRate) 
        {
            FindAndShoot();
        }
    }

    void FindAndShoot()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        float closestDist = Mathf.Infinity;
        Transform target = null;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy") && col.gameObject.activeSelf)
            {
                Enemy e = col.GetComponent<Enemy>();
                if (e != null && e.health > 0)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = col.transform;
                    }
                }
            }
        }

        if (target != null)
        {
            ShootFireball(target);
            timer = 0f; 
        }
    }

    void ShootFireball(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float startAngle = projectileCount > 1 ? -spreadAngle / 2f : 0f;
        float angleStep = projectileCount > 1 ? spreadAngle / (projectileCount - 1) : 0f;

        SoundManager.instance.PlaySFX(SoundManager.SFX.DragonFire);

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float finalAngle = baseAngle + currentAngle;

            GameObject fireball = PoolManager.instance.Get(4);

            fireball.transform.position = transform.position;
            fireball.transform.rotation = Quaternion.Euler(0, 0, finalAngle);

            fireball.GetComponent<Fireball>().Init(damage, projectileSpeed);
        }
    }
}

/*
========================================================
[ BabyDragon.cs 설명서 (새끼용 펫)]
1. 이 스크립트의 역할:
   - 플레이어의 등 뒤를 졸졸 따라다니며, 주변에 몬스터가 있으면 알아서 불덩이를 뱉는 똑똑한 펫입니다.

2. 주요 변수:
   - projectileCount: 한 번에 뱉는 불덩이 개수 (레벨업해서 늘어나면 샷건처럼 여러 갈래로 쏩니다!)
   - detectRadius: 몬스터를 탐지하는 레이더 범위
   - backDistance: 플레이어 뒤로 얼마나 떨어져서 따라다닐지 거리

3. 주요 함수:
   - Update(): 플레이어가 왼쪽을 보면 용도 왼쪽을 보며 등 뒤로 위치를 부드럽게(Lerp) 이동합니다. 타이머를 재서 쏠 때가 되면 FindAndShoot()을 부릅니다.
   - FindAndShoot(): 내 레이더(detectRadius) 안의 몬스터를 싹 뒤져서 가장 가까운 놈을 타겟으로 잡습니다.
   - ShootFireball(): 타겟을 향해 각도를 계산하고, PoolManager(창고)에서 불덩이를 꺼내와 부채꼴 모양으로 흩뿌려 쏩니다.

4. 작동 흐름:
   - 졸졸 따라다님 -> 타이머 참 -> 주변 적 탐색 -> 가장 가까운 적을 향해 발사각 계산 -> 불덩이 소환 및 스탯 전달!
========================================================
*/