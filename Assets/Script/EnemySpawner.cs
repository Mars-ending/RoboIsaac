using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval = 3.0f; // 每3秒刷一只怪
    private float timer = 0f;

    private int currentSpawnCount = 0; // 内部计数器：记录已经刷了几个

    public int maxSpawnCount = 10;

    void Start()
    {
        // --- 【关键修改】主动向裁判申报 ---
        if (GameManager.Instance != null)
        {
            // 告诉 GameManager：我要贡献 maxSpawnCount 这么多敌人
            GameManager.Instance.RegisterEnemies(maxSpawnCount);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // 1. 从池子里拿一只怪
        GameObject enemy = EnemyPool.Instance.GetEnemy();
        
        if (enemy != null)
        {
            // 2. 设定位置 (在这个刷怪笼的位置)
            enemy.transform.position = transform.position;
            enemy.transform.rotation = Quaternion.identity;

            // 3. 激活它 (这会触发 EnemyController 的 OnEnable)
            enemy.SetActive(true);
        }
    }
}