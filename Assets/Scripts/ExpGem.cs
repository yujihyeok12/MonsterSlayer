using UnityEngine;

public class ExpGem : MonoBehaviour
{
    public static int activeGemCount = 0;
    public static float compressedExpPool = 0f;
    public const int MAX_GEMS = 200;

    [Header("자석(흡수) 설정")]
    public float moveSpeed = 15f;

    [HideInInspector] public float currentExp = 10f;

    private Transform player;
    private Player playerScript; 

    private bool isFollowing = false;
    private SpriteRenderer spriter;
    private Vector3 originalScale;

    void Awake()
    {
        GameObject playerObj = GameObject.Find("player");
        player = playerObj.transform;
        playerScript = playerObj.GetComponent<Player>();

        spriter = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        activeGemCount++;
        isFollowing = false;
    }

    void OnDisable()
    {
        activeGemCount--;
    }

    public void InitGem(float baseExp, bool isBigGem)
    {
        if (isBigGem)
        {
            currentExp = baseExp + compressedExpPool;
            compressedExpPool = 0f;
            transform.localScale = originalScale * 1.5f;
        }
        else
        {
            currentExp = baseExp;
            transform.localScale = originalScale;
        }
    }

    void Update()
    {
        if (player == null || playerScript == null) return;

        if (!isFollowing)
        {
            if (Vector3.Distance(transform.position, player.position) <= playerScript.magnetRange)
            {
                isFollowing = true;
            }
        }

        if (isFollowing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "player")
        {
            GameManager.instance.GetExp(currentExp);
            gameObject.SetActive(false);
        }
    }

    public void StartGlobalMagnet()
    {
        isFollowing = true;
    }
}
