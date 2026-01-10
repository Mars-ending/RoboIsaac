using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    
    [Header("跳跃设置")]
    public int maxJumpCount = 2;
    private int currentJumpCount = 0;

    [Header("视角设置")]
    public Transform playerCamera;
    public float mouseSensitivity = 2.0f;

    [Header("战斗设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Rigidbody rb;
    private float xRotation = 0f;

    [Header("生命值设置")]
    public int maxHP = 3;
    private int currentHP;
    public Text hpText;
    
    [Header("无敌时间设置")]
    public float invincibilityDuration = 1.0f;
    private bool isInvincible = false;
    public GameObject bodyModel;

    [Header("当前属性")]
    public float currentBulletSize = 1.0f;
    public float currentDamage = 1.0f;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
        
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>().transform;
    }

    void Update()
    {
        // 1. 重置跳跃次数 (注意：改回了 velocity)
        if (IsGrounded() && rb.linearVelocity.y <= 0.1f) 
        {
            currentJumpCount = 0;
        }

        // 2. 跳跃输入
        if (Input.GetButtonDown("Jump"))
        {
            if (currentJumpCount < maxJumpCount) PerformJump();
        }

        // 3. 射击输入 (J键)
        if (Input.GetKeyDown(KeyCode.J)) 
        {
            Shoot(); // 调用射击
        }

        // --- 【新增】掉落检测 ---
        // 如果高度低于 -10 (假设地面是 0)，直接判死
        if (transform.position.y < -10f)
        {
            Die(); // 直接调用现有的死亡逻辑
        }
        // ----------------------



        // 4. 鼠标视角
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 【注意】我删除了这里原本重复的一大段代码，那是导致错误的根源
    }

    void FixedUpdate()
    {
        // 5. 物理移动 (注意：改回了 velocity)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }

    void PerformJump()
    {
        // 改回 velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        currentJumpCount++;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }

    void Shoot()
    {
        // 旧写法：Instantiate(...)
        
        // --- 新写法：从池子拿 ---
        GameObject bulletObj = BulletPool.Instance.GetBullet();

        if (bulletObj != null)
        {
            // 1. 设置位置和旋转 (因为是复用的旧子弹，它的位置还在上次消失的地方)
            bulletObj.transform.position = firePoint.position;
            bulletObj.transform.rotation = firePoint.rotation;

            // 2. 激活它！(这会触发子弹脚本里的 OnEnable)
            bulletObj.SetActive(true);

            // 3. 传参数
            BulletController bulletScript = bulletObj.GetComponent<BulletController>();
            if (bulletScript != null) 
            {
                bulletScript.Setup(currentBulletSize, currentDamage);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible) return;

        currentHP -= damageAmount;
        Debug.Log($"主角受伤！剩余血量: {currentHP}");
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
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

    System.Collections.IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        Renderer myRenderer = GetComponent<Renderer>(); 
        if (bodyModel != null) myRenderer = bodyModel.GetComponent<Renderer>();

        for (int i = 0; i < 5; i++)
        {
            if(myRenderer != null) myRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            if(myRenderer != null) myRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("游戏结束！");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
