using UnityEngine;
using System.Collections.Generic;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    public GameObject enemyPrefab; // 怪物预制体
    public int poolSize = 10;      // 准备10个怪物

    private List<GameObject> pooledEnemies;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        pooledEnemies = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.SetActive(false); // 先隐藏
            pooledEnemies.Add(obj);
        }
    }

    public GameObject GetEnemy()
    {
        for (int i = 0; i < pooledEnemies.Count; i++)
        {
            // 找一个没在用的（隐藏状态的）怪物
            if (!pooledEnemies[i].activeInHierarchy)
            {
                return pooledEnemies[i];
            }
        }
        
        // 如果池子空了，可选：扩展池子 (这里简化处理，返回null)
        return null;
    }
}