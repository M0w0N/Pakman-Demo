using UnityEngine;
using UnityEngine.UI;

public class SceneTransitionButton : MonoBehaviour
{
    [Header("通用跳转设置")]
    [Tooltip("需要绑定跳转事件的 UI 按钮组件")]
    [SerializeField] private Button targetButton;

    [Tooltip("点击该按钮后，想要跳转的目标场景名称（需与 Build Settings 保持一致）")]
    [SerializeField] private string targetSceneName;

    void Start()
    {
        if (targetButton != null)
        {
            // 动态添加点击事件监听
            targetButton.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 上的 LevelTransitionButton 脚本未绑定 targetButton！");
        }
    }

    // 核心通用触发逻辑
    private void OnButtonClick()
    {
        // 健壮性检查：防止策划/美术不小心填了空字符串或者空格
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError($"[{targetButton.name}] 的目标场景名称为空，无法跳转！");
            return;
        }

        // 调用全局场景管理器
        if (GameSceneManager.Instance != null)
        {
            Debug.Log($"🎬 按钮 [{targetButton.name}] 被点击，正在通知管理器跳转至场景: {targetSceneName}");
            GameSceneManager.Instance.LoadLevel(targetSceneName);
        }
        else
        {
            Debug.LogError("未找到 GameSceneManager 实例！请确保从初始化常驻场景启动游戏。");
        }
    }

    void OnDestroy()
    {
        // 良好的代码习惯：销毁时移除监听
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClick);
        }
    }
}