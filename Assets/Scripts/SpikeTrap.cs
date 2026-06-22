using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("惩罚设置")]
    [Tooltip("碰到尖刺后扣多少能量")]
    public int energyPenalty = 10;

    [Tooltip("碰到尖刺后扣多少分")]
    public int scorePenalty = 0;

    [Tooltip("碰到尖刺后是否传送回起点")]
    public bool resetToStart = false;

    [Tooltip("碰到尖刺后的无敌时间（秒），防止连续扣血")]
    public float invincibilityDuration = 1f;

    private float lastHitTime = -999f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 无敌保护
        if (Time.time - lastHitTime < invincibilityDuration) return;
        lastHitTime = Time.time;

        ApplyPenalty(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time - lastHitTime < invincibilityDuration) return;
        lastHitTime = Time.time;

        ApplyPenalty(collision.gameObject);
    }

    private void ApplyPenalty(GameObject player)
    {
        Debug.Log($"玩家踩到尖刺！惩罚：能量-{energyPenalty}, 分数-{scorePenalty}, 回起点={resetToStart}");

        if (energyPenalty > 0 && EnergyManager.Instance != null)
        {
            EnergyManager.Instance.ConsumeEnergy(energyPenalty);
        }

        if (scorePenalty > 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(-scorePenalty);
        }

        if (resetToStart)
        {
            ResetToStart resetComp = player.GetComponent<ResetToStart>();
            if (resetComp != null)
            {
                resetComp.SendMessage("ResetPlayer", SendMessageOptions.DontRequireReceiver);
            }
        }

        // TODO: 在此添加更多惩罚逻辑（比如闪屏、震屏、扣命等）
    }
}
