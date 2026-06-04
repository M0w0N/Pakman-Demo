using UnityEngine;

public class FaceToTarget : MonoBehaviour
{
    public enum TargetType { Mouse, Transform }

    [Header("目标设置")]
    public TargetType targetType = TargetType.Mouse; // 默认朝向鼠标，也可以改成朝向特定物体
    public Transform targetTransform;                // 如果选了 Transform 模式，把目标拖到这里

    [Header("旋转手感")]
    public bool smoothRotation = true;               // 是否开启平滑旋转（有转身动画感）
    public float rotationSpeed = 10f;                // 转身速度（值越大转得越快）

    [Header("美术贴图修正")]
    [Tooltip("如果你的原图素材默认朝向不是右边，用这个调整偏角。正数逆时针，负数顺时针。")]
    public float angleOffset = 0f;

    void Update()
    {
        Vector3 targetPos = Vector3.zero;

        // 1. 获取目标点的位置
        if (targetType == TargetType.Mouse)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPos = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0f);
        }
        else if (targetType == TargetType.Transform && targetTransform != null)
        {
            targetPos = targetTransform.position;
        }
        else
        {
            return; // 没有目标则不旋转
        }

        // 2. 【核心数学计算】计算从自己指向目标的向量
        Vector3 direction = targetPos - transform.position;

        // 3. 使用 Atan2 算出这个向量与 X 轴正方向的夹角（弧度制），并转化为角度制
        // $\theta = \arctan(\frac{y}{x}) \times \frac{180}{\pi}$
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 4. 加上美术素材的初始偏角修正
        targetAngle += angleOffset;

        // 5. 应用旋转
        if (smoothRotation)
        {
            // 平滑旋转：插值计算当前角度到目标角度（类似转向灯/脖子扭动）
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // 瞬间锁定：直接修改 Z 轴欧拉角
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
    }
}