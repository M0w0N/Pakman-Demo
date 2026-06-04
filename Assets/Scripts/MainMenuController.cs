using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI 面板绑定")]
    public GameObject panelMainMenu;     // 拖入 Panel_MainMenu
    public GameObject panelLevelSelect;   // 拖入 Panel_LevelSelect

    void Start()
    {
        // 游戏启动时，铁律：确保主菜单显示，选关界面隐藏
        if (panelMainMenu != null) panelMainMenu.SetActive(true);
        if (panelLevelSelect != null) panelLevelSelect.SetActive(false);
    }

    // 【接口】点击主菜单的“选关”按钮时调用
    public void OpenLevelSelect()
    {
        panelMainMenu.SetActive(false);   // 隐藏主菜单
        panelLevelSelect.SetActive(true);  // 显示选关页
        Debug.Log("📂 切换到选关界面");
    }

    // 【接口】点击选关界面的“返回”按钮时调用
    public void BackToMainMenu()
    {
        panelMainMenu.SetActive(true);    // 显示主菜单
        panelLevelSelect.SetActive(false); // 隐藏选关页
        Debug.Log("↩️ 返回主菜单");
    }

    // 【接口】点击具体的关卡按钮时调用（以关卡1为例）
    public void LoadLevel(string levelName)
    {
        Debug.Log($"🚀 玩家选择了关卡：{levelName}，正在加载场景...");

        // 严格遵循之前的白板规范，跳转到对应的正式游戏场景
        SceneManager.LoadScene(levelName);
    }
}