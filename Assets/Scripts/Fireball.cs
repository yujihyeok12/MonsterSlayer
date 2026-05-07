using UnityEngine;

public class Fireball : MonoBehaviour
{
    private float damage;
    private float speed;
    private float aliveTimer; 

    public void Init(float newDamage, float newSpeed)
    {
        damage = newDamage;
        speed = newSpeed;
    }

    void OnEnable()
    {
        aliveTimer = 0f;
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        aliveTimer += Time.deltaTime;
        if (aliveTimer >= 3f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && enemy.health > 0)
            {
                float finalDamage = damage * GameManager.instance.player.damageMultiplier;
                enemy.TakeDamage(finalDamage);
            }

            gameObject.SetActive(false);
        }
    }
}

/*
========================================================
[ Fireball.cs 설명서 (불덩이 발사체)]
1. 이 스크립트의 역할:
   - 새끼용이 뱉어낸 불덩이 그 자체입니다. 앞으로 쭉 날아가다가 적을 맞추면 데미지를 주고 터집니다.

2. 주요 변수:
   - damage, speed: 새끼용이 뱉을 때 정해준 데미지와 날아가는 속도
   - aliveTimer: 허공으로 날아갔을 때 영원히 날아가지 않도록 수명을 재는 타이머

3. 주요 함수:
   - Init(): 새끼용이 이 불덩이를 소환하자마자 "너 데미지는 이거고 속도는 이거야!" 하고 세팅해 주는 함수입니다.
   - OnEnable(): 창고(PoolManager)에서 꺼내질 때마다 타이머를 0으로 리셋해 줍니다.
   - Update(): 앞으로 날아가며, 3초가 지나면 렉 방지를 위해 스스로 눈동자를 끕니다(SetActive(false)).
   - OnTriggerEnter2D(): 적과 닿으면 플레이어의 '데미지 배율(용사의 검 등)'을 곱해서 최종 데미지를 먹인 뒤 화면에서 사라집니다.
========================================================
*/