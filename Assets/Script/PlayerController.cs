using UnityEngine;
using Spine.Unity;

public class SimpleMove : MonoBehaviour
{
    public float speed = 5f;

    // 把子物体 flen 拖到这里
    public SkeletonAnimation skeletonAnimation;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 锁定 Z 轴，防止父物体摔倒
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 1. 处理移动 (父物体移动)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }

        // 2. 处理转向 (只翻转子物体的 Spine 骨骼)
        if (moveInput != 0 && skeletonAnimation != null)
        {
            // 重点：这里只改骨骼 ScaleX，绝对不准动 transform.localScale
            // 如果方向反了，把 1 和 -1 互换
            skeletonAnimation.skeleton.ScaleX = (moveInput > 0) ? 1f : -1f;
        }
    }
}
