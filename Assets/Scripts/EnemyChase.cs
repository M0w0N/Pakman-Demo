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
            // 如果手动拖入了，也尝试缓存 PlayerController2D 引用
            playerController = playerTransform.GetComponent<PlayerController2D>();
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

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
    /// 碰到主角时触发 —— 强化状态下敌人可被消灭
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController2D player = collision.gameObject.GetComponent<PlayerController2D>();
        if (player != null)
        {
            // 强化状态：主角碰到敌人 → 消灭敌人
            if (player.IsPoweredUp)
            {
                Debug.Log("敌人在强化状态下被消灭！");
                Destroy(gameObject);
                return;
            }

            // 正常状态：保留速度阈值判定接口
            float playerSpeed = collision.rigidbody != null
                ? collision.rigidbody.linearVelocity.magnitude
                : 0f;

            Debug.Log($"敌人撞到主角！主角当前速度: {playerSpeed:F2}  （强化状态 = false）");
        }
    }
}
