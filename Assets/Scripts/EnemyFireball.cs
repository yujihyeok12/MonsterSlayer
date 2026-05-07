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

/*
========================================================
[📝 EnemyFireball.cs 설명서 (보스 공격 투사체)]
1. 이 스크립트의 역할:
   - 보스 몬스터가 플레이어를 향해 발사하는 빨간 불덩이 공격입니다.

2. 주요 변수:
   - damage: 맞았을 때 플레이어가 입는 피해량
   - moveDir: 불덩이가 날아갈 방향

3. 주요 함수:
   - Setup(): 보스가 불덩이를 소환하면서 "저쪽으로 날아가!" 하고 방향을 정해줍니다.
   - OnTriggerEnter2D(): 날아가다가 플레이어랑 닿으면 아프게 때리고(TakeDamage) 자기는 사라집니다.

4. 작동 흐름:
   - 보스가 소환 및 방향 지시 -> 일직선으로 날아감 -> 플레이어 맞추거나 5초 지나면 소멸
========================================================
*/