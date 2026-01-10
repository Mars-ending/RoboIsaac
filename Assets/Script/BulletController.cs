using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 1f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // --- 必须用 OnEnable 代替 Start ---
    // 因为子弹是反复使用的，Start 只会在第一次出生时执行一次
    // OnEnable 每次被激活（SetActive true）都会执行
    void OnEnable()
    {
        // 1. 重置“自动销毁”倒计时
        // 以前是 Destroy(gameObject, lifeTime)
        // 现在必须用协程来手动计时
        StartCoroutine(DeactivateAfterTime());

        // 2. 重置物理状态
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero; // 清除之前的惯性！(Unity 6用linearVelocity，旧版用velocity)
            rb.angularVelocity = Vector3.zero; // 清除旋转惯性
            
            // 3. 重新给速度
            rb.linearVelocity = transform.forward * speed; 
            // 注意：如果你是旧版Unity，请把上面的 linearVelocity 改回 velocity
        }
    }

    // 外部设置属性的方法
    public void Setup(float sizeMultiplier, float newDamage)
    {
        transform.localScale = Vector3.one * 0.2f * sizeMultiplier;
        damage = newDamage;
    }

    void OnCollisionEnter(Collision collision)
    {
        // --- 错误写法 (绝对禁止!) ---
        // Destroy(gameObject); 

        // --- 正确写法 (回收) ---
        gameObject.SetActive(false);
    }

    // 倒计时协程
    IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        
        // 时间到了还没撞到东西？回收自己
        gameObject.SetActive(false);
    }
}