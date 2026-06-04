using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 核心物理规范：关闭重力，由鼠标引力场接管
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 gravitySource = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            Vector2 currentPosition = rb.position;
            Vector2 gravitationalVector = gravitySource - currentPosition;

            // 【物理平滑改动】
            // 不要直接设置速度！而是施加一个持续的引力。
            // ForceMode2D.Force 会把这个向量当成一个持续的拉力，完美与反弹力进行物理融合
            rb.AddForce(gravitationalVector, ForceMode2D.Force);
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