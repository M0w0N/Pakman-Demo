using UnityEngine;
using UnityEngine.UI;

public class LevelWinChecker : MonoBehaviour
{
    [Header("Level Config")]
    [Tooltip("确保拖入的是支持评分进度的最新 LevelConfig 资源")]
    public LevelConfig config;

    [Header("UI Bindings")]
    [Tooltip("用于显示【评分评级】的动态进度条 Slider")]
    public Slider progressSlider;
    [Tooltip("返回点标记（ReturnToStart模式）")]
    public GameObject returnPointMarker;
    [Tooltip("返回提示文字（ReturnToStart模式）")]
    public GameObject returnPointHint;

    [Header("Progress Bar Target Marker")]
    [Tooltip("静态标记，在进度条上标出ReturnToStart需要的硬性吃豆门槛。不需要可留空。")]
    public RectTransform targetMarker;
    [Tooltip("是否隐藏 Slider 的默认滑块")]
    public bool hideSliderHandle = true;

    // 内部联动组件
    private LevelStopwatch stopwatch;

    // 内部核心状态
    public float currentProgress = 0f; // 当前评分进度值
    private int totalPellets = 0;
    private int currentEaten = 0;
    private bool hasWon = false;
    private bool returnUnlocked = false;

    void Start()
    {
        if (config == null)
        {
            Debug.LogWarning("LevelWinChecker: 未分配 LevelConfig，脚本停用。");
            return;
        }

        // 1. 自动获取同物体上的秒表组件进行联动
        stopwatch = GetComponent<LevelStopwatch>();
        if (stopwatch == null)
        {
            Debug.LogWarning("LevelWinChecker: 未在同一物体上找到 LevelStopwatch 秒表脚本！");
        }

        // 2. 初始化评分数据
        totalPellets = config.totalPellets;
        currentProgress = config.initialRating; // 从配置表读取初始分（如100）

        // 3. 兼容处理返回点 UI 隐藏
        if (returnPointMarker == null) returnPointMarker = GameObject.Find("ReturnPointMarker");
        if (returnPointHint == null) returnPointHint = GameObject.Find("ReturnPointHint");
        if (returnPointMarker != null) returnPointMarker.SetActive(false);
        if (returnPointHint != null) returnPointHint.SetActive(false);

        // 4. 初始化 UI 评分进度条
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = config.maxRating; // 最大值为配置表上限
            progressSlider.value = currentProgress;

            if (hideSliderHandle && progressSlider.handleRect != null)
            {
                progressSlider.handleRect.gameObject.SetActive(false);
            }
        }

        // 5. 初始化吃豆硬性门槛标记 (仅用于 ReturnToStart)
        if (targetMarker != null && totalPellets > 0 && config.winCondition == LevelConfig.WinCondition.ReturnToStart)
        {
            // 注意：这里算的是【吃豆解锁进度】。如果是新版时间系统，你也可以删掉它，或者用来标记 S 级的硬性线
            float thresholdPct = (float)config.pelletsRequiredForReturn / totalPellets;
            RectTransform fillAreaContainer = progressSlider?.fillRect?.parent as RectTransform;
            if (fillAreaContainer != null)
            {
                targetMarker.SetParent(fillAreaContainer, false);
                targetMarker.pivot = new Vector2(0.5f, 0.5f);
                targetMarker.anchorMin = new Vector2(thresholdPct, 0.5f);
                targetMarker.anchorMax = new Vector2(thresholdPct, 0.5f);
                targetMarker.anchoredPosition = Vector2.zero;
            }
        }
        else if (targetMarker != null)
        {
            targetMarker.gameObject.SetActive(false); // 其他模式隐藏该硬性标记
        }

