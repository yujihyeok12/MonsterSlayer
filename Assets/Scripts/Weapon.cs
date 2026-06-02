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
