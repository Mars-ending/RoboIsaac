using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval = 3.0f; // 每3秒刷一只怪
    private float timer = 0f;

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