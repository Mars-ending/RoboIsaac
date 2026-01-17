using UnityEngine;
using UnityEngine.SceneManagement; //以此来加载主菜单

public class PauseMenuController : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject pausePanel; // 把 PauseMenuPanel 拖进来

    // 一个变量记录当前是不是暂停状态
    public static bool isGamePaused = false;

    void Update()
    {
        // 监听 ESC 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused)
            {
                ResumeGame(); // 如果已经暂停，就继续
            }
            else
            {
                PauseGame(); // 如果没暂停，就暂停
            }
        }
    }

    // --- 功能 1：暂停游戏 ---
    void PauseGame()
    {
        pausePanel.SetActive(true); // 显示菜单
        
        //  核心：把时间流速设为 0，物理和动画都会静止
        Time.timeScale = 0f; 
        
        isGamePaused = true;

        //  必须解锁鼠标，否则你点不到按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- 功能 2：继续游戏 (绑定给“继续”按钮) ---
    public void ResumeGame()
    {
        pausePanel.SetActive(false); // 隐藏菜单
        
        // ▶️ 核心：把时间恢复为 1
        Time.timeScale = 1f;
        
        isGamePaused = false;

        //  重新锁定鼠标，继续战斗
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- 功能 3：返回主菜单 (绑定给“结束”按钮) ---
    public void QuitToMenu()
    {
        
        // 如果在暂停状态下直接换场景，时间流速可能还是 0。
        // 导致你进入下一关或者主菜单时，游戏是动不了的。
        // 所以离开前必须恢复时间！
        Time.timeScale = 1f; 
        isGamePaused = false;

        Debug.Log("返回主菜单...");
        SceneManager.LoadScene(0); // 假设你的主菜单 Build Index 是 0
    }
    
    // (可选) 直接退出桌面
    public void QuitDesktop()
    {
        Debug.Log("退出桌面");
        Application.Quit();
    }
}