        Debug.Log($"LevelWinChecker: [{config.levelName}] 初始化成功，初始评分: {currentProgress}");
    }

    void Update()
    {
        if (config == null || hasWon) return;

        // 【核心联动逻辑 1】：随时间扣除进度条评分
        UpdateProgressDrain();

        // 计算吃了多少豆子（用于解锁 Return 终点）
        int remaining = GameObject.FindGameObjectsWithTag("Reward").Length;
        currentEaten = totalPellets - remaining;

        // 检查各种胜利条件的【解锁状态】
        switch (config.winCondition)
        {
            case LevelConfig.WinCondition.EatAllPellets:
                if (remaining <= 0) TriggerWin();
                break;

            case LevelConfig.WinCondition.ReturnToStart:
                if (currentEaten >= config.pelletsRequiredForReturn && !returnUnlocked)
                {
                    UnlockFinishPoint();
                }
                break;

            case LevelConfig.WinCondition.ReachExitTrigger:
                if (!returnUnlocked)
                {
                    UnlockFinishPoint(); // 走格子直接激活终点
                }
                break;
        }
    }

    /// <summary>
    /// 每帧执行：随时间倒扣评分，并更新 UI
    /// </summary>
    private void UpdateProgressDrain()
    {
        // 随时间减少
        currentProgress -= config.ratingDrainPerSecond * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, config.maxRating);

        // 更新 UI
        if (progressSlider != null)
        {
            progressSlider.value = currentProgress;
        }

        
       /* 惩罚机制：如果进度条彻底扣光，判定为失败（你可以自己决定要不要加这个）
        if (currentProgress <= 0f)
        {
            TriggerReplayHint();
        }
        */
    }

    /// <summary>
    /// 【核心联动逻辑 2】：当主角在外界吃豆子时，由主角脚本主动调用这个方法，补充评分
    /// </summary>
    public void OnPlayerEatPellet()
    {
        if (hasWon || config == null) return;

        // 加上配置表里的奖励分
        currentProgress += config.ratingGainPerPellet;
        currentProgress = Mathf.Clamp(currentProgress, 0f, config.maxRating);

        if (progressSlider != null)
        {
            progressSlider.value = currentProgress;
        }

        Debug.Log($"吃到豆子！当前评分上涨至: {currentProgress}");
    }

    private void UnlockFinishPoint()
    {
        returnUnlocked = true;
        if (returnPointMarker != null) returnPointMarker.SetActive(true);
        if (returnPointHint != null && config.winCondition == LevelConfig.WinCondition.ReturnToStart)
        {
            returnPointHint.SetActive(true);
        }
    }

    public void OnPlayerReachFinishPoint()
    {
        if (config == null || hasWon) return;
        if (!returnUnlocked) return;

        TriggerWin();
    }

    /// <summary>
    /// 触发胜利结算
    /// </summary>
    public void TriggerWin()
    {
        if (hasWon) return;
        hasWon = true;

        // 【核心联动逻辑 3】：通知秒表停止计时
        float finalTime = 0f;
        if (stopwatch != null)
        {
            stopwatch.StopTimer(); // 停止秒表
            finalTime = stopwatch.getTime(); // 拿到最终通关秒数
        }

        // 【核心联动逻辑 4】：根据最终剩余进度百分比计算评级
        float finalPct = (currentProgress / config.maxRating) * 100f;
        string finalRank = "C"; // 保底

        foreach (var rank in config.rankThresholds)
        {
            if (finalPct >= rank.minRatingPct)
            {
                finalRank = rank.rankName;
                break; // 匹配到最高符合的就跳出
            }
        }

        Debug.Log($"【通关结算】用时: {finalTime:F2}, 剩余评分比: {finalPct:F1}%, 最终评级: {finalRank}");

        // 【核心联动逻辑 5】：将最终评级和时间，传递给面板展示
        if (LevelClearPanel.Instance != null)
        {
            // 如果你的面板脚本支持传参，可以直接：
            LevelClearPanel.Instance.Show(finalRank, finalTime);
        }
        else
        {
            GameObject panel = GameObject.Find("Panel_LevelClear");
            if (panel != null)
            {
                panel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
    }

    /// <summary>
    /// 进度归零时的失败判定
    /// </summary>
    private void TriggerGameOver()
    {
        hasWon = true; // 拦截不再触发胜利
        if (stopwatch != null) stopwatch.StopTimer();

        Debug.Log("【游戏失败】评分进度条已耗尽！");
        // 这里可以打开你的失败面板，例如：GameOverPanel.Instance.Show();
        Time.timeScale = 0f;
    }
}