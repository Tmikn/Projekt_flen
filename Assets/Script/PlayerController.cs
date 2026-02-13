using UnityEngine;
using Spine.Unity;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float speed = 5f;
    public float jumpForce = 12f;

    [Header("地面检测")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Spine 组件")]
    public SkeletonAnimation skeletonAnimation;

    [Header("动画名")]
    public string idleAnim = "idle";
    public string runAnim = "run";
    public string jumpAnim = "jump";

    [Header("三连击设置")]
    public Transform attackPoint;
    public GameObject swordBeamPrefab;

    public int comboCount = 0;          // 当前连击数 (0, 1, 2, 3)
    public float comboWindow = 1.0f;    // 连击有效窗口期 (上次攻击结束后多久内按键算连击)
    public float lastAttackEndTime;     // 上次攻击结束的时间点

    private Rigidbody2D rb;
    private bool isAttacking = false;   // 是否正在动作硬直中
    private string currentAnim = ""; // 记录当前播放的动画名

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (skeletonAnimation == null) skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        float moveInput = Input.GetAxisRaw("Horizontal");

        // --- 1. 连击输入检测 ---
        // 条件：按下攻击键 + 当前没在攻击动作中
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            CheckCombo();
        }

        // --- 2. 状态处理 ---
        if (!isAttacking)
        {
            // 正常移动和跳跃
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
            }
            HandleSpineAnimation(moveInput);

            // 超时重置连击：如果超过了窗口期还没攻击，连击数归零
            if (Time.time > lastAttackEndTime + comboWindow)
            {
                comboCount = 0;
            }
        }
        else if (isAttacking && rb != null && isGrounded)
        {
            // 攻击时定身
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void CheckCombo()
    {
        // 如果距离上次攻击结束已经过了太久，重置为第一击
        if (Time.time > lastAttackEndTime + comboWindow)
        {
            comboCount = 0;
        }

        // 连击数 +1
        comboCount++;

        // 如果超过3击，重置为1 (或者你可以设置打完一套强制冷却)
        if (comboCount > 3)
        {
            comboCount = 1;
        }

        StartCoroutine(PerformComboAttack(comboCount));
    }

    IEnumerator PerformComboAttack(int currentHit)
    {
        isAttacking = true;

        // --- 根据连击数设置不同的参数 ---
        float damageDelay = 0.2f; // 动作持续时间
        Color charColor = Color.white; // 角色变色
        Vector3 beamScale = Vector3.one; // 剑气大小
        Color beamColor = Color.white;   // 剑气颜色

        switch (currentHit)
        {
            case 1: // 第一击：轻快
                damageDelay = 0.2f;
                charColor = Color.yellow;      // 变黄
                beamScale = new Vector3(1f, 1f, 1f);
                beamColor = Color.cyan;        // 蓝光
                Debug.Log("连击 1！");
                break;

            case 2: // 第二击：稍重
                damageDelay = 0.25f;
                charColor = new Color(1f, 0.5f, 0f); // 橙色
                beamScale = new Vector3(1.5f, 1.2f, 1f); // 变大
                beamColor = Color.yellow;      // 黄光
                Debug.Log("连击 2！！");
                break;

            case 3: // 第三击：终结技
                damageDelay = 0.5f;            // 硬直更长
                charColor = Color.red;         // 红色
                beamScale = new Vector3(2.5f, 2f, 1f);   // 巨大
                beamColor = Color.red;         // 红光
                Debug.Log("连击 3！！！(终结)");
                break;
        }

        // 1. 角色变色反馈
        if (skeletonAnimation != null)
            skeletonAnimation.skeleton.SetColor(charColor);

        // 2. 生成剑气 (带方向修正)
        float direction = (skeletonAnimation.skeleton.ScaleX > 0) ? 1f : -1f;
        if (attackPoint != null)
        {
            // 修正攻击点位置
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * direction;
            attackPoint.localPosition = localPos;

            if (swordBeamPrefab != null)
            {
                GameObject beam = Instantiate(swordBeamPrefab, attackPoint.position, Quaternion.identity);

                // 设置剑气大小
                Vector3 finalScale = beamScale;
                finalScale.x *= direction; // 修正方向
                beam.transform.localScale = finalScale;

                // 设置剑气颜色 (如果Prefab上有SpriteRenderer)
                SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = beamColor;

                Destroy(beam, damageDelay); // 特效持续时间等于动作时间
            }
        }

        // 3. 播放Idle防止滑步
        if (isGrounded) SetAnimation(idleAnim, true);

        // 等待动作硬直
        yield return new WaitForSeconds(damageDelay);

        // 4. 恢复
        if (skeletonAnimation != null)
            skeletonAnimation.skeleton.SetColor(Color.white);

        isAttacking = false;
        lastAttackEndTime = Time.time; // 记录这次攻击结束的时间，用于判断连击窗口
    }

    // --- 以下是原有的辅助函数 ---
    void HandleSpineAnimation(float input)
    {
        if (skeletonAnimation == null) return;
        if (input != 0) skeletonAnimation.skeleton.ScaleX = (input > 0) ? 1f : -1f;

        if (isGrounded)
        {
            if (input != 0) SetAnimation(runAnim, true);
            else SetAnimation(idleAnim, true);
        }
        else SetAnimation(jumpAnim, false);
    }

    void SetAnimation(string name, bool loop)
    {
        // 简单状态机：如果正在移动输入，不要被Idle打断 (除了攻击强制调用)
        if (currentAnim == name) return;
        skeletonAnimation.state.SetAnimation(0, name, loop);
        currentAnim = name;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null) { Gizmos.color = Color.red; Gizmos.DrawWireSphere(groundCheck.position, checkRadius); }
    }
}

