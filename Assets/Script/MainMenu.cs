using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用

public class MainMenu : MonoBehaviour
{
    // 点击按钮时调用这个函数
    public void PlayGame()
    {
        // 加载你的游戏场景名字（请确保你的游戏场景叫 GameScene 或者你自己起的名字）
        SceneManager.LoadScene("GameScene"); 
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏！");
        Application.Quit();
    }
}