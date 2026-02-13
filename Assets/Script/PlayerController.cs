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
    public GameObject swordBeamPrefab;  // 必须把 SwordBeam 预制体拖进来

    public int comboCount = 0;
    public float comboWindow = 1.0f;
    public float lastAttackEndTime;

    private string currentAnim = "";
    private Rigidbody2D rb;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        if (skeletonAnimation == null)
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 连击输入检测 (地面且未攻击)
        if (Input.GetButtonDown("Fire1") && !isAttacking && isGrounded)
        {
            CheckCombo();
        }

        // 状态处理
        if (!isAttacking)
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
            }
            HandleSpineAnimation(moveInput);

            if (Time.time > lastAttackEndTime + comboWindow)
            {
                comboCount = 0;
            }
        }
        else if (isAttacking && rb != null && isGrounded)
        {
            rb.linearVelocity = Vector2.zero; // 攻击时定身
        }
    }

    void CheckCombo()
    {
        if (Time.time > lastAttackEndTime + comboWindow)
        {
            comboCount = 0;
        }

        comboCount++;

        if (comboCount > 3)
        {
            comboCount = 1;
        }

        StartCoroutine(PerformComboAttack(comboCount));
    }

    IEnumerator PerformComboAttack(int currentHit)
    {
        isAttacking = true;

        float damageDelay = 0.2f;
        Color charColor = Color.white;
        Vector3 beamScale = Vector3.one;
        Color beamColor = Color.white;

        switch (currentHit)
        {
            case 1:
                damageDelay = 0.2f;
                charColor = Color.yellow;
                beamScale = new Vector3(1f, 1f, 1f);
                beamColor = Color.cyan;
                break;
            case 2:
                damageDelay = 0.25f;
                charColor = new Color(1f, 0.5f, 0f);
                beamScale = new Vector3(1.5f, 1.2f, 1f);
                beamColor = Color.yellow;
                break;
            case 3:
                damageDelay = 0.5f;
                charColor = Color.red;
                beamScale = new Vector3(2.5f, 2f, 1f);
                beamColor = Color.red;
                break;
        }

        // Spine 变色 (使用安全写法)
        if (skeletonAnimation != null)
        {
            skeletonAnimation.skeleton.R = charColor.r;
            skeletonAnimation.skeleton.G = charColor.g;
            skeletonAnimation.skeleton.B = charColor.b;
            skeletonAnimation.skeleton.A = charColor.a;
        }

        // 确定朝向
        float direction = (skeletonAnimation.skeleton.ScaleX > 0) ? 1f : -1f;

        if (attackPoint != null)
        {
            // 调整发波点位置 (根据朝向)
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * direction;
            attackPoint.localPosition = localPos;

            if (swordBeamPrefab != null)
            {
                // 生成剑气
                GameObject beam = Instantiate(swordBeamPrefab, attackPoint.position, Quaternion.identity);

                // 设置剑气的大小和方向
                Vector3 finalScale = beamScale;
                finalScale.x *= direction;
                beam.transform.localScale = finalScale;

                // 设置剑气颜色
                SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = beamColor;

                // 注意：Destroy(beam) 在 SwordBeam 脚本里已经写了，这里不需要重复写
                // 但为了保险起见，damageDelay 之后可以不销毁，让 SwordBeam 自己管理寿命
                // 或者你希望按照连击节奏强制销毁也行：
                Destroy(beam, damageDelay + 0.1f);
            }
        }

        if (isGrounded) SetAnimation(idleAnim, true);

        yield return new WaitForSeconds(damageDelay);

        // 恢复 Spine 颜色
        if (skeletonAnimation != null)
        {
            skeletonAnimation.skeleton.R = 1;
            skeletonAnimation.skeleton.G = 1;
            skeletonAnimation.skeleton.B = 1;
            skeletonAnimation.skeleton.A = 1;
        }

        isAttacking = false;
        lastAttackEndTime = Time.time;
    }

    void HandleSpineAnimation(float input)
    {
        if (skeletonAnimation == null) return;

        if (input != 0)
            skeletonAnimation.skeleton.ScaleX = (input > 0) ? 1f : -1f;

        if (isGrounded)
        {
            if (input != 0) SetAnimation(runAnim, true);
            else SetAnimation(idleAnim, true);
        }
        else SetAnimation(jumpAnim, false);
    }

    void SetAnimation(string name, bool loop)
    {
        if (currentAnim == name) return;
        skeletonAnimation.state.SetAnimation(0, name, loop);
        currentAnim = name;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}
