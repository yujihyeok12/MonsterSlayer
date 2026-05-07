using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    [Header("프리팹 보관소")]
    public GameObject[] prefabs;

    private List<GameObject>[] pools;

    void Awake()
    {
        instance = this;

        ExpGem.activeGemCount = 0;
        ExpGem.compressedExpPool = 0f;

        pools = new List<GameObject>[prefabs.Length];

        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true); 
                break;
            }
        }

        if (select == null)
        {
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }
}

/*
========================================================
[ PoolManager.cs 상세 설명서 (메모리 최적화 창고)]
1. 스크립트 역할:
   - 게임 중에 수백 개씩 생겼다 없어지는 몬스터, 보석, 이펙트 등을 매번 Instantiate/Destroy 하면 렉(GC 스파이크)이 걸립니다. 이를 막기 위해 한 번 만든 오브젝트를 리스트에 담아두고 껐다 켰다(재활용) 하는 '오브젝트 풀링(Object Pooling)' 창고입니다.

2. 주요 변수 및 함수:
   - pools 배열: 프리팹 종류별(0번: 몬스터, 1번: 보석 등)로 실제 만들어진 오브젝트들을 보관하는 서랍장입니다.
   - Get(index): 다른 스크립트에서 "index번 프리팹 하나 줘!" 라고 할 때 실행됩니다.
     해당 번호의 서랍장(pools[index])을 뒤져서 안 쓰고 꺼져있는(activeSelf == false) 녀석을 찾아 켜서(true) 줍니다. 만약 서랍에 꺼져있는 게 하나도 없다면 그때만 어쩔 수 없이 새로 만들어서 서랍에 추가하고 줍니다.
========================================================
*/