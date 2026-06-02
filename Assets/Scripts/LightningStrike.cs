using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public Enemy targetEnemy;

    void Start()
    {
        if (targetEnemy != null)
        {
            float finalDamage = damage * GameManager.instance.player.damageMultiplier;
            targetEnemy.TakeDamage(finalDamage);
        }

        Destroy(gameObject, 0.5f);
    }
}