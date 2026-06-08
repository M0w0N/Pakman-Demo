using TMPro; // 1. 必须引入 TextMeshPro 的命名空间 (如果使用的是老版Text，用 using UnityEngine.UI;)
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI 绑定")]
    [SerializeField] private TextMeshProUGUI scoreText; // 2. 拖入你的预制文本组件

    public int currentScore = 0;

    [Header("全局通关弹窗UI (从Persistent场景拖入)")]
    public GameObject panelLevelClear;
    public TMPro.TextMeshProUGUI finalScoreText; // 弹窗上的最终得分文本

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 3. 游戏开始时，初始化 UI 显示
        UpdateScoreUI();
    }

    // 加分接口
    public void AddScore(int points)
    {
        currentScore += points;

        // 4. 得分改变时，立即刷新屏幕显示
        UpdateScoreUI();
    }

    // 5. 专门负责更新 UI 的私有方法
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore;
        }
    }

    public void ShowLevelClearWindow()
    {
        if (panelLevelClear != null)
        {
            // 1. 啪！让隐藏的全局通关弹窗显示出来
            panelLevelClear.SetActive(true);

            // 2. 将本次关卡的最终得分同步到弹窗上
            if (finalScoreText != null)
            {
                finalScoreText.text = "FINAL SCORE: " + currentScore.ToString("D4");
            }

            // 3. 进阶物理控场：关卡通过了，让场景里的动态物体全停下
            Time.timeScale = 0f; // 暂停游戏物理世界，防止鬼怪继续撞你，玩家可以安心点按钮
            Debug.Log("🏆 关卡胜利！全局通关弹窗已唤醒，游戏物理暂停。");
        }
    }
    public void HideLevelClearWindow()
    {
        // 这里的 winWindow 就是你挂载的结算弹窗物体
        if (panelLevelClear != null)
        {
            panelLevelClear.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}