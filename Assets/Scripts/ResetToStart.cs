using UnityEngine;

public class ResetToStart : MonoBehaviour
{
    [Header("重置设置")]
    [Tooltip("这里拖入玩家角色，如果挂在角色身上则留空，会自动获取")]
    public GameObject playerObject;

    private Vector2 startPosition;
    private Rigidbody2D playerRb;
    

    void Start()
    {
        // 1. 如果没有手动指定物体，默认获取脚本所在的物体
        if (playerObject == null)
        {
            playerObject = this.gameObject;
        }

        // 2. 记录游戏刚开始时，角色的初始黄金坐标作为“起点”
        startPosition = playerObject.transform.position;

        // 3. 缓存角色的物理与控制组件
        playerRb = playerObject.GetComponent<Rigidbody2D>();
        
    }

    void Update()
    {
        // 4. 监听键盘 R 键 (Reset)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }
    }

    private void ResetPlayer()
    {
        if (playerObject == null) return;

        Debug.Log("🔄 摁下 R 键：已清除物理惯性，角色传送回起点！");

        // 【核心操作】防止传送后由于残留速度直接穿墙或暴冲
        if (playerRb != null)
        {
            // 1. 瞬间将刚体速度归零（清除下坠惯性和反弹惯性）
            playerRb.linearVelocity = Vector2.zero;

            // 2. 瞬间清除刚体可能残留的旋转角速度
            playerRb.angularVelocity = 0f;

            // 3. 通过刚体进行安全的物理传送
            playerRb.position = startPosition;
        }
        else
        {
            // 如果物体没刚体，再使用常规传送
            playerObject.transform.position = startPosition;
        }

        // 【状态重置】通知引力脚本：重置下坠状态，让鼠标重新捕捉
        
    }
}