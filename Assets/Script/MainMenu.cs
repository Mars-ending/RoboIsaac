using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenu : MonoBehaviour
{
    [Header("UI 面板引用")]
    public GameObject mainPanel;   // 对应你的 "panel" (主界面)
    public GameObject bridgePanel; // 对应你的 "Bridge" (任务说明界面)

    void Start()
    {
        // 游戏启动时，确保主页显示，任务页隐藏
        // 这样你在编辑器里乱开乱关也没事，运行会自动复位
        if (mainPanel != null) mainPanel.SetActive(true);
        if (bridgePanel != null) bridgePanel.SetActive(false);
    }

    // --- 函数 1：给主页 "panel" 上的【开始游戏】按钮用 ---
    public void ShowBriefing()
    {
        // 关闭主页，打开任务说明
        mainPanel.SetActive(false);
        bridgePanel.SetActive(true);
    }

    // --- 函数 2：给任务页 "Bridge" 上的【出发/GO】按钮用 ---
    public void StartGame()
    {
        // 这里才真正加载场景
        // 请确保 Build Settings 里你的游戏场景叫 "GameScene" 或者是索引 1
        // 建议用索引：SceneManager.LoadScene(1);
        SceneManager.LoadScene("GameScene"); 
    }

    // --- 函数 3：给【退出】按钮用 ---
    public void QuitGame()
    {
        Debug.Log("退出游戏！");
        Application.Quit();
    }
}