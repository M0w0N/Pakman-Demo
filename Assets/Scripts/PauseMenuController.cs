using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂在关卡场景的 Canvas 下 —— 按 ESC 呼出/关闭暂停菜单。
/// 不与胜利弹窗冲突，不在主菜单场景生效。
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("UI 绑定")]
    [Tooltip("暂停菜单的根 Panel，拖入")]
    public GameObject pausePanel;

    [Header("关卡场景名")]
    [Tooltip("主菜单场景名，用于判断是否在关卡内")]
    public string mainMenuSceneName = "Scene_MainMenu";

    private bool isPaused = false;

    void Start()
    {
        // 启动时确保暂停面板是隐藏的
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        // 只在按下 ESC 的那一帧处理
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // 如果不在关卡内（主菜单场景是加载状态），不响应暂停
        if (SceneManager.GetSceneByName(mainMenuSceneName).isLoaded) return;

        // 如果胜利弹窗正开着，不响应暂停 —— 避免暂停菜单盖在结算上面
        if (ScoreManager.Instance != null
            && ScoreManager.Instance.panelLevelClear != null
            && ScoreManager.Instance.panelLevelClear.activeSelf)
        {
            return;
        }

        // 切换暂停状态
        TogglePause();
    }

    /// <summary>
    /// 暂停按钮（或 ESC）调用 — 切换暂停/继续
    /// </summary>
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        Debug.Log("⏸ 游戏暂停");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pausePanel != null) pausePanel.SetActive(false);
        Debug.Log("▶ 游戏继续");
    }

    /// <summary>
    /// "继续游戏"按钮绑定这个方法
    /// </summary>
    public void OnResumeButton()
    {
        if (!isPaused) return;
        TogglePause();
    }

    /// <summary>
    /// "返回主菜单"按钮绑定这个方法
    /// </summary>
    public void OnReturnToMainMenu()
    {
        // 恢复时间，否则回到菜单也是暂停的
        Time.timeScale = 1f;
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.ReturnToMainMenuFromLevel();
        }
    }
}
