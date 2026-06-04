using UnityEngine;

public class LevelWinChecker : MonoBehaviour
{
    public enum WinCondition { EatAllPellets, ReachExitTrigger }

    [Header("通关条件选择")]
    public WinCondition winCondition = WinCondition.EatAllPellets;

    [Header("如果是【吃完豆子通关】")]
    [Tooltip("场景里剩余多少个豆子时算通关，默认是 0 个")]
    public int targetRemainingPellets = 0;

    // 内部计数器
    private int currentPelletCount = 0;
    private bool hasWon = false; // 防止重复触发通关逻辑

    void Start()
    {
        // 如果选了“吃豆子通关”，在关卡初始化时自动数一下场景里一共有多少个豆子
        if (winCondition == WinCondition.EatAllPellets)
        {
            CountPelletsInScene();
        }
    }

    void Update()
    {
        // 每一帧都在盯盘（如果选了吃豆子模式）
        if (winCondition == WinCondition.EatAllPellets && !hasWon)
        {
            // 重新清点场景中还剩多少个带有 "Reward" 标签的豆子
            // （因为主角吃豆子是用 Destroy 销毁的，所以豆子总数会自动减少）
            int remaining = GameObject.FindGameObjectsWithTag("Reward").Length;

            if (remaining <= targetRemainingPellets)
            {
                TriggerWin();
            }
        }
    }

    // 重新数豆子
    private void CountPelletsInScene()
    {
        currentPelletCount = GameObject.FindGameObjectsWithTag("Reward").Length;
        Debug.Log($"📊 关卡裁判：本关共检测到 {currentPelletCount} 个豆子。");
    }

    // 【核心通关出口】
    public void TriggerWin()
    {
        if (hasWon) return; // 如果已经赢了，直接拦截，防止一帧内触发多次
        hasWon = true;

        Debug.Log("🏁 关卡裁判：通关条件达成！正在通知全局常驻 UI...");

        // 联动上一问：远程呼叫常驻场景（Persistent）里的账本，让它把大弹窗啪地弹出来
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ShowLevelClearWindow();
        }
    }
}