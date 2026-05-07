using UnityEngine;

public class GoldEffect : MonoBehaviour
{
    public float floatSpeed = 2f;  
    public float lifeTime = 0.5f;  
    private float timer = 0f;

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }
}

/*
========================================================
[📝 GoldEffect.cs 설명서 (동전 연출)]
1. 이 스크립트의 역할:
   - 몬스터를 잡고 골드를 얻었을 때, 몬스터가 죽은 자리에서 쪼그만 동전이나 "+1" 글씨가 위로 살짝 떠올랐다가 사라지는 시각적 효과(이펙트)를 담당합니다.

2. 주요 변수:
   - floatSpeed: 위로 떠오르는 속도
   - lifeTime: 화면에 보여질 시간 (보통 0.5초)

3. 작동 흐름:
   - 몬스터 죽을 때 소환됨 -> 위로 스르륵 올라감 -> 0.5초 뒤에 스스로 사라짐(SetActive(false))
========================================================
*/