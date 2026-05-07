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

/*
========================================================
[ TreasureChest.cs 상세 설명서 (보물상자 연출)]
1. 스크립트 역할:
   - 필드에 생성된 보물상자가 플레이어와 닿았을 때의 '오픈 연출'을 담당합니다.

2. 핵심 작동 흐름 및 함수:
   - OnTriggerEnter2D(): 플레이어의 몸체가 상자에 닿는 순간 실행됩니다. 이때 상자를 두 번 먹어서 보상이 중복으로 들어오는 버그를 막기 위해 `coll.enabled = false`로 물리 충돌 판정부터 바로 꺼버립니다.
   - OpenRoutine(): 상자가 열리는 과정(코루틴)입니다.
     -> 열리는 애니메이션(Trigger "Open") 실행 -> '덜컹' 사운드 재생 -> 0.6초간 대기(상자가 열리는 모습을 유저가 눈으로 볼 수 있게 기다려줌) -> GameManager의 OpenTreasureChest()를 호출해서 화면을 덮는 룰렛 UI를 띄움 -> 볼일이 끝난 필드의 상자는 Destroy!
========================================================
*/