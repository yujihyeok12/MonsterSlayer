using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;

    [Header("카메라 설정")]
    public float smoothSpeed = 5f;
    private Vector3 offset;

    void Start()
    {
        player = GameObject.Find("player").transform;
        offset = new Vector3(0, 0, -15f);
    }

    void LateUpdate()
    {
        if (GameManager.instance != null && GameManager.instance.isCinematic) return;
        if (player == null) return;

        transform.position = player.position + offset;
    }
}

/*
========================================================
[ CameraFollow.cs 설명서 (카메라 감독님)]
1. 이 스크립트의 역할:
   - 게임 화면(카메라)이 항상 플레이어를 졸졸 따라다니게 해주는 스크립트입니다. Main Camera에 붙어있습니다.

2. 주요 변수:
   - offset: 플레이어와 카메라 사이의 거리입니다. (보통 z축으로 -15 정도 떨어져서 화면을 비춥니다)

3. 주요 함수:
   - LateUpdate(): 모든 캐릭터들이 이동을 끝낸 직후(맨 마지막)에 실행됩니다. 플레이어의 최종 위치를 보고 그 위로 카메라를 덮어씌웁니다.

4. 작동 흐름:
   - 시작할 때 플레이어를 찾음 -> 매 프레임마다 플레이어 머리 위(offset 위치)로 카메라를 이동함.
   - 단, 보스 등장 등 '시네마틱 연출(isCinematic)' 중일 때는 따라가기를 잠시 멈춥니다.
========================================================
*/