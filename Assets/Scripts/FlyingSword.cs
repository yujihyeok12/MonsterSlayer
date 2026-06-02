using UnityEngine;

public class FlyingSword : MonoBehaviour
{
    public enum SwordState { Idle, Chase, Return, Cooldown }
    public SwordState state = SwordState.Idle;

    [Header("전투 설정")]
    public float damage = 10f;
    public float speed = 20f;
    public float detectRadius = 10f;
    public float turnSpeed = 10f;

    [Header("시간 설정")]
    public float chaseTimeLimit = 5f;
    public float cooldownTime = 2f;

    [Header("이미지(각도) 설정")]
    public float idleRotation = 180f;
    public float flyRotationOffset = 90f;

    [Header("등 뒤 호위 정렬 설정")]
    public float backDistance = 1.5f;
    public float spacing = 0.8f; 

    private Transform player;
    private Transform target;

    private float myXOffset;
    private float timer = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
        player = GameObject.Find("player").transform;
        RefreshAllSwords();
    }

    public static void RefreshAllSwords()
    {
        FlyingSword[] allSwords = FindObjectsOfType<FlyingSword>();
        for (int i = 0; i < allSwords.Length; i++)
        {
            allSwords[i].SetIndex(i);
        }
    }

    public void SetIndex(int index)
    {
        myXOffset = backDistance + (index * spacing);
    }

    void Update()
    {
        if (player == null) return;

        float playerDir = Mathf.Sign(player.localScale.x);

        Vector3 waitPos = player.position + new Vector3(-playerDir * myXOffset, 0, 0);

        switch (state)
        {
            case SwordState.Idle:
                transform.position = Vector3.Lerp(transform.position, waitPos, 10f * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, idleRotation);
                FindNewTarget();
                break;

            case SwordState.Chase:
                timer += Time.deltaTime;

                if (timer >= chaseTimeLimit)
                {
                    state = SwordState.Return;
                    break;
                }

                if (Vector3.Distance(transform.position, player.position) > detectRadius)
                {
                    Vector3 centerDir = (player.position - transform.position).normalized * speed;
                    currentVelocity = Vector3.Lerp(currentVelocity, centerDir, turnSpeed * Time.deltaTime);
                    target = null;
                }
                else
                {
                    if (target != null)
                    {
                        Enemy e = target.GetComponent<Enemy>();
                        if (e == null || e.health <= 0 || !target.gameObject.activeSelf)
                        {
                            target = null;
                        }
                    }

                    if (target == null) FindNewTarget();

                    if (target != null)
                    {
                        Vector3 desiredVelocity = (target.position - transform.position).normalized * speed;
                        currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, turnSpeed * Time.deltaTime);
                    }
                }

                if (currentVelocity != Vector3.zero)
                {
                    currentVelocity = currentVelocity.normalized * speed;
                }

                transform.position += currentVelocity * Time.deltaTime;
                LookAtDirection(currentVelocity);
                break;

            case SwordState.Return:
                Vector3 returnDir = (waitPos - transform.position).normalized * speed;

                currentVelocity = Vector3.Lerp(currentVelocity, returnDir, (turnSpeed * 5f) * Time.deltaTime);

                if (currentVelocity != Vector3.zero)
                {
                    currentVelocity = currentVelocity.normalized * (speed * 1.5f);
                }

                transform.position += currentVelocity * Time.deltaTime;
                LookAtDirection(currentVelocity);

                if (Vector3.Distance(transform.position, waitPos) < 0.5f)
                {
                    state = SwordState.Cooldown;
                    timer = 0f;
                }
                break;

            case SwordState.Cooldown:
                transform.position = waitPos;
                transform.rotation = Quaternion.Euler(0, 0, idleRotation);

                timer += Time.deltaTime;
                if (timer >= cooldownTime)
                {
                    state = SwordState.Idle;
                }
                break;
        }
    }

    void FindNewTarget()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.position, detectRadius);
        float closestDist = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy") && col.gameObject.activeSelf)
            {
                Enemy e = col.GetComponent<Enemy>();
                if (e != null && e.health > 0)
                {
                    float dist = Vector3.Distance(transform.position, col.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestEnemy = col.transform;
                    }
                }
            }
        }

        target = closestEnemy;

        if (target != null && state == SwordState.Idle)
        {
            timer = 0f;
            state = SwordState.Chase;
            SoundManager.instance.PlaySFX(SoundManager.SFX.FlyingSword);
            currentVelocity = (target.position - transform.position).normalized * speed;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (state == SwordState.Chase && collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.health > 0)
            {
                float finalDamage = damage * GameManager.instance.player.damageMultiplier;
                enemy.TakeDamage(finalDamage);
            }
        }
    }

    void LookAtDirection(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + flyRotationOffset);
    }
}
