using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 5f;
    public Vector2 inputVec;

    [Header("체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider hpSlider;
    public Text hpText;

    [Header("피격(무적) 설정")]
    public float invincibilityTime = 0.5f;
    private float hitTimer = 0f;

    [Header("기본 패시브 스탯")]
    public float armor = 0f;
    public float magnetRange = 1.5f;

    [Header("보물상자 스탯 (배율 & 특수)")]
    public float damageMultiplier = 1.0f;  
    public float armorMultiplier = 1.0f;   
    public float maxHpMultiplier = 1.0f;   
    public float speedMultiplier = 1.0f;
    public float flatMaxHpBonus = 0f;     
    public float hpRegenAmount = 0f;       
    public int reviveCount = 0;

    [HideInInspector] public float baseMaxHealth;

    [Header("사운드 설정")]
    public float stepInterval = 0.35f;
    private float stepTimer = 0f;

    private Animator anim;
    private Rigidbody2D rigid;
    private SpriteRenderer spriter;
    private bool isDead = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateMaxHealth();

        StartCoroutine(HpRegenRoutine());
    }

    void Update()
    {
        if (isDead) return;

        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.magnitude > 0)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                if (SoundManager.instance != null)
                    SoundManager.instance.PlaySFX(SoundManager.SFX.PlayerMove);

                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval;
        }

        if (hitTimer > 0) hitTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (isDead) return;
        Vector2 nextVec = inputVec.normalized * (speed * speedMultiplier) * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (isDead) return;
        if (inputVec.x != 0) transform.localScale = new Vector3(inputVec.x < 0 ? -1 : 1, 1, 1);
    }
    public void UpdateMaxHealth()
    {
        if (baseMaxHealth == 0f) baseMaxHealth = maxHealth;

        float oldMax = maxHealth;

        maxHealth = (baseMaxHealth + flatMaxHpBonus) * maxHpMultiplier;

        float difference = maxHealth - oldMax;
        if (difference > 0)
        {
            currentHealth += difference;
        }

        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHpUI();
    }

    IEnumerator HpRegenRoutine()
    {
        while (true) 
        {
            yield return new WaitForSeconds(5f);

            if (hpRegenAmount > 0 && currentHealth < maxHealth && !isDead)
            {
                Heal(hpRegenAmount);
                Debug.Log($" 빵 효과: 체력 {hpRegenAmount} 회복!");
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead || hitTimer > 0) return;

        float finalArmor = armor * armorMultiplier;
        float finalDamage = Mathf.Max(1f, damageAmount - finalArmor);

        currentHealth -= finalDamage;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        hitTimer = invincibilityTime;

        UpdateHpUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitEffect());
        }
    }

    IEnumerator HitEffect()
    {
        spriter.color = new Color(1f, 1f, 1f, 0.4f);
        yield return new WaitForSeconds(0.1f);
        spriter.color = new Color(1f, 1f, 1f, 1f);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null && enemy.health > 0) TakeDamage(enemy.damage);
        }
    }

    public void Die()
    {
        if (isDead) return;

        if (reviveCount > 0)
        {
            reviveCount--;
            currentHealth = maxHealth * 0.5f; 
            hitTimer = 2f;

            UpdateHpUI();

            Debug.Log($"해골 효과 발동 남은 부활 횟수: {reviveCount}");

            if (GameManager.instance != null)
            {
                GameManager.instance.ShowReviveEffect();
                GameManager.instance.UseSkullItem();
            }

            return; 
        }

        isDead = true;
        anim.SetTrigger("Dead");

        if (SoundManager.instance != null)
            SoundManager.instance.PlaySFX(SoundManager.SFX.PlayerDead);

        if (GameManager.instance != null) GameManager.instance.GameOver();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHpUI();
    }

    public void UpdateHpUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
