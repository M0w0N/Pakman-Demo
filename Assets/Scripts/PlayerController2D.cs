using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb;

    public float gravityStrength = 6f; // 引力强度，可以在 Inspector 中调整
    public float maxSpeed = 6f; // 主角的最大速度，防止被引力拉得过快

    void Start()
    {
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
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Vector2 currentPosition = rb.position;
            Vector2 gravitationalVector = mousePosition - currentPosition;
            // 规范：引力向量的长度（即引力强度）由 gravityStrength 控制，而不是直接用距离
            Vector2 pullDirection = gravitationalVector.normalized;
            Vector2 finalForce = pullDirection * gravityStrength;

            rb.AddForce(finalForce, ForceMode2D.Force);
        }
        if(rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
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

            // 播放吃豆音效（如果以后有的话）
            // AudioSource.PlayClipAtPoint(...);

            // 完美的物理销毁：把被吃掉的豆子从世界里抹去
            Destroy(other.gameObject);
        }
    }
}