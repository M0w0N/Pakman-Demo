using TMPro;
using UnityEngine;

/// <summary>
/// 挂在常驻场景，管理关卡结算弹窗的显示/隐藏。
/// 不依赖 LevelWinChecker，任何地方都可调用。
/// </summary>
public class LevelClearPanel : MonoBehaviour
{
    public static LevelClearPanel Instance { get; private set; }
    private LevelStopwatch stopwatch;
    private TMP_Text timerText;

    [Header("UI 绑定")]
    [Tooltip("结算弹窗根 Panel")]
    public GameObject panelLevelClear;
    [Tooltip("最终时间文本")]
    [SerializeField] private TMP_Text finalTimeText; // 推荐使用 TMP_Text
    [Tooltip("评级文本")]
    [SerializeField] private TMP_Text ratingText;    // 用于显示 S/A/B/C 评级（可选）

    public bool IsShowing => panelLevelClear != null && panelLevelClear.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (panelLevelClear == null)
            panelLevelClear = GameObject.Find("Panel_LevelClear");
        if (finalTimeText == null)
            finalTimeText = GameObject.Find("Text_FinalTime")?.GetComponent<TextMeshProUGUI>();

        if (panelLevelClear != null)
            panelLevelClear.SetActive(false);
    }

    public void Show(string finalRank, float finalTime)
    {
        if (panelLevelClear == null) return;

        panelLevelClear.SetActive(true);

        if (ratingText != null)
        {
            ratingText.text = finalRank;
        }

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(finalTime / 60f);
            int seconds = Mathf.FloorToInt(finalTime % 60f);
            int fraction = Mathf.FloorToInt((finalTime * 100f) % 100f);

            // 格式化字符串，例如：01:23.45
            finalTimeText.text = "Time: " + string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
        }

        Time.timeScale = 0f;
        Debug.Log("🏆 关卡胜利！结算弹窗显示，游戏暂停。");
    }

    public void Hide()
    {
        if (panelLevelClear == null) return;

        panelLevelClear.SetActive(false);
        Time.timeScale = 1f;
    }
}
