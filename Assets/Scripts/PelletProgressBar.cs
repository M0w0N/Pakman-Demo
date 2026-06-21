using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在关卡 Canvas 下的 Slider 上，实时显示豆子收集进度（已吃/总数）。
/// </summary>
public class PelletProgressBar : MonoBehaviour
{
    [Header("UI 绑定")]
    [Tooltip("拖入场景里的 Slider 组件，留空则自动获取当前物体上的 Slider")]
    public Slider progressSlider;

    [Header("设置")]
    [Tooltip("每多少秒刷新一次进度，默认 0.2 秒")]
    public float refreshInterval = 0.2f;

    private int totalPellets = -1;
    private float timer = 0f;

    void Start()
    {
        if (progressSlider == null)
        {
            progressSlider = GetComponent<Slider>();
        }

        // 找场景里豆子总数（只做一次）
        totalPellets = GameObject.FindGameObjectsWithTag("Reward").Length;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
    }

    void Update()
    {
        if (progressSlider == null || totalPellets <= 0) return;

        timer += Time.deltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            RefreshProgress();
        }
    }

    private void RefreshProgress()
    {
        int remaining = GameObject.FindGameObjectsWithTag("Reward").Length;
        int eaten = totalPellets - remaining;
        float progress = (float)eaten / totalPellets;
        progressSlider.value = Mathf.Clamp01(progress);
    }

    /// <summary>
    /// 供外部调用：关卡重开时重新计数
    /// </summary>
    public void ResetBar()
    {
        totalPellets = GameObject.FindGameObjectsWithTag("Reward").Length;
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
    }
}
