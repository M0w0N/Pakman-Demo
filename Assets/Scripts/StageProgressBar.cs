using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在局内，用于实时反应玩家当前评级/得分进度条（满条 -> 扣时间 -> 吃豆子涨条）
/// </summary>
public class StageProgressBar : MonoBehaviour
{
    [Header("UI 绑定")]
    [SerializeField] private Slider progressSlider;

    // 缓存对裁判或分数管理器的引用
    private LevelWinChecker winChecker;

    void Start()
    {
        if (progressSlider == null)
        {
            progressSlider = GetComponent<Slider>();
        }

        // 寻找局内的裁判组件
        winChecker = Object.FindFirstObjectByType<LevelWinChecker>();

        if (progressSlider != null && winChecker != null && winChecker.config != null)
        {
            // 如果你的配置表里设置了满分是 100 或者是 maxProgress
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 100f; // 或者写 winChecker.config.maxProgress;
            progressSlider.value = 100f;    // 刚开局满分
        }
    }

    void Update()
    {
        if (progressSlider == null || winChecker == null) return;

        // 核心进化：每帧直接去同步裁判脚本里算好的实时 Progress 数值！
        // 裁判负责根据配置（每秒扣X点，吃豆加X点）算分，进度条只负责把它画出来。
        // 假设你在 LevelWinChecker 暴露了一个 public float currentProgress 属性
        progressSlider.value = winChecker.currentProgress;
    }
}
