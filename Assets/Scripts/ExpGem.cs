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

/*
========================================================
[📝 ExpGem.cs 설명서 (경험치 보석 & 자석 시스템)]
1. 이 스크립트의 역할:
   - 몬스터가 죽을 때 떨어뜨리는 보석입니다. 플레이어가 다가가면 슉! 하고 빨려 들어와 경험치를 줍니다.

2. 주요 변수:
   - currentExp: 이 보석이 품고 있는 경험치 양
   - compressedExpPool: 🌟최적화 핵심! 맵에 보석이 200개 넘게 쌓이면 렉이 걸리니까, 새로 나오는 보석은 투명하게 만들고 경험치만 여기에 킵(저장)해 둡니다. 나중에 빨간 왕보석(isBigGem)이 나올 때 몰아서 줍니다.

3. 주요 함수:
   - Update(): 매 프레임 플레이어와의 거리를 재다가, 자석 범위(magnetRange) 안에 들어오면 플레이어 쪽으로 날아갑니다.
   - StartGlobalMagnet(): 맵에 있는 '자석 아이템'을 먹었을 때 호출되며, 거리에 상관없이 맵 전체의 보석이 일제히 날아옵니다.

4. 작동 흐름:
   - 몬스터가 드랍 -> 땅에 대기 -> 자석 범위에 닿음 -> 플레이어에게 날아감 -> 닿으면 경험치 주고 비활성화
========================================================
*/