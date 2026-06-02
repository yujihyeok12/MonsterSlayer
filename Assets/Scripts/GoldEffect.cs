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
