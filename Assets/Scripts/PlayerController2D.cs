using System.Collections;
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

    [Header("大力丸强化配置")]
    public float powerUpDuration = 6f;   // 强化状态持续秒数
    public float powerUpScale = 1.5f;    // 强化期间体积放大倍数
    public bool IsPoweredUp { get; private set; }  // 敌人读取的状态标记

    [Header("物品吸取配置")]
    public float magnetRadius = 1f;      // 吸物范围半径
    public float magnetFlySpeed = 6f;     // 豆子飞向主角的速度（单位/秒）
    public float magnetScanInterval = 0.15f; // 扫描间隔（秒），避免每帧扫

    // 状态控制变量
    // private bool isDashing = false;      // 状态锁：是否正在冲刺
    private float dashCooldownTimer = 0f;// CD 计时器
    private float magnetScanTimer = 0f;   // 吸物扫描计时器
    private Vector3 originalScale;

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

        // 记录初始大小，用于强化结束后复原
        originalScale = transform.localScale;
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

        // 吸物扫描：按间隔执行，而非每帧
        magnetScanTimer += Time.deltaTime;
        if (magnetScanTimer >= magnetScanInterval)
        {
            magnetScanTimer = 0f;
            PerformMagnetPull();
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
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(pointsPerPellet);
            }
            if (EnergyManager.Instance != null)
            {
                EnergyManager.Instance.AddEnergy(rewardEnergyGain); // 每吃一个豆子增加能量
            }

            // AudioSource.PlayClipAtPoint(...);

            // 完美的物理销毁：把被吃掉的豆子从世界里抹去
            Destroy(other.gameObject);
        }

        // 吃大力丸：体积变大 + 敌人反向逃跑
        if (other.CompareTag("PowerPellet"))
        {
            // 如果已经有强化状态在运行了，先停掉旧的避免叠状态
            if (IsPoweredUp)
            {
                StopCoroutine(nameof(PowerUpTimer));
            }
            StartCoroutine(PowerUpTimer());

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

    /// <summary>
    /// 扫描吸物范围内的 Reward 豆子，通知它们开始飞向主角
    /// </summary>
    private void PerformMagnetPull()
    {
        float currentRadius = IsPoweredUp ? magnetRadius * 1.5f : magnetRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, currentRadius);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Reward")) continue;

            PelletMagnetReceiver receiver = hit.GetComponent<PelletMagnetReceiver>();
            if (receiver == null) continue;

            receiver.StartFlyToPlayer(transform, magnetFlySpeed);
        }
    }

    /// <summary>
    /// 大力丸强化倒计时：体积变大 → 持续 powerUpDuration 秒 → 体积复原
    /// </summary>
    private System.Collections.IEnumerator PowerUpTimer()
    {
        IsPoweredUp = true;
        transform.localScale = originalScale * powerUpScale;

        Debug.Log($"⚡ 大力丸激活！体积 x{powerUpScale}，持续 {powerUpDuration} 秒。敌人开始逃跑！");

        yield return new WaitForSeconds(powerUpDuration);

        IsPoweredUp = false;
        transform.localScale = originalScale;

        Debug.Log("⚡ 大力丸失效，攻守恢复。");
    }
}