using UnityEngine;
using System.Collections;

public class TreasureChest : MonoBehaviour
{
    private Animator anim;
    private Collider2D coll;

    void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "player" || collision.CompareTag("Player"))
        {
            coll.enabled = false;
            StartCoroutine(OpenRoutine());
        }
    }

    IEnumerator OpenRoutine()
    {
        if (anim != null) anim.SetTrigger("Open");

        if (SoundManager.instance != null)
            SoundManager.instance.PlaySFX(SoundManager.SFX.ChestOpen);

        yield return new WaitForSeconds(0.6f);

        if (GameManager.instance != null)
        {
            GameManager.instance.OpenTreasureChest();
        }

        Destroy(gameObject);
    }
}
