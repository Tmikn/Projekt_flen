using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float maxSpeed = 8f;      // 最大速度
    public float acceleration = 40f; // 加速度
    public float deceleration = 30f; // 减速摩擦力

    private float currentSpeed = 0f;

    [Header("移动参数")]
    //public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("状态检测")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // 获取动画组件
    }

    void Update()
    {
        // 1. 获取输入
        moveInput = Input.GetAxisRaw("Horizontal");

        // 2. 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // 3. 跳跃处理
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. 角色转向
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(moveInput > 0 ? -1 : 1, 1, 1);
        }


    }

    void FixedUpdate()
    {
        float targetSpeed = moveInput * maxSpeed;

        // 渐进插值计算当前速度
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        // 传给 Animator 的依然是绝对值，用于控制动画切换
        anim.SetFloat("Speed", Mathf.Abs(currentSpeed));
    }
}
