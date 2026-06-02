using UnityEngine;

public class ThrowingDaggerWeapon : MonoBehaviour
{
    [Header("단검 스탯")]
    public float damage = 5f;
    public int count = 1;           
    public float fireRate = 1.5f;  
    public float projectileSpeed = 15f; 
    public float spreadAngle = 15f; 
    public float spriteAngleOffset = 90f;

    [Header("연결")]
    public Player player;
    public GameObject daggerPrefab;

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
            FireDaggers();
            timer = 0f;
        }
    }

    void FireDaggers()
    {
        float baseAngle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;

        SoundManager.instance.PlaySFX(SoundManager.SFX.DaggerThrow);

        for (int i = 0; i < count; i++)
        {
            float offsetAngle = 0f;
            if (count > 1)
            {
                offsetAngle = spreadAngle * (i - (count - 1) / 2f);
            }

            float finalAngle = baseAngle + offsetAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, finalAngle + spriteAngleOffset);

            GameObject dagger = Instantiate(daggerPrefab, transform.position, rotation);

            ThrowingDagger projectile = dagger.GetComponent<ThrowingDagger>();
            if (projectile != null)
            {
                projectile.damage = damage;
                projectile.speed = projectileSpeed;
                projectile.dir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));
            }
        }
    }
}