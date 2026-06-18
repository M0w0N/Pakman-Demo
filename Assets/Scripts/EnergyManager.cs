using UnityEngine;
using UnityEngine.UI; // 必须引入 UI 命名空间

public class EnergyManager : MonoBehaviour
{
    // 静态单例，方便其他脚本一键调用
    public static EnergyManager Instance;

    [Header("UI 组件引用")]
    public Slider energySlider;

    [Header("能量参数")]
    public int maxEnergy = 100;    // 最大能量值
    private int currentEnergy = 0; // 当前能量值

    void Awake()
    {
        // 确保单例唯一性
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 初始化 UI 状态
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }
    }

    /// <summary>
    /// 公开方法：供豆子或主角调用，增加能量
    /// </summary>
    /// <param name="amount">增加的能量点数</param>
    public void AddEnergy(int amount)
    {
        // 增加能量，并用 Mathf.Clamp 锁死在 0 到最大值之间，防止爆表
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);

        // 刷新 UI 显示
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }

        Debug.Log($"⚡ 能量增加！当前能量: {currentEnergy}/{maxEnergy}");

        // 【扩展接口】如果能量满了，可以在这里触发特殊机制
        if (currentEnergy >= maxEnergy)
        {
            OnEnergyFull();
        }
    }

    public void ConsumeEnergy(int amount)
    {
        // 增加能量，并用 Mathf.Clamp 锁死在 0 到最大值之间，防止爆表
        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);

        // 刷新 UI 显示
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }

        Debug.Log($"⚡ 能量消耗！当前能量: {currentEnergy}/{maxEnergy}");

        // 【扩展接口】如果能量满了，可以在这里触发特殊机制
        if (currentEnergy >= maxEnergy)
        {
            OnEnergyFull();
        }
    }

    /// <summary>
    /// 能量满格后的特殊逻辑
    /// </summary>
    private void OnEnergyFull()
    {
        Debug.Log("🔥 能量已满！大招准备就绪！");
        // 你可以在这里让主角变身、移速加快、或者播放一个炫酷的满能量特效
    }

    /// <summary>
    /// 跨关卡时重置能量槽
    /// </summary>
    public void ResetEnergy()
    {
        currentEnergy = 0;
        if (energySlider != null)
        {
            energySlider.value = 0;
        }
    }
}