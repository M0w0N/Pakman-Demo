using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private string currentLevelName = "";
    
    //关卡重置标记，防止重复触发
    private bool isResetting = false;

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

    private void Update()
    {
        // 监听玩家按下重置键（例如键盘上的 R 键）
        if (Input.GetKeyDown(KeyCode.R) && !isResetting && !string.IsNullOrEmpty(currentLevelName))
        {
            ReplayCurrentLevel();
        }
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
        if (isResetting) return;

        // 开启协程执行“异步卸载并重载”
        StartCoroutine(ResetLevelCoroutine());
    }

    // ⏳ 核心重置协程
    private IEnumerator ResetLevelCoroutine()
    {
        isResetting = true;
        Debug.Log($"🔄 开始重置关卡: {currentLevelName}");

        // 1. 恢复游戏物理时间和状态（防止因为之前通关/暂停导致的 Time.timeScale = 0）
        Time.timeScale = 1f;

        // 2. 隐藏通关结算/暂停面板（通过面板自身的单例隐藏，而不是关闭整个常驻脚本）
        if (LevelClearPanel.Instance != null)
        {
            LevelClearPanel.Instance.Hide();
        }

        // 3. 局内数据清理（如重置分数等全局单例）
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.currentScore = 0;
        }

        // 4. 【关键】异步卸载当前的关卡子场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(currentLevelName);
        while (!unloadOp.isDone)
        {
            yield return null; // 等待卸载彻底完成，这会清空物理世界里的所有动态垃圾、敌人和弹道
        }

        // 5. 【关键】重新以 Additive (叠加) 模式加载该子场景
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(currentLevelName, LoadSceneMode.Additive);
        while (!loadOp.isDone)
        {
            yield return null; // 等待加载彻底完成
        }

        // 6. 可选：如果你需要让新加载出来的场景物体作为活动场景（比如物理引力计算需要），可以激活它
        Scene newScene = SceneManager.GetSceneByName(currentLevelName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        // 7. 重置完成，释放标记
        isResetting = false;
        Debug.Log($"✨ 关卡 {currentLevelName} 重置彻底完成！");
    }
}
