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

        health = maxHealth;
        isDead = false;
        isKnockback = false;
        anim.SetBool("Dead", false);
        coll.enabled = true;
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

        if (treasureChestPrefab != null && Random.Range(0, 100) < 1) // 1 = 1% 확률
        {
            Instantiate(treasureChestPrefab, transform.position, Quaternion.identity);
        }

        if (Random.Range(0, 100) < 50)
        {
            GameManager.instance.AddGold(1);
            SoundManager.instance.PlaySFX(SoundManager.SFX.GetGold);
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
        else
        {
            if (ExpGem.activeGemCount < ExpGem.MAX_GEMS)
            {
                GameObject gem = PoolManager.instance.Get(expGemPoolIndex);
                gem.transform.position = transform.position;
                bool makeBig = ExpGem.compressedExpPool > 0;
                gem.GetComponent<ExpGem>().InitGem(dropExpAmount, makeBig);
            }
            else ExpGem.compressedExpPool += dropExpAmount;
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

/*
========================================================
[ Enemy.cs 설명서 (몬스터 AI 및 스탯)]
1. 이 스크립트의 역할:
   - 일반 몬스터와 보스 몬스터의 뇌(AI)와 체력을 담당합니다. 플레이어를 쫓아가고, 맞으면 피가 깎이고, 죽으면 아이템을 떨어뜨립니다.

2. 주요 변수:
   - health, speed, damage: 몬스터의 기본 스탯
   - isBoss: 이 녀석이 보스인지 체크 (보스면 불덩이도 쏩니다!)
   - treasureChestPrefab: 1% 확률로 떨어뜨릴 대박 보물상자

3. 주요 함수:
   - OnEnable(): 창고(PoolManager)에서 꺼내질 때마다 체력과 상태를 새것처럼 초기화합니다.
   - FixedUpdate(): 좀비처럼 무조건 플레이어가 있는 방향을 향해 다가갑니다. (시간 정지 중엔 멈춤)
   - TakeDamage(): 플레이어 무기에 맞았을 때 피가 깎이고 넉백(밀려남)됩니다.
   - Die(): 체력이 0이 되면 경험치 보석, 골드, 힐링 고기, 자석, 그리고 1% 확률의 '보물상자'를 땅에 뿌리고 창고로 돌아갑니다.

4. 작동 흐름:
   - 소환됨 -> 플레이어 추적 -> 맞아서 피 깎임 -> 죽으면서 템 드랍 -> 비활성화(풀링)
========================================================
*/