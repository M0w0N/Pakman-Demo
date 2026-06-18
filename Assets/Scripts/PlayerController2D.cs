using TMPro;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera mainCam;

    public float gravityStrength = 6f; // 引力强度，可以在 Inspector 中调整
    public float maxSpeed = 6f; // 主角的最大速度，防止被引力拉得过快

    [Header("冲刺技能配置")]
    public float dashForce = 15f;        // 冲刺时的爆发速度
    public float dashDuration = 0.2f;    // 冲刺持续时间（瞬间冲完）
    public float dashCooldown = 1f;      // 冲刺冷却时间（CD）
    public int dashEnergyCost = 20;   // 每次冲刺消耗的能量
    public int rewardEnergyGain = 5;


    // 状态控制变量
    // private bool isDashing = false;      // 状态锁：是否正在冲刺
    private float dashCooldownTimer = 0f;// CD 计时器

    void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();

        // 核心物理规范：关闭重力，由鼠标引力场接管
        rb.gravityScale = 0f;

        // 增加了持续面向鼠标方向的脚本，此行不清楚是否应该继续保留
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // 此选项在菜单中已存在，用于辅助脚本目标自动勾选
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Vector2 currentPosition = rb.position;
            Vector2 gravitationalVector = mousePosition - currentPosition;
            Vector2 pullDirection = gravitationalVector.normalized;
            Vector2 finalForce = pullDirection * gravityStrength;

            rb.AddForce(finalForce, ForceMode2D.Force);
        }
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void Update()
    {
        // 技能冷却检测
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(1)) // 点击右键
        {
            // 检测是否在冷却时间内
            if (dashCooldownTimer > 0)
            {
                Debug.Log("技能正在冷却中，还剩 " + dashCooldownTimer.ToString("F1") + " 秒！");
                return; // 直接拦截，不执行后续冲刺逻辑
            }

            // 如果能走到这里，说明 dashCooldownTimer <= 0，即不在冷却时间内
            PerformDash();
        }
    }

    [Header("得分设置")]
    public int pointsPerPellet = 10; // 每个豆子值多少分（在主角这里调）

    // 主角作为一个带 Rigidbody 的动态实体，去主动触发勾选了 Is Trigger 的豆子
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 规范：给你的豆子 Prefab 挂上一个叫 "Reward" 的 Tag
        if (other.CompareTag("Reward"))
        {
            // 核心联动：一句话远程呼叫账本加分，不需要知道账本是怎么实现的
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointsPerPellet);

            }
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.AddEnergy(rewardEnergyGain); // 每吃一个豆子增加 5 点能量
            }

            // AudioSource.PlayClipAtPoint(...);

            // 完美的物理销毁：把被吃掉的豆子从世界里抹去
            Destroy(other.gameObject);
        }
    }

    private void PerformDash()
    {
        dashCooldownTimer = dashCooldown; // 刷新 CD
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // 扣除能量
        if (EnergyManager.Instance != null)
        {
            EnergyManager.Instance.ConsumeEnergy(dashEnergyCost);
        }

        // 计算冲刺方向：从角色当前位置指向鼠标当前位置
        Vector2 dashDirection = ((Vector2)mousePosition - (Vector2)transform.position).normalized;

        // 如果玩家鼠标刚好悬停在角色身上（距离太近），则朝角色正前方或默认方向冲刺
        if (dashDirection == Vector2.zero)
        {
            dashDirection = transform.up;
        }

        // 赋予刚体冲刺速度 (物理执行)
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);

        // 冲刺结束，恢复状态
    }
}