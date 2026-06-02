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
