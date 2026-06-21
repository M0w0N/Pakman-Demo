using UnityEngine;

/// <summary>
/// ScriptableObject 关卡配置表 —— 在 Project 窗口右键 → Create → Level Config 创建。
/// 拖到场景里 LevelWinChecker 的 config 槽上即可。
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig_New", menuName = "Level Config", order = 0)]
public class LevelConfig : ScriptableObject
{
    public enum WinCondition
    {
        EatAllPellets,       // 吃光所有豆子
        ReturnToStart,       // 吃够一定量 → 返回起点
        ReachExitTrigger     // 触碰出口区域
    }

    [Header("基本信息")]
    public string levelName = "未命名关卡";

    [Header("胜利条件")]
    public WinCondition winCondition = WinCondition.EatAllPellets;

    [Header("EatAllPellets / ReturnToStart 共用")]
    [Tooltip("场景里 Reward 豆子的总数（进度条基准）")]
    public int totalPellets = 20;

    [Header("仅 ReturnToStart")]
    [Tooltip("吃够多少个豆子后起点解锁")]
    public int pelletsRequiredForReturn = 10;

    [Header("时间限制（秒）")]
    [Tooltip("0 表示不限时")]
    public float timeLimitSeconds = 0f;
}
