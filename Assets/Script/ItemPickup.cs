using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("这个物品增加多少属性？(0代表不加)")]
    public float addMoveSpeed = 0f;    // 增加移动速度
    public float addBulletSpeed = 0f;  // 增加子弹飞行速度
    public float addBulletSize = 0f;   // 增加子弹变大 (比如 0.5)
    public int addDamage = 0;          // 增加伤害

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CubeController player = other.GetComponent<CubeController>();
            if (player != null)
            {
                // 把面板上填的所有数值都传给主角
                player.GetBuff(addMoveSpeed, addBulletSpeed, addBulletSize, addDamage);
                
                // 吃掉后消失
                Destroy(gameObject);
            }
        }
    }
}