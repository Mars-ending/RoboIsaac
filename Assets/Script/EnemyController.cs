using UnityEngine;
using System.Collections; // 引用协程

public class EnemyController : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHP = 5;
    private int currentHP;
    public int damageToPlayer = 1;

    [Header("AI 移动设置")]
    public float moveSpeed = 3.0f;      
    public float chaseRange = 10.0f;    
    public float wanderTime = 2.0f;     
    
    // 内部变量
    private Transform playerTransform;  
    private Rigidbody rb;
    private Animator animator; // --- 【新增】动画控制器 ---
    private float timer;                
    private Vector3 wanderDirection;    
    private bool isDead = false; // --- 【新增】死亡状态锁 ---

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // --- 【修复跌倒的关键 1】代码强制锁定旋转 ---
        // 这样怪物就永远不会“摔倒”了，只会水平移动和旋转
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            // 注意：如果你需要受重力影响掉下悬崖，就去掉 FreezePositionY，但必须保留 FreezeRotation
            rb.constraints = RigidbodyConstraints.FreezeRotation; 
        }

        // --- 【新增】自动获取子物体上的 Animator ---
        animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void OnEnable()
    {
        currentHP = maxHP;
        isDead = false; // 复活
        timer = wanderTime;
        GetNewWanderDirection();
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; 
            rb.angularVelocity = Vector3.zero;
        }

        // 重置动画状态 (防止复活时卡在死亡动画里)
        if (animator != null)
        {
            animator.Rebind(); 
            animator.Update(0f);
        }
    }

    void FixedUpdate()
    {
        // --- 【新增】如果你死了，就别动了 ---
        if (isDead) return;

        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // 状态判断
        if (distance < chaseRange)
        {
            ChasePlayer();
        }
        else
        {
            Wander();
        }

        // --- 【新增】驱动动画 ---
        UpdateAnimation();
    }

    // --- 【新增】专门处理动画参数 ---
    void UpdateAnimation()
    {
        if (animator != null)
        {
            // 获取当前实际速度 (只看水平速度，忽略Y轴)
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            float currentSpeed = horizontalVelocity.magnitude;

            // 传给 Animator (对应 Blend Tree 的 Speed 参数)
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    void ChasePlayer()
    {
        Vector3 targetPos = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        transform.LookAt(targetPos);

        rb.linearVelocity = transform.forward * moveSpeed;
        
        // 保持原本的 Y 轴速度 (重力)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    void Wander()
    {
        timer -= Time.deltaTime; 

        if (timer <= 0)
        {
            GetNewWanderDirection();
            timer = wanderTime; 
        }

        // 巡逻速度慢一点 (0.5倍)
        rb.linearVelocity = wanderDirection * (moveSpeed * 0.5f);
    }

    void GetNewWanderDirection()
    {
        float x = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        
        wanderDirection = new Vector3(x, 0, z).normalized;
        
        if (wanderDirection != Vector3.zero)
        {
            // 使用平滑旋转，防止怪物瞬间转头看起来很生硬
            Quaternion lookRot = Quaternion.LookRotation(wanderDirection);
            transform.rotation = lookRot; 
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return; // 死了就不判定的碰撞了

        if (collision.gameObject.CompareTag("Player"))
        {
            CubeController player = collision.gameObject.GetComponent<CubeController>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
                // 撞人后稍微后退一点
                Vector3 pushDir = (transform.position - collision.transform.position).normalized;
                rb.AddForce(pushDir * 2f, ForceMode.Impulse);
            }
        }

        BulletController bullet = collision.gameObject.GetComponent<BulletController>();
        if (bullet != null)
        {
            TakeDamage(bullet.damage);
            bullet.gameObject.SetActive(false); 
        }
    }

    void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHP -= (int)amount;
        
        // 可选：播放受击动画
        // if (animator != null) animator.SetTrigger("TriggerHit");

        if (currentHP <= 0) Die();
    }

    void Die()
    {
        if (isDead) return; // 防止死两次
        isDead = true;

        if (GameManager.Instance != null) GameManager.Instance.OnEnemyKilled();

        // 1. 停止物理运动
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; // 彻底停止物理计算
        GetComponent<Collider>().enabled = false; // 关闭碰撞体，防止尸体挡路

        // 2. 播放死亡动画
        if (animator != null)
        {
            animator.SetTrigger("TriggerDie");
        }

        // 3. 延迟回收 (等待动画播完)
        StartCoroutine(RecycleDelay());
    }

    IEnumerator RecycleDelay()
    {
        // 假设死亡动画大概 2 秒
        yield return new WaitForSeconds(2.0f);

        // 复原状态 (为了下次从池子里出来时是好的)
        rb.isKinematic = false; 
        GetComponent<Collider>().enabled = true;
        
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}