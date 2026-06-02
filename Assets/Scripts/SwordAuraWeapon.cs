using UnityEngine;

public class SwordAuraWeapon : MonoBehaviour
{
    [Header("검기 스탯")]
    public float damage = 10f;
    public float sizeMultiplier = 1f;   // 최대 2배까지 커질 크기
    public float maxDistance = 5f;      // 날아갈 최대 거리

    public float fireRate = 1.5f;       // 발사 쿨타임
    public float projectileSpeed = 15f; // 날아가는 속도
    public float spreadAngle = 20f;     // 3갈래가 퍼지는 각도
    public float spriteAngleOffset = 0f;// 이미지 기본 각도 보정용

    [Header("연결")]
    public Player player;
    public GameObject auraPrefab;

    private float timer;
    private Vector2 lastDir = Vector2.right;

    void Update()
    {
        if (player.inputVec != Vector2.zero)
        {
            lastDir = player.inputVec.normalized;
        }

        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            FireAura();
            timer = 0f;
        }
    }

    void FireAura()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlaySFX(SoundManager.SFX.DaggerThrow);

        float baseAngle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;

        for (int i = 0; i < 3; i++)
        {
            float offsetAngle = spreadAngle * (i - 1);
            float finalAngle = baseAngle + offsetAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, finalAngle + spriteAngleOffset);

            GameObject aura = Instantiate(auraPrefab, transform.position, rotation);

            aura.transform.localScale = Vector3.one * sizeMultiplier;

            SwordAuraProjectile projectile = aura.GetComponent<SwordAuraProjectile>();
            if (projectile != null)
            {
                projectile.damage = damage;
                projectile.speed = projectileSpeed;
                projectile.maxDistance = maxDistance;
                projectile.dir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            }
        }
    }
}
