using UnityEngine;

public class VictoryTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // 检查是不是主角碰到了我
        // 前提：你的主角必须有 "Player" 这个标签 (Tag)
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家到达撤离点！");
            
            // 调用 GameManager 的胜利逻辑
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }
}