using UnityEngine;
using UnityEngine.Tilemaps; 

public class Reposition : MonoBehaviour
{
    private Transform player;
    private TilemapRenderer myRenderer;

    [Header("타일 설정")]
    private float tileSize = 50f;

    void Start()
    {
        player = GameObject.Find("player").transform;

        myRenderer = GetComponentInChildren<TilemapRenderer>();
    }

    void LateUpdate()
    {
        Vector3 centerPos = myRenderer != null ? myRenderer.bounds.center : transform.position;

        float diffX = player.position.x - centerPos.x;
        float diffY = player.position.y - centerPos.y;

        Vector3 newPos = transform.position;

        if (diffX >= tileSize)
        {
            newPos.x += tileSize * 2f;
        }
        else if (diffX <= -tileSize)
        {
            newPos.x -= tileSize * 2f;
        }

        if (diffY >= tileSize)
        {
            newPos.y += tileSize * 2f;
        }
        else if (diffY <= -tileSize)
        {
            newPos.y -= tileSize * 2f;
        }

        transform.position = newPos;
    }
}

/*
========================================================
[ Reposition.cs 상세 설명서 (무한 맵 러닝머신)]
1. 스크립트 역할:
   - 뱀파이어 서바이버처럼 플레이어가 한 방향으로 계속 걸어가도 맵이 끝없이 이어지게 만드는 마술(무한 맵) 타일 스크립트입니다.

2. 작동 원리 (LateUpdate):
   - 카메라가 플레이어를 쫓아간 직후(LateUpdate)에 실행됩니다.
   - 타일의 중심 위치(centerPos)와 플레이어의 위치를 비교(diffX, diffY)합니다.
   - 플레이어가 내 타일 크기(tileSize)보다 더 멀리 벗어났다면, 타일 자체를 플레이어가 걸어가는 방향 앞쪽(tileSize * 2f)으로 순간이동 시킵니다. (마치 러닝머신 바닥이 앞으로 땡겨지는 원리)
========================================================
*/