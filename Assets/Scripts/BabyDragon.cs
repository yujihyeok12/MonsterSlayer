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

