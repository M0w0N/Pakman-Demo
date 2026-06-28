using UnityEngine;
using TMPro; // 如果使用传统 Text，请换成 using UnityEngine.UI;

public class LevelStopwatch : MonoBehaviour
{
    [Header("UI 绑定")]
    [Tooltip("用于实时显示时间的文本组件")]
    [SerializeField] public TMP_Text timerText;

    [Header("设置")]
    [Tooltip("是否在 Start 时自动开始计时")]
    [SerializeField] private bool autoStart = true;

    // 属性：让其他脚本（如评级系统、结算面板）能直接获取最终用时
    public float ElapsedTime { get; private set; }

    private bool isRunning = false;

    private void Start()
    {
        if (autoStart)
        {
            StartTimer();
        }
    }

    private void Update()
    {
        if (!isRunning) return;

        // 累计叠加每帧流逝的时间
        ElapsedTime += Time.deltaTime;

        // 实时更新文本显示
        UpdateTimerDisplay();
    }

    /// <summary>
    /// 启动（或恢复）计时
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// 暂停/停止计时（通常在触发胜利 TriggerWin 或游戏失败时调用）
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// 重置秒表
    /// </summary>
    public void ResetTimer()
    {
        ElapsedTime = 0f;
        UpdateTimerDisplay();
    }

    public float getTime()
    {
        return ElapsedTime;
    }

    /// <summary>
    /// 将秒数格式化为 00:00.00 并更新到文字组件上
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        // 计算分、秒、毫秒
        int minutes = Mathf.FloorToInt(ElapsedTime / 60f);
        int seconds = Mathf.FloorToInt(ElapsedTime % 60f);
        int fraction = Mathf.FloorToInt((ElapsedTime * 100f) % 100f);

        // 格式化字符串，例如：01:23.45
        timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
    }
}