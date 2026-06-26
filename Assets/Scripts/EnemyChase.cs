using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("目标绑定")]
    [Tooltip("留空则自动搜索场景中挂有 PlayerController2D 的物体")]
    public Transform playerTransform;

    [Header("移动参数")]
    [Tooltip("追击的恒定速度基准")]
    public float moveSpeed = 3f;
    [Tooltip("加速度 —— 值越大，敌人转向越灵敏，惯性感越弱")]
    public float acceleration = 8f;
    [Tooltip("最大速度上限，防止物理穿透或连锁反弹越冲越快")]
    public float maxSpeed = 5f;

    [Header("逃跑参数")]
    [Tooltip("敌人逃跑时的最大速度，可以比追击时快或慢")]
    public float fleeMaxSpeed = 7f;

    [Header("AI 触发条件")]
    [Tooltip("是否已经激活追逐状态")]
    public bool isActivated = false;
    [Tooltip("惊醒半径：主角进入这个距离（米）时，AI开始追逐")]
    public float detectionRadius = 6f;
    [Tooltip("是否允许脱战：勾选后，若主角跑得太远，AI会失去目标停下")]
    public bool canLoseTarget = false;
    [Tooltip("脱战半径：当主角离开这个距离后，AI放弃追逐（必须大于惊醒半径）")]
    public float loseTargetRadius = 10f;

    private Rigidbody2D rb;
    private PlayerController2D playerController;  // 缓存主角引用，用于读强化状态

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 核心物理规范：关闭重力，和主角保持一致的物理环境
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 自动锁定主角
        if (playerTransform == null)
        {
            PlayerController2D player = FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player;
            }
            else
            {
                Debug.LogWarning("EnemyChase: 未找到 PlayerController2D，请在 Inspector 中手动拖入目标。");
            }
        }
        else
        {
            playerController = playerTransform.GetComponent<PlayerController2D>();
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // --- AI 触发与状态条件检查 ---
        // 计算当前与玩家的实际物理距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (!isActivated)
        {
            // 如果还没激活，检查玩家是否进入了惊醒半径
            if (distanceToPlayer <= detectionRadius)
            {
                isActivated = true;
                Debug.Log($"{gameObject.name} 发现了玩家，开始追击！");
            }
            else
            {
                // 未激活状态下，物理速度应该逐渐衰减趋近于0，防止因被其他物体撞击后无限滑行
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);
                return; // 核心拦截：未激活则不进行后面的追逐物理计算
            }
        }
        else
        {
            // 如果已经激活，且开启了“允许脱战”，检查玩家是否跑得太远
            if (canLoseTarget && distanceToPlayer > loseTargetRadius)
            {
                isActivated = false;
                Debug.Log($"{gameObject.name} 失去了玩家的踪迹。");
                return;
            }
        }

        // --- 以下为原有的追逐/逃跑物理逻辑 ---

        // 核心反转：主角吃了大力丸 → 敌人逃跑而非追击
        bool isPlayerPoweredUp = (playerController != null && playerController.IsPoweredUp);

        // 计算方向：追击时指向主角，逃跑时远离主角
        Vector2 rawDirection = playerTransform.position - transform.position;
        Vector2 direction = isPlayerPoweredUp ? -rawDirection.normalized : rawDirection.normalized;

        // 逃跑期间允许更快的上限，增加被追上的风险
        float currentMaxSpeed = isPlayerPoweredUp ? fleeMaxSpeed : maxSpeed;

        rb.AddForce(direction * acceleration, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }

    /// <summary>
    /// 碰在场景面板绘制辅助实心圈/线，方便在编辑器里直观调距离
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制惊醒半径（红色圈）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 如果开启了脱战，绘制脱战半径（黄色圈）
        if (canLoseTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, loseTargetRadius);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
        if (player != null)
        {
            if (player.IsPoweredUp)
            {
                Debug.Log("敌人在强化状态下被消灭！");
                Destroy(gameObject);
                return;
            }

            float playerSpeed = collision.rigidbody != null
                ? collision.rigidbody.linearVelocity.magnitude
                : 0f;

            Debug.Log($"敌人撞到主角！主角当前速度: {playerSpeed:F2}  （强化状态 = false）");
        }
    }
}