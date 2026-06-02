using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    [Header("무기 설정")]
    public float damage = 5f; 

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