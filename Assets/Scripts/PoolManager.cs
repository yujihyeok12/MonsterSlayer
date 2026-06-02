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