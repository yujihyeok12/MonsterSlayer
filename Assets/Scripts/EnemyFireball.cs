using UnityEngine;

public class EnemyFireball : MonoBehaviour
{
    public float damage = 20f;
    public float speed = 10f;

    private Vector2 moveDir;
    public void Setup(Vector2 dir)
    {
        moveDir = dir;
    }

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "player" || collision.CompareTag("Player"))
        {
            Player p = collision.GetComponent<Player>();
            if (p != null) p.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
