using UnityEngine;

public class SwordAuraProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public float maxDistance;
    [HideInInspector] public Vector2 dir;

    private Vector2 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);

        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

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