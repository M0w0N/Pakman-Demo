using TMPro;
using UnityEngine;

/// <summary>
/// 挂在常驻场景，管理关卡结算弹窗的显示/隐藏。
/// 不依赖 LevelWinChecker，任何地方都可调用。
/// </summary>
public class LevelClearPanel : MonoBehaviour
{
    public static LevelClearPanel Instance { get; private set; }

    [Header("UI 绑定")]
    [Tooltip("结算弹窗根 Panel")]
    public GameObject panelLevelClear;
    [Tooltip("最终得分文本")]
    public TextMeshProUGUI finalScoreText;

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
        if (finalScoreText == null)
            finalScoreText = GameObject.Find("Text_FinalScore")?.GetComponent<TextMeshProUGUI>();

        if (panelLevelClear != null)
            panelLevelClear.SetActive(false);
    }

    public void Show()
    {
        if (panelLevelClear == null) return;

        panelLevelClear.SetActive(true);

        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            finalScoreText.text = "FINAL SCORE: " + ScoreManager.Instance.currentScore.ToString("D4");
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
