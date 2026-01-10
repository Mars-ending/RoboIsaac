using UnityEngine;
using UnityEngine.UI; // 需要控制UI
using UnityEngine.SceneManagement; // 需要重置场景

public class GameManager : MonoBehaviour
{
    // 单例模式：让其他脚本能轻松找到这个裁判
    public static GameManager Instance;

    [Header("游戏设置")]
    public Text gameInfoText; // 用来显示 "你赢了" 或 "剩余敌人: X"
    private int enemyCount = 0; // 剩下的敌人数量

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. 游戏开始时，自动数一下场上有多少个敌人
        // 注意：这要求你的怪物必须都有 "Enemy" 这个标签 (Tag)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = enemies.Length;

        UpdateEnemyUI();
    }

    // --- 供怪物死的时候调用 ---
    public void OnEnemyKilled()
    {
        enemyCount--; // 数量减 1
        UpdateEnemyUI();

        // 检查是不是赢了
        if (enemyCount <= 0)
        {
            WinGame();
        }
    }

    void UpdateEnemyUI()
    {
        if (gameInfoText != null)
        {
            gameInfoText.text = "剩余敌人: " + enemyCount;
        }
    }

    void WinGame()
    {
        Debug.Log("你赢了！");
        if (gameInfoText != null)
        {
            gameInfoText.text = "VICTORY! (2秒后重开)";
            gameInfoText.color = Color.green;
        }

        // 2秒后重新开始，让你享受一下胜利的喜悦
        Invoke("RestartGame", 2f);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}