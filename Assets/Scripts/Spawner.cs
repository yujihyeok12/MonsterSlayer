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

    [Header("--- 무한 모드 설정 ---")]
    public bool isInfiniteMode = false;
    public float infiniteBaseHealth = 400f;
    public float infiniteBaseSpeed = 6f;
    public float infiniteBaseDamage = 150f;
    public float healthAddPerMinute = 200f;
    public float damageAddPerMinute = 10f;

    private float infiniteMinuteTimer = 0f;
    private float currentHealthBonus = 0f;
    private float currentDamageBonus = 0f;
    private int[] allStageEnemies;

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
        if (isInfiniteMode)
        {
            infiniteMinuteTimer += Time.deltaTime;

            if (infiniteMinuteTimer >= 60f)
            {
                currentHealthBonus += healthAddPerMinute;
                currentDamageBonus += damageAddPerMinute;
                infiniteMinuteTimer = 0f;

                Enemy[] activeEnemies = FindObjectsOfType<Enemy>();
                foreach (Enemy e in activeEnemies)
                {
                    if (e.gameObject.activeSelf && !e.isBoss) e.ApplyInfiniteStats();
                }
            }

            timer += Time.deltaTime;
            if (timer > 0.2f)
            {
                Spawn(5, allStageEnemies);
                timer = 0f;
            }
            return;
        }

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
            Vector3 spawnPos = player.position + (Vector3)(randomDir * 15f);

            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            Debug.Log($" {stageName} 보스 등장!! ");

            if (GameManager.instance != null)
            {
                GameManager.instance.ShowBossWarning(boss.transform);
            }
        }
    }

    public void AdvanceNextStage()
    {
        currentStageIndex++;
        stageTimer = 0f;
        isBossSpawned = false;
    }

    public void StartInfiniteMode()
    {
        isInfiniteMode = true;
        currentHealthBonus = 0f;
        currentDamageBonus = 0f;
        infiniteMinuteTimer = 0f;

        List<int> allEnemiesList = new List<int>();
        foreach (var stage in stages)
        {
            foreach (int id in stage.allowedEnemies) if (!allEnemiesList.Contains(id)) allEnemiesList.Add(id);
        }
        allStageEnemies = allEnemiesList.ToArray();

        Enemy[] activeEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy e in activeEnemies)
        {
            if (e.gameObject.activeSelf && !e.isBoss) e.ApplyInfiniteStats();
        }
    }

    public float GetCurrentInfiniteHealth() => infiniteBaseHealth + currentHealthBonus;
    public float GetCurrentInfiniteSpeed() => infiniteBaseSpeed;
    public float GetCurrentInfiniteDamage() => infiniteBaseDamage + currentDamageBonus;
}