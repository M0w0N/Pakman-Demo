using UnityEngine;

/// <summary>
/// 挂在起点返回区域的 Trigger 碰撞体上。
/// 玩家踩到且解锁条件已满足时，通知 LevelWinChecker 通关。
/// </summary>
public class ReturnPointTrigger : MonoBehaviour
{
    private void Awake()
    {
        // 自检：必须具备 Collider2D 并勾选 IsTrigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError($"ReturnPointTrigger on [{gameObject.name}]: missing Collider2D!", this);
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"ReturnPointTrigger on [{gameObject.name}]: Collider2D found but IsTrigger is NOT checked!", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"ReturnPointTrigger: [{gameObject.name}] OnTriggerEnter2D triggered by [{other.name}] tag=[{other.tag}]");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"ReturnPointTrigger: ignoring non-Player tag [{other.tag}]");
            return;
        }

        LevelWinChecker checker = FindObjectOfType<LevelWinChecker>();
        if (checker == null)
        {
            Debug.LogError("ReturnPointTrigger: FindObjectOfType<LevelWinChecker>() returned null!");
            return;
        }

        Debug.Log("ReturnPointTrigger: calling OnPlayerReachReturnPoint()...");
        checker.OnPlayerReachReturnPoint();
    }
}
