using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private string currentLevelName = "";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 确保切换场景时，这个核心管理器自己不会被销毁
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 游戏启动时，叠加加载主菜单场景
        LoadMainMenu();
    }

    // 加载主菜单
    public void LoadMainMenu()
    {
        // 如果之前有加载关卡，先卸载掉
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            SceneManager.UnloadSceneAsync(currentLevelName);
            currentLevelName = "";
        }

        // 使用 Additive 模式叠加主菜单
        SceneManager.LoadScene("Scene_MainMenu", LoadSceneMode.Additive);
    }

    // 切换到具体关卡（由主菜单按钮，或者通关触碰点触发）
    public void LoadLevel(string levelName)
    {
        // 1. 如果当前在主菜单，先卸载主菜单
        if (SceneManager.GetSceneByName("Scene_MainMenu").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Scene_MainMenu");
        }

        // 2. 如果当前已经在一个关卡里，先卸载旧关卡
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            SceneManager.UnloadSceneAsync(currentLevelName);
        }

        // 3. 异步叠加加载新关卡
        currentLevelName = levelName;
        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);

        // 4. 加载完成后重置角色位置、刷新 UI 分数等初始化逻辑
        // StartCoroutine(InitLevelAfterLoad());
    }

    // 新增：供结算大弹窗（或其他暂停菜单）调用的“回到主菜单”接口
    // 供结算大弹窗调用的“回到主菜单”接口
    public void ReturnToMainMenuFromLevel()
    {
        Debug.Log("🏠 收到请求：正在从关卡返回主菜单...");

        MainMenuController.shouldOpenLevelSelectDirectly = true;

        // 隐藏通关弹窗
        if (LevelClearPanel.Instance != null)
        {
            LevelClearPanel.Instance.Hide();
        }

        // 1. 安全卸载当前处于运行中的游戏关卡
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            SceneManager.UnloadSceneAsync(currentLevelName);
            currentLevelName = "";
        }

        // 2. 检查并叠加加载主菜单
        if (!SceneManager.GetSceneByName("Scene_MainMenu").isLoaded)
        {
            SceneManager.LoadScene("Scene_MainMenu", LoadSceneMode.Additive);
        }
    }

    public void ReplayCurrentLevel()
    {
        // 恢复游戏物理时间   
        Time.timeScale = 1f;

        // 清理分数
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.currentScore = 0;
        }

        // 2. 隐藏掉通关弹窗自己
        gameObject.SetActive(false);

        // 3. 重新加载这个场景，实现物理世界的彻底重置
        SceneManager.LoadScene(currentLevelName);
    }
}
