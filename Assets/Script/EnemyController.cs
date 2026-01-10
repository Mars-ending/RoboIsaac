using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHP = 5;
    private int currentHP;
    public int damageToPlayer = 1;

    [Header("AI 移动设置")]
    public float moveSpeed = 3.0f;      // 怪物移动速度
    public float chaseRange = 10.0f;    // 警戒范围：小于这个距离就开始追你
    public float wanderTime = 2.0f;     // 每隔几秒换一个随机方向
    
    // 内部变量
    private Transform playerTransform;  // 记录主角的位置
    private Rigidbody rb;
    private float timer;                // 计时器
    private Vector3 wanderDirection;    // 当前随机溜达的方向

    void Awake()
    {
        // 获取组件这种只需要做一次的事情，放在 Awake
        rb = GetComponent<Rigidbody>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void OnEnable()
    {
        // 1. 必须重置血量！否则它出来就是尸体
        currentHP = maxHP;
        
        // 2. 重置状态
        timer = wanderTime;
        GetNewWanderDirection();
        
        // 3. 消除物理惯性 (防止它保留着上次死掉时被击飞的速度)
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; // Unity 6 写法，旧版用 velocity
            rb.angularVelocity = Vector3.zero;
        }
    }


    

    void FixedUpdate()
    {
        // 如果主角死了或者不存在，就呆在原地
        if (playerTransform == null) return;

        // 2. 计算怪物和主角的距离
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < chaseRange)
        {
            // --- 情况 A：主角在范围内 -> 追！ ---
            ChasePlayer();
        }
        else
        {
            // --- 情况 B：主角太远 -> 随机溜达 ---
            Wander();
        }
        if (playerTransform == null) return;
        float distance2 = Vector3.Distance(transform.position, playerTransform.position);
        if (distance2 < chaseRange) ChasePlayer();
        else Wander();
    }

    // 追逐逻辑
    void ChasePlayer()
    {
        // 1. 看向主角 (这就有了“盯”着你的效果)
        // transform.LookAt(目标位置)
        // 我们只希望它水平旋转，不希望它抬头看天或低头看地，所以把 y 轴固定
        Vector3 targetPos = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        transform.LookAt(targetPos);

        // 2. 向前冲 (因为已经看向主角了，transform.forward 就是主角的方向)
        rb.linearVelocity = transform.forward * moveSpeed;
        
        // 保留垂直方向的速度（如果有重力的话），防止悬空
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.GetComponent<Rigidbody>().linearVelocity.y, rb.linearVelocity.z);
    }

    // 巡逻逻辑
    void Wander()
    {
        timer -= Time.deltaTime; // 倒计时

        // 时间到了，换个新方向
        if (timer <= 0)
        {
            GetNewWanderDirection();
            timer = wanderTime; // 重置计时器
        }

        // 沿着设定好的方向移动
        rb.linearVelocity = wanderDirection * (moveSpeed * 0.5f); // 巡逻时速度慢一点
    }

    // 生成随机方向的小工具
    void GetNewWanderDirection()
    {
        // 随机生成 x 和 z (-1 到 1 之间)
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        
        // 归一化，保证方向长度是 1，不会因为斜着走就变快
        wanderDirection = new Vector3(x, 0, z).normalized;
        
        // 让怪物看向它要走的方向 (看起来更自然)
        if (wanderDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(wanderDirection);
        }
    }

    // --- 碰撞与受伤逻辑 (保持不变) ---
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CubeController player = collision.gameObject.GetComponent<CubeController>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
                // 简单的反弹
                Vector3 pushDir = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pushDir * 5f, ForceMode.Impulse);
            }
        }

        BulletController bullet = collision.gameObject.GetComponent<BulletController>();
        if (bullet != null)
        {
            TakeDamage(bullet.damage);
            bullet.gameObject.SetActive(false); // 记得用 SetActive 配合对象池
        }
    }

    void TakeDamage(float amount)
    {
        currentHP -= (int)amount;
        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled();
        Destroy(gameObject);
    }

    // --- 【新增】调试画线功能 ---
    // 这个函数只有在 Unity 编辑器里才生效，游戏里看不见
    // 它可以帮你画出“警戒范围”的红圈
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}