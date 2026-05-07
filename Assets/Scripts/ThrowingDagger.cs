using UnityEngine;

public class ThrowingDagger : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public float speed;
    [HideInInspector] public Vector2 dir;

    void Start()
    {
        if (dir.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        Destroy(gameObject, 1f);
    }

    void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

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
            Destroy(gameObject);
        }
    }
}

/*
========================================================
[ ThrowingDagger.cs 상세 설명서 (투척 단검 투사체)]
1. 스크립트 역할:
   - ThrowingDaggerWeapon에 의해 생성된 후, 지시받은 방향으로 일직선으로 빠르게 날아가는 실제 칼날(발사체) 역할을 합니다.

2. 핵심 작동 흐름 및 함수:
   - Start(): 🌟디테일 연출! 만약 칼날이 날아가는 방향(dir.x)이 왼쪽(< 0)이라면, 유니티 SpriteRenderer의 flipX를 켜서 칼날의 그림을 좌우 반전시켜 줍니다. (칼끝이 진행 방향을 향하도록)
     그리고 너무 멀리 날아가서 메모리를 갉아먹지 않도록 1초 뒤에 스스로 파괴(Destroy)되게 시한폭탄을 달아둡니다.
   - Update(): 매 프레임마다 나에게 주입된 방향(dir)과 속도(speed)를 곱해서 우직하게 앞으로 날아갑니다. (Space.World 기준)
   - OnTriggerEnter2D(): 날아가다 몬스터 태그를 가진 녀석과 부딪히면 -> 플레이어가 먹은 '용사의 검(damageMultiplier)' 배율을 곱해 최종 데미지를 계산하고 -> 몬스터의 피를 깎은 뒤 -> 자신은 화면에서 파괴됩니다.
========================================================
*/