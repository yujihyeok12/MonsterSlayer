using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("몬스터 설정")]
    public float speed = 3f;
    public float maxHealth = 10f;
    public float health;
    public float knockbackPower = 5f;
    public float damage = 10f;

    [Header("보스 설정")]
    public bool isBoss = false;
    public bool isFinalBoss = false;
    public GameObject enemyFireballPrefab;
    public float fireballCooldown = 1.5f;
    public float spriteAngleOffset = 90f;
    public GameObject blackGemPrefab;
    public float bossDropExp = 1000f;

    private float fireballTimer = 0f;
    private bool isDead = false;
    private bool isKnockback = false;

    private Rigidbody2D target;
    private Rigidbody2D rigid;
    private Animator anim;
    private Collider2D coll;

    private WaitForSeconds knockbackTime = new WaitForSeconds(0.1f);
    private Vector3 originalScale;

    [Header("드랍 아이템 프리팹 (일반 몬스터용)")]
    public GameObject healItemPrefab;
    public GameObject magnetItemPrefab;
    public GameObject treasureChestPrefab;
    public float dropExpAmount = 10f;
    public int expGemPoolIndex = 5;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();

        if (Spawner.instance != null && Spawner.instance.isInfiniteMode)
        {
            ApplyInfiniteStats();
        }
        else
        {
            health = maxHealth;
        }

        isDead = false;
        isKnockback = false;
        anim.SetBool("Dead", false);
        coll.enabled = true;
    }

    public void ApplyInfiniteStats()
    {
        if (isBoss) return;

        maxHealth = Spawner.instance.GetCurrentInfiniteHealth();
        health = maxHealth;
        speed = Spawner.instance.GetCurrentInfiniteSpeed();
        damage = Spawner.instance.GetCurrentInfiniteDamage();

        GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0.5f);
    }

    void Update()
    {
        if (isDead || target == null) return;

        if (GameManager.instance != null && GameManager.instance.isMonsterFreeze)
        {
            anim.speed = 0f;
            return;
        }
        else
        {
            anim.speed = 1f;
        }

        if (isBoss && enemyFireballPrefab != null)
        {
            fireballTimer += Time.deltaTime;
            if (fireballTimer >= fireballCooldown)
            {
                ShootFireballFromMap();
                fireballTimer = 0f;
            }
        }
    }

    void ShootFireballFromMap()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = target.position + (randomDir * 20f);
        Vector2 dirToPlayer = (target.position - spawnPos).normalized;

        float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        GameObject fireball = Instantiate(enemyFireballPrefab, (Vector3)spawnPos, Quaternion.Euler(0, 0, baseAngle + spriteAngleOffset));

        EnemyFireball fbScript = fireball.GetComponent<EnemyFireball>();
        if (fbScript != null) fbScript.Setup(dirToPlayer);
    }

    void FixedUpdate()
    {
        if (target == null || isDead)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        if (GameManager.instance != null && GameManager.instance.isMonsterFreeze)
        {
            rigid.linearVelocity = Vector2.zero;
            return;
        }

        if (isKnockback) return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed;
        rigid.linearVelocity = nextVec;

        if (!isBoss && dirVec.magnitude > 40f)
        {
            gameObject.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (target == null || isDead) return;

        transform.localScale = new Vector3(
            target.position.x < transform.position.x ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x),
            originalScale.y,
            originalScale.z
        );
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            SoundManager.instance.PlaySFX(SoundManager.SFX.MonsterHit);
            if (!isBoss) StartCoroutine(KnockBack());
        }
        else
        {
            Die();
        }
    }

    IEnumerator KnockBack()
    {
        isKnockback = true;
        Vector2 pushDir = rigid.position - target.position;
        rigid.linearVelocity = pushDir.normalized * knockbackPower;
        yield return knockbackTime;
        isKnockback = false;
    }

    void Die()
    {
        isDead = true;
        anim.SetBool("Dead", true);
        coll.enabled = false;

        GameManager.instance.AddKill();

        if (isBoss)
        {
            if (blackGemPrefab != null)
            {
                GameObject gem = Instantiate(blackGemPrefab, transform.position, Quaternion.identity);
                gem.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                ExpGem expGem = gem.GetComponent<ExpGem>();
                if (expGem != null) expGem.InitGem(bossDropExp, true);
            }

            if (isFinalBoss)
            {
                if (GameManager.instance != null) GameManager.instance.GameClear();
            }
            else
            {
                if (Spawner.instance != null) Spawner.instance.AdvanceNextStage();
            }

            StartCoroutine(BossDeadRoutine());
        }

        if (treasureChestPrefab != null && Random.Range(0, 100) < 1)
        {
            Instantiate(treasureChestPrefab, transform.position, Quaternion.identity);
        }

        if (Random.Range(0, 100) < 50)
        {
            GameManager.instance.AddGold(1);
            if (SoundManager.instance != null) SoundManager.instance.PlaySFX(SoundManager.SFX.GetGold);
            GameObject goldPop = PoolManager.instance.Get(8);
            goldPop.transform.position = transform.position;
        }

        if (GameManager.instance.vampireHealAmount > 0)
        {
            if (Random.Range(0, 100) < 50)
                GameObject.Find("player").GetComponent<Player>().Heal(GameManager.instance.vampireHealAmount);
        }

        float dropChance = Random.Range(0f, 100f);

        if (dropChance <= 0.1f && magnetItemPrefab != null) Instantiate(magnetItemPrefab, transform.position, Quaternion.identity);
        else if (dropChance <= 1f && healItemPrefab != null) Instantiate(healItemPrefab, transform.position, Quaternion.identity);
        else if (!isBoss) 
        {
            float finalDropExp = dropExpAmount;
            if (Spawner.instance != null && Spawner.instance.isInfiniteMode)
            {
                finalDropExp = 600f;
            }

            if (ExpGem.activeGemCount < ExpGem.MAX_GEMS)
            {
                GameObject gem = PoolManager.instance.Get(expGemPoolIndex);
                gem.transform.position = transform.position;
                bool makeBig = ExpGem.compressedExpPool > 0;

                gem.GetComponent<ExpGem>().InitGem(finalDropExp, makeBig);
            }
            else
            {
                ExpGem.compressedExpPool += finalDropExp;
            }
        }

        StartCoroutine(DeadRoutine());
    }

    IEnumerator DeadRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }

    IEnumerator BossDeadRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}