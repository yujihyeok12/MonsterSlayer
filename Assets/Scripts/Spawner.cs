using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public static Spawner instance; 

    [Header("Spawn sys (기본 소환)")]
    public Transform player;
    public float spawnRadius = 20f;

    [Header("Wave sys (웨이브 설정)")]
    public float waveInterval = 120f;
    public float waveDuration = 60f;
    public float normalSpawnTime = 0.5f;
    public float waveSpawnTime = 0.15f;
    public int normalSpawnCount = 1;
    public int waveSpawnCount = 3;

    [System.Serializable]
    public class StageData
    {
        public string stageName;           
        public int[] allowedEnemies;       
        public float bossSpawnTime = 300f; 
        public GameObject bossPrefab;     
    }

    [Header("--- 스테이지 설정 ---")]
    public StageData[] stages;
    public int currentStageIndex = 0;

    private float timer;
    private float stageTimer = 0f; 
    private bool isBossSpawned = false; 

    [HideInInspector] public bool isWave = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        player = GameObject.Find("player").transform;
    }

    void Update()
    {
        if (currentStageIndex >= stages.Length) return; 

        timer += Time.deltaTime;
        stageTimer += Time.deltaTime;

        StageData currentStage = stages[currentStageIndex];

        if (stageTimer >= currentStage.bossSpawnTime && !isBossSpawned)
        {
            SpawnBoss(currentStage.bossPrefab, currentStage.stageName);
            isBossSpawned = true; 
        }

        float cycle = waveInterval + waveDuration;
        float currentCycleTime = stageTimer % cycle;
        isWave = (currentCycleTime >= waveInterval);

        float currentSpawnTime = isWave ? waveSpawnTime : normalSpawnTime;
        int currentSpawnCount = isWave ? waveSpawnCount : normalSpawnCount;

        if (timer > currentSpawnTime)
        {
            Spawn(currentSpawnCount, currentStage.allowedEnemies);
            timer = 0f;
        }
    }

    void Spawn(int count, int[] allowedEnemies)
    {
        if (allowedEnemies.Length == 0) return;

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, allowedEnemies.Length);
            int realPoolIndex = allowedEnemies[randomIndex];

            GameObject enemy = PoolManager.instance.Get(realPoolIndex);

            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 spawnPos = player.position + (Vector3)(randomDir * spawnRadius);

            enemy.transform.position = spawnPos;
        }
    }

    void SpawnBoss(GameObject bossPrefab, string stageName)
    {
        if (bossPrefab != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 spawnPos = player.position + (Vector3)(randomDir * 25f);

            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"🚨 {stageName} 보스 등장!! 🚨");

            if (GameManager.instance != null)
            {
                GameManager.instance.ShowBossWarning(boss.transform);
            }   
        }
    }
    
    public void AdvanceNextStage()
    {
        Debug.Log("보스 처치, 다음 스테이지로");
        currentStageIndex++;   
        stageTimer = 0f;       
        isBossSpawned = false; 
    }
}

/*
========================================================
[ Spawner.cs 상세 설명서 (몬스터 & 웨이브 소환 매니저)]
1. 스크립트 역할:
   - 게임 내내 플레이어 주변에 몬스터를 생성하고, 특정 시간이 되면 '웨이브(몰려옴)'를 발생시키며, 스테이지 보스 소환과 다음 스테이지로의 진행을 총괄하는 컨트롤 타워입니다.

2. 주요 변수 (기획자가 유니티 인스펙터에서 조절할 값들):
   - waveInterval & waveDuration: 평화로운 시간(Interval)이 지나면, 몬스터가 미친듯이 쏟아지는 시간(Duration)이 얼마나 지속될지 정합니다.
   - StageData (클래스): 스테이지별 이름, 등장할 일반 몬스터 번호들(allowedEnemies 배열), 보스 등장 시간, 보스 프리팹을 하나로 묶어둔 데이터 뭉치입니다.

3. 핵심 작동 흐름 및 함수:
   - Update(): 매 프레임 시간을 재면서 두 가지를 체크합니다.
     1) [보스 체크] 현재 스테이지의 타이머(stageTimer)가 보스 소환 시간에 도달하면 SpawnBoss()를 부릅니다.
     2) [웨이브 체크] 'stageTimer % (평화+웨이브 시간)' 공식을 이용해 지금이 웨이브 타임인지 계산합니다. 웨이브 중이라면 소환 주기(currentSpawnTime)를 확 줄이고 소환 마리수(currentSpawnCount)를 늘립니다.
   - Spawn(): 🌟메모리 최적화의 핵심! 
     스테이지 데이터에 등록된 몬스터 번호 중 하나를 랜덤으로 뽑은 뒤, `PoolManager.instance.Get()`을 통해 '창고'에서 몬스터를 꺼내옵니다. (절대 새로 Instantiate 하지 않습니다)
   - SpawnBoss(): 보스 프리팹은 풀매니저에 없으므로 직접 Instantiate로 생성하며, 생성 직후 GameManager에게 "보스 나왔으니 시간 멈추고 경고창 띄워라!" 라고 명령합니다.
   - AdvanceNextStage(): 보스가 죽었을 때 Enemy.cs에서 이 함수를 부릅니다. 스테이지 타이머를 0으로 리셋하고 다음 난이도(다음 스테이지)로 넘어갑니다.
========================================================
*/