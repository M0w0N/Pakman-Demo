using UnityEngine;
using Unity.Cinemachine;

public class CinemachineBindPlayer : MonoBehaviour
{
    // 🔥 【核心改动】：新版中“虚拟相机”的组件名变成了 CinemachineCamera 
    private CinemachineCamera vcam;

    void Start()
    {
        // 获取挂在当前物体上的新版虚拟相机组件
        vcam = GetComponent<CinemachineCamera>();
    }

    void Update()
    {
        // 1. 如果相机组件正常，且当前没有追踪目标（或者上一关的主角已经被销毁了断开了引用）
        if (vcam != null && vcam.Follow == null)
        {
            // 2. 利用雷达标签跨场景扫描新出生、带 "Player" 标签的主角
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                // 3. 【核心绑定】：空降将主角的 Transform 赋值给相机的 Follow
                vcam.Follow = player.transform;

                Debug.Log("🤖 [Cinemachine v3] 成功跨场景锁定新关卡主角！");
            }
        }
    }
}