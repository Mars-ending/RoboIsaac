using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏目标")]
    public int killsToSpawnPortal = 10; // 杀多少个怪出传送门？
    private int currentKills = 0;       // 当前杀了几个

    [Header("传送门设置")]
    public GameObject portalPrefab;     // 拖拽你的 VictoryMarker 预制体
    public Transform portalSpawnPoint;  // 拖拽一个空物体，决定传送门出现在哪
    


    [Header("UI")]
    public TextMeshProUGUI gameInfoText;// 中间的 "剩余敌人: 5"
    public TextMeshProUGUI playerStatsText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    // 1. 怪物死亡时调用
    public void OnEnemyKilled()
    {
        currentKills++;
        UpdateUI();

        // 检查是否达到了生成传送门的条件
        // 注意：只在刚好第 10 个的时候生成一次
        if (currentKills == killsToSpawnPortal)
        {
            SpawnPortal();
        }
    }

    void SpawnPortal()
    {
        if (portalPrefab != null && portalSpawnPoint != null)
        {
            Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);
            
            if (gameInfoText != null) 
            {
                gameInfoText.text = "Evacuation Point SHOW! SEEK!";
                gameInfoText.color = Color.yellow;
            }
            Debug.Log("传送门已生成！");
        }
    }

    // 2. 只有碰到传送门时才调用这个
    public void WinGame()
    {
        if (gameInfoText != null)
        {
            gameInfoText.text = "FINI! NEXT LEVEL...";
            gameInfoText.color = Color.green;
        }

        // 2秒后执行 LoadNextLevel
        Invoke("LoadNextLevel", 2f);
    }

    void LoadNextLevel()
    {
        // 1. 获取当前场景的编号 (Index)
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 2. 计算下一关的编号
        int nextSceneIndex = currentSceneIndex + 1;

        // 3. 检查有没有下一关
        // SceneManager.sceneCountInBuildSettings 是总场景数
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // 有下一关，加载它
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // 没有下一关了（通关了），回到第一关，或者去主菜单
            Debug.Log("恭喜通关所有关卡！回到第一关");
            SceneManager.LoadScene(0); // 0 就是 Level1
        }
    }



    void RestartGame()
    {
        // Time.timeScale = 1; // 如果暂停了记得恢复
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (gameInfoText != null && currentKills < killsToSpawnPortal)
        {
            gameInfoText.text = $"KILL: {currentKills} / {killsToSpawnPortal}";
        }
    }

    public void UpdateStatsUI(float speed, float damage)
    {
        if (playerStatsText != null)
        {
            // F1 表示保留1位小数，比如 5.0
            playerStatsText.text = $"Speed: {speed:F1} | Dmg: {damage}";
        }
    }
    
    // --- 兼容旧代码 (防止报错) ---
    // 你的 Spawner 可能会调用 RegisterEnemies，虽然现在我们按击杀数算，
    // 但留着这个空函数可以防止 Spawner 报错
    public void RegisterEnemies(int amount) { }
}