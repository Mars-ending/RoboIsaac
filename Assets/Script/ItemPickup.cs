using UnityEngine;


public class ItemPickup : MonoBehaviour

{
    [Header("道具效果")]
    public float sizeAmount = 2.0f; // 吃到后子弹变大几倍？
    public float damageAmount = 2.0f; // 吃到后伤害变多少？

    // 当有物体进入这个触发器时调用
    void OnTriggerEnter(Collider other)
    {
        // 1. 检查撞到的是不是玩家 (Tag)
        if (other.CompareTag("Player"))
        {
            // 2. 尝试获取玩家身上的脚本
            CubeController player = other.GetComponent<CubeController>();

            if (player != null)
            {
                // 3. 修改玩家的属性
                player.currentBulletSize = sizeAmount;
                player.currentDamage = damageAmount;

                Debug.Log("吃到道具！子弹变大了！");

                // 4. 吃完后销毁道具自己
                Destroy(gameObject);
            }
        }
    }
}