using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("引用绑定")]
    private Transform playerTransform;
    private Rigidbody2D rb;

    [Header("弹射设置")]
    public float launchInterval = 4f;    // 每隔几秒弹射一次
    public float launchForce = 22f;      // 弹射的初始爆发力度（桌球出杆速度，可以大一点）

    [Header("自然减速（桌球摩擦力）")]
    [Tooltip("值越大，弹射后减速越快。0代表永不减速，就像太空中一样")]
    public float FrictionDrag = 1.8f;

    // 内部计时器
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // TA 物理规范配置
        rb.gravityScale = 0f; // 彻底关闭重力，实现完全的平面桌球漂移
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 设置刚体的线性阻尼 —— 这就是 Unity 自带的“空气阻力/表面摩擦力”
        // 赋值后，Unity 的物理引擎会自动让刚体做“自然对数减速”，完全不需要我们在 Update 里写减速代码


        // 随机初始时间，错开多只怪的起跳时机
        timer = Random.Range(0f, launchInterval);
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 无论怪物当前是在飞速旋转、反弹还是已经减速静止，计时器都在跑
        timer += Time.deltaTime;

        if (timer >= launchInterval)
        {
            TriggerBilliardLaunch();
            timer = 0f;
        }
    }

    // 发起桌球式强力弹射
    private void TriggerBilliardLaunch()
    {
        if (playerTransform == null) return;

        // 1. 瞄准玩家：计算当前怪物指向玩家的精准方向向量
        Vector2 launchDirection = (playerTransform.position - transform.position).normalized;

        // 2. 出杆击球！施加一个瞬间的冲量速度 (Velocity Change)
        // 这一步会瞬间覆盖掉怪物上一轮残余的微弱滑行速度
        rb.linearVelocity = launchDirection * launchForce;

        Debug.Log("🎱 桌球怪蓄力一击！朝着主角撞去！");
    }

    // 碰撞检测
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 因为我们完全使用了 Unity 自带的物理材质和刚体阻尼
        // 当它撞击到 Tilemap 墙面时，物理引擎会自动根据反弹角进行标准的“镜面反射”反弹
        // 撞墙后，它会带着残余的力继续在场景里反弹滑行，直到阻尼让它完全停下
    }
}