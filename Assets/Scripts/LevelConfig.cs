using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject 关卡配置表 —— 支持动态进度评级系统
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

    // 评级结构体，方便在 Inspector 中直观配置
    [System.Serializable]
    public struct RankData
    {
        public string rankName;       // 评级名称，如 "S", "A", "B", "C"
        [Range(0f, 100f)]
        public float minRatingPct;  // 达到该评级所需的最低进度百分比 (0-100)
    }

    [Header("基本信息")]
    public string levelName = "未命名关卡";

    [Header("胜利条件")]
    public WinCondition winCondition = WinCondition.EatAllPellets;

    [Header("EatAllPellets / ReturnToStart 共用")]
    [Tooltip("场景里 Reward 豆子的总数（数量进度条基准）")]
    public int totalPellets = 20;

    [Header("仅 ReturnToStart")]
    [Tooltip("吃够多少个豆子后起点解锁")]
    public int pelletsRequiredForReturn = 10;

    [Header("时间限制（秒）")]
    [Tooltip("0 表示不限时（原有硬性时间限制，若用动态进度条，可设为0，由进度条归零作为失败条件）")]
    public float timeLimitSeconds = 0f;

    [Header("=== 评分/评级进度条配置 ===")]

    [Tooltip("进度条的最大值（通常为100）")]
    public float maxRating = 100f;

    [Tooltip("开局时的初始进度值")]
    public float initialRating = 50f;

    [Tooltip("每秒钟进度条自动减少的数值")]
    public float ratingDrainPerSecond = 2f;

    [Tooltip("每吃一颗豆子回复的进度数值")]
    public float ratingGainPerPellet = 5f;

    [Header("评级阈值设置 (请按从高到低顺序填写)")]
    [Tooltip("例如：S->80, A->50, B->20")]
    public List<RankData> rankThresholds = new List<RankData>()
    {
        new RankData { rankName = "S", minRatingPct = 80f },
        new RankData { rankName = "A", minRatingPct = 50f },
        new RankData { rankName = "B", minRatingPct = 20f },
        new RankData { rankName = "C", minRatingPct = 0f }
    };
}