using UnityEngine;

public class Fireball : MonoBehaviour
{
    private float damage;
    private float speed;
    private float aliveTimer; 

    public void Init(float newDamage, float newSpeed)
    {
        damage = newDamage;
        speed = newSpeed;
    }

    void OnEnable()
    {
        aliveTimer = 0f;
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        aliveTimer += Time.deltaTime;
        if (aliveTimer >= 3f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.health > 0)
            {
                float finalDamage = damage * GameManager.instance.player.damageMultiplier;
                enemy.TakeDamage(finalDamage);
            }

            gameObject.SetActive(false);
        }
    }
}
