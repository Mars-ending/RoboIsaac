using UnityEngine;
using System.Collections.Generic; // 必须引用这个，用于使用 List

public class BulletPool : MonoBehaviour
{
    // 单例模式：为了让主角脚本能方便地找到这个池子
    public static BulletPool Instance;

    [Header("池子设置")]
    public GameObject bulletPrefab; // 子弹预制体
    public int poolSize = 20;       // 咱们先造20颗子弹备用

    // 这就是“池子”，存子弹的列表
    private List<GameObject> pooledBullets;

    void Awake()
    {
        Instance = this; // 赋值单例
    }

    void Start()
    {
        // 1. 初始化池子
        pooledBullets = new List<GameObject>();

        // 2. 提前生好一堆子弹，并关掉它们
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false); // 关键：先隐藏
            pooledBullets.Add(obj); // 放入列表
        }
    }

    // --- 提供给外部的方法：从池子里拿子弹 ---
    public GameObject GetBullet()
    {
        for (int i = 0; i < pooledBullets.Count; i++)
        {
            // --- 新增防护逻辑 ---
            // 如果这个子弹不小心被 Destroy 了（变成了 null），不仅要跳过，最好还要补一个新的
            if (pooledBullets[i] == null)
            {
                GameObject obj = Instantiate(bulletPrefab);
                obj.SetActive(false);
                pooledBullets[i] = obj; // 补填这个坑
                return obj;
            }
            // ------------------

            if (!pooledBullets[i].activeInHierarchy)
            {
                return pooledBullets[i];
            }
        }
        return null;
    }
}