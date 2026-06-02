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
