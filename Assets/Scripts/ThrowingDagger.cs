using UnityEngine;

public class ThrowingDagger : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public Vector2 dir;

    void Start()
    {
        if (dir.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        Destroy(gameObject, 1f);
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
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
            Destroy(gameObject);
        }
    }
}
