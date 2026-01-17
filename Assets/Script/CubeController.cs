using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections; // 引用协程
using TMPro;
public class CubeController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    
    [Header("跳跃设置")]
    public int maxJumpCount = 2;
    private int currentJumpCount = 0;

    // --- 【新增】地面检测变量 ---
    [Header("地面检测 (必须设置)")]
    public Transform groundCheck;   // 拖入脚底的空物体
    public LayerMask groundMask;    // 设置为 Ground 层
    public float groundDistance = 0.2f; // 检测半径
    private bool isGrounded;        // 存储检测结果

    [Header("视角设置")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.0f;

    [Header("战斗与动画设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 5.0f;     
    private float nextFireTime = 0f;  
    
    // --- 【新增】投掷延迟 ---
    public float throwDelay = 0.3f; // 手挥出去0.3秒后球才出来

    [Header("组件")]
    private Rigidbody rb;
    private Animator animator; // --- 【新增】动画控制器 ---
    private float xRotation = 0f;

    [Header("生命值设置")]
    public int maxHP = 3;
    private int currentHP;
    public TextMeshProUGUI hpText;
    
    [Header("无敌时间设置")]
    public float invincibilityDuration = 1.0f;
    private bool isInvincible = false;
    public GameObject bodyModel;

    [Header("当前属性")]
    public float currentDamage = 1.0f;

    // --- 子弹属性 ---
    public float currentBullSpeed = 20f; 
    public float currentBulletSize = 1.0f; 

    private Camera mainCamera;

    [Header("音效设置")]
    public AudioSource audioSource; // 拖入主角身上的 AudioSource
    public AudioClip throwSound;    // 投掷音效
    public AudioClip runSound;      // 跑步音效

    // 内部变量：防止跑步声音重复播放
    private bool isRunningSoundPlaying = false;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
        UpdateUI();
        
        rb = GetComponent<Rigidbody>();
        
        // --- 【新增】自动获取子物体上的 Animator ---
        animator = GetComponentInChildren<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>().transform;

        mainCamera = Camera.main;
    }

    public void GetBuff(float addMoveSpeed, float addBulletSpeed, float addBulletSize, int addDamage)
    {
        moveSpeed += addMoveSpeed;
        currentBullSpeed += addBulletSpeed;
        currentBulletSize += addBulletSize;
        currentDamage += addDamage;

        UpdateUI();
        Debug.Log("吃到增益！当前属性 updated.");
    }

    void UpdateUI()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateStatsUI(moveSpeed, (int)currentDamage);
        }
    }

    void Update()
    {


        if (PauseMenuController.isGamePaused) return;

        // ------------- 1. 跑步音效控制 -------------
        // 判断条件：有速度 且 在地面上
        // (假设你用 CharacterController 或者 Rigidbody 判断了 ground)
        bool isMoving = rb.linearVelocity.magnitude > 0.1f; // 如果用的 Unity 6+ linearVelocity
        
        if (isMoving)
        {
            if (!audioSource.isPlaying) 
            {
                // 如果没有在播放，就开始播放跑步声
                // 注意：这种写法只适合 runSound 本身是循环的素材
                // 或者用 audioSource.PlayOneShot(runSound) 配合计时器（稍微复杂点）
                
                // 简单方案：直接赋值并播放
                audioSource.clip = runSound;
                audioSource.loop = true; // 跑步需要循环
                audioSource.Play();
            }
        }
        else
        {
            // 如果停下来了，且正在播放跑步声，就停止
            if (audioSource.clip == runSound && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false; // 关掉循环，以免影响投掷
            }
        }

        // ------------- 2. 投掷音效 (放在你的输入检测里) -------------
        if (Input.GetMouseButtonDown(0)) 
        {
            // ... 你的投掷逻辑 ...
            PlayThrowSound();
        }
    




        // =========================================================
        // 1. 地面检测 (物理球体检测)
        // =========================================================
        // 如果 groundCheck 还没赋值，防止报错，先跳过
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // 如果在地面且速度向下 (说明踩实了)，重置二段跳
        if (isGrounded && rb.linearVelocity.y <= 0.1f) 
        {
            currentJumpCount = 0;
        }

        // =========================================================
        // 2. 更新动画参数
        // =========================================================
        if (animator != null)
        {
            // A. 速度 (前后移动 W/S)
            float v = Input.GetAxis("Vertical"); // 获取 W/S 输入 (-1 到 1)
            // 这里我们稍微改一下，让后退也播放走路动画（取绝对值，或者你可以加后退动画）
            float targetSpeed = Mathf.Abs(v); 
            // 如果按了 Shift 加速，可以乘以一个倍率，这里暂时简单处理
            if (Mathf.Abs(v) > 0.1f) targetSpeed = 1f; // 只要按 W/S 就设为跑动级别(1)，你可以根据需求改成 0.5

            animator.SetFloat("Speed", targetSpeed, 0.1f, Time.deltaTime);

            // --- 【新增】B. 转向 (左右移动 A/D) ---
            float h = Input.GetAxis("Horizontal"); // 获取 A/D 输入 (-1 到 1)
            // 把这个值传给 Animator 的 Turn 参数
            animator.SetFloat("Turn", h, 0.1f, Time.deltaTime); // 加了 0.1f 的阻尼让切换更平滑

            // C. 空中状态
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
        // =========================================================
        // 3. 跳跃输入
        // =========================================================
        if (Input.GetButtonDown("Jump") && currentJumpCount < maxJumpCount) 
        {
            PerformJump();
        }

        // 4. 掉落死亡
        if (transform.position.y < -10f) Die();

        // =========================================================
        // 5. 视角控制 (保持不变)
        // =========================================================
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        if (playerCamera != null)
        {
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -45f, 45f);
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // =========================================================
        // 6. 射击 (修改为动画驱动)
        // =========================================================
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;

            // --- 【修改】不再直接 Shoot，而是开始投掷流程 ---
            StartThrowSequence();
        }
        Vector3 currentRot = transform.rotation.eulerAngles;
    
    // 如果发现 X 或 Z 歪了 (不是 0)，强制设回 0
    // 我们只保留 Y 轴 (左右转动)，把 X 和 Z 归零
    if (Mathf.Abs(currentRot.x) > 0.1f || Mathf.Abs(currentRot.z) > 0.1f)
    {
        // 重新赋值旋转：X=0, Y=保持当前, Z=0
        transform.rotation = Quaternion.Euler(0, currentRot.y, 0);
    }


    }


    
    void PlayThrowSound()
    {
        if (throwSound != null)
        {
            // PlayOneShot 的好处是：它可以和正在播放的跑步声“混音”，不会打断跑步声
            audioSource.PlayOneShot(throwSound);
        }
    }




    void FixedUpdate()
    {
        // 保持你原有的逻辑不变
        float h = Input.GetAxis("Horizontal"); 
        float v = Input.GetAxis("Vertical"); 

        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.transform.forward;
            Vector3 camRight = mainCamera.transform.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * v + camRight * h;

            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }

        // ==========================================
        // 强制把旋转速度 (角速度) 设为 0
        rb.angularVelocity = Vector3.zero;

    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        currentJumpCount++;
        
        // --- 【新增】刚跳起来强制设为 false，防止动画这一帧还没反应过来
        if (animator != null) animator.SetBool("IsGrounded", false);
    }

    // --- 【新增】投掷动画流程 ---
    void StartThrowSequence()
    {
        // 1. 播放动画
        if (animator != null)
        {
            animator.SetTrigger("TriggerThrow");
        }

        // 2. 开启协程延迟发射
        StartCoroutine(SpawnBulletWithDelay(throwDelay));
    }

    // --- 【新增】延迟发射协程 ---
    IEnumerator SpawnBulletWithDelay(float delay)
    {
        // 等待 delay 秒 (配合动作挥手的时间)
        yield return new WaitForSeconds(delay);

        // 时间到，球飞出去
        Shoot();
    }

    // 这是真正的发射逻辑 (保持不变)
    void Shoot()
    {
        if (firePoint == null) return;

        GameObject bulletObj = BulletPool.Instance.GetBullet();

        if (bulletObj != null)
        {
            bulletObj.transform.position = firePoint.position;
            bulletObj.transform.rotation = firePoint.rotation;
            bulletObj.SetActive(true);

            BulletController bulletScript = bulletObj.GetComponent<BulletController>();
            if (bulletScript != null) 
            {
                bulletScript.Setup(currentBulletSize, (int)currentDamage);
            }

            Rigidbody rbBull = bulletObj.GetComponent<Rigidbody>();
            if (rbBull != null)
            {
                rbBull.linearVelocity = Vector3.zero; 
                rbBull.angularVelocity = Vector3.zero;
                rbBull.linearVelocity = firePoint.forward * currentBullSpeed;
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        // 如果死了或者正在无敌，就不受伤
        if (isInvincible || currentHP <= 0) return;

        currentHP -= damageAmount;
        UpdateHPUI();

        // 播放受击动画 (如果还没死)
        if (currentHP > 0 && animator != null)
        {
            animator.SetTrigger("TriggerHurt");
        }

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            // 只有活着的时候才闪烁
            StartCoroutine(BecomeInvincible());
        }
    }

    void UpdateHPUI()
    {
        if (hpText != null)
        {
            hpText.text = "HP: " + currentHP + " / " + maxHP;
        }
    }

    

    void Die()
    {
        Debug.Log("角色死亡！");
        
        // 1. 播放死亡动画
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            // 也是为了保险，触发一次 Trigger 防止它卡在 Hurt 状态
            // (有时候刚受击就死了，Animator需要强制切换)
        }

        // 2. 禁用控制 (防止尸体滑步)
        // 禁用这个脚本的 Update，你就不能动了
        this.enabled = false; 
        
        // 禁用碰撞体 (防止尸体挡路)
        GetComponent<Collider>().enabled = false;
        rb.isKinematic = true; // 停止物理运算

        // 3. 延迟几秒后重置场景 (等待动画播完)
        StartCoroutine(RestartLevelDelay());
    }

    IEnumerator RestartLevelDelay()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // -------------------------------------------------------------
    // 修改 3：修复 Bug 的无敌闪烁
    // -------------------------------------------------------------
    IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        // 【关键修复】
        // 我们不找 GetComponent<Renderer>()，因为那可能是 Cube 的方块渲染器
        // 我们要找 bodyModel (你的新人物) 里面的所有渲染器 (包括皮肤、武器、盔甲)
        Renderer[] charRenderers = null;

        if (bodyModel != null)
        {
            // 获取人物模型下所有的渲染器 (蒙皮的、普通的都算)
            charRenderers = bodyModel.GetComponentsInChildren<Renderer>();
        }

        // 闪烁 5 次
        for (int i = 0; i < 5; i++)
        {
            // 变透明 (关掉显示)
            if (charRenderers != null)
            {
                foreach (var r in charRenderers) r.enabled = false;
            }
            
            yield return new WaitForSeconds(0.1f);

            // 变回来 (开启显示)
            if (charRenderers != null)
            {
                foreach (var r in charRenderers) r.enabled = true;
            }

            yield return new WaitForSeconds(0.1f);
        }

        isInvincible = false;
    }
}