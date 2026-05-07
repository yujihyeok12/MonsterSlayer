using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 설정")]
    public Transform player;
    public float speed = 150f;
    public float radius = 1.5f;

    void Start()
    {
        player = GameObject.Find("player").transform;
        ArrangeWeapons();
    }

    void Update()
    {
        if (player == null) return;

        transform.position = player.position;
        transform.Rotate(Vector3.forward * speed * Time.deltaTime);
    }

    public void ArrangeWeapons()
    {
        int count = transform.childCount;
        if (count == 0) return;

        float angle = 360f / count;

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);

            child.localRotation = Quaternion.Euler(0, 0, angle * i);

            child.localPosition = child.localRotation * Vector3.up * radius;
        }
    }
}

/*
========================================================
[회전검 매니저]
1. 이 스크립트의 역할:
   - 플레이어 주변을 빙글빙글 도는 '회전검'들의 중심축 역할을 합니다. 이 중심축이 돌면 자식으로 달려있는 검들이 같이 돕니다.

2. 주요 변수:
   - speed: 도는 속도
   - radius: 플레이어로부터 얼마나 떨어져서 돌 것인지(반지름)

3. 주요 함수:
   - Update(): 매 프레임마다 플레이어의 위치를 따라가면서 중심축을 회전시킵니다.
   - ArrangeWeapons(): 레벨업을 해서 검 개수가 늘어났을 때, 검들이 겹치지 않고 예쁜 원형으로 일정하게 퍼지도록 각도와 위치를 재배치해 주는 함수입니다.

4. 작동 흐름:
   - 처음 시작하거나 검이 추가될 때 ArrangeWeapons()로 줄을 세우고, 이후엔 계속 플레이어를 따라다니며 뱅글뱅글 돕니다.
========================================================
*/