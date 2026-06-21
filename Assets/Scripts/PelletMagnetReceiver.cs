using UnityEngine;

/// <summary>
/// 挂在 Reward 豆子 Prefab 上。被主角吸物范围扫中后，
/// 用 Transform 匀速飞向主角，撞上即被吃。
/// </summary>
public class PelletMagnetReceiver : MonoBehaviour
{
    private bool isFlying = false;
    private Transform playerTransform;
    private float flySpeed;

    public void StartFlyToPlayer(Transform player, float speed)
    {
        if (isFlying) return;

        playerTransform = player;
        flySpeed = speed;
        isFlying = true;

        StartCoroutine(nameof(FlyRoutine));
    }

    private System.Collections.IEnumerator FlyRoutine()
    {
        if (playerTransform == null) yield break;

        while (gameObject != null)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                flySpeed * Time.deltaTime);

            yield return null;
        }
    }
}
