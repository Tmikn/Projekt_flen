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

    [Header("攻击动画名")]
    public string attackAnim1 = "slash1";
    public string attackAnim2 = "slash1";
    public string attackAnim3 = "slash1";

    [Header("三连击设置")]
    public Transform attackPoint;
    public GameObject swordBeamPrefab;

    // --- 新增：专门控制生成延迟 ---
    [Tooltip("6帧大约是 0.1秒(60fps) 到 0.2秒(30fps)")]
    public float beamSpawnDelay = 0.15f; // 默认给个中间值，你可以在面板微调

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

        if (Input.GetButtonDown("Fire1") && !isAttacking && isGrounded)
        {
            CheckCombo();
        }

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
            rb.linearVelocity = Vector2.zero;
        }
    }

    void CheckCombo()
    {
        if (Time.time > lastAttackEndTime + comboWindow)
        {
            comboCount = 0;
        }

        comboCount++;
        if (comboCount > 3) comboCount = 1;

        StartCoroutine(PerformComboAttack(comboCount));
    }

    IEnumerator PerformComboAttack(int currentHit)
    {
        isAttacking = true;

        Color charColor = Color.white;
        Vector3 beamScale = Vector3.one;
        Color beamColor = Color.white;
        string animToPlay = attackAnim1;

        // 不同的连击段数，配置不同的颜色和大小
        switch (currentHit)
        {
            case 1:
                animToPlay = attackAnim1;
                charColor = Color.yellow;
                beamScale = new Vector3(1f, 1f, 1f);
                beamColor = Color.cyan;
                break;
            case 2:
                animToPlay = attackAnim2;
                charColor = new Color(1f, 0.5f, 0f);
                beamScale = new Vector3(1.5f, 1.2f, 1f);
                beamColor = Color.yellow;
                break;
            case 3:
                animToPlay = attackAnim3;
                charColor = Color.red;
                beamScale = new Vector3(2.5f, 2f, 1f);
                beamColor = Color.red;
                break;
        }

        // 1. 立即播放动作
        SetAnimation(animToPlay, false);

        // 2. 【关键】先等待“6帧” (前摇)
        // 只有等这几帧过去了，才生成剑气
        yield return new WaitForSeconds(beamSpawnDelay);

        // 3. 时间到了，Spine变色 + 生成剑气
        if (skeletonAnimation != null)
        {
            skeletonAnimation.skeleton.R = charColor.r;
            skeletonAnimation.skeleton.G = charColor.g;
            skeletonAnimation.skeleton.B = charColor.b;
            skeletonAnimation.skeleton.A = charColor.a;
        }

        float beamLifeTime = 0.3f; // 默认存活时间，稍后会从预制体读取

        float direction = (skeletonAnimation.skeleton.ScaleX > 0) ? 1f : -1f;
        if (attackPoint != null && swordBeamPrefab != null)
        {
            Vector3 localPos = attackPoint.localPosition;
            localPos.x = Mathf.Abs(localPos.x) * direction;
            attackPoint.localPosition = localPos;

            // 生成剑气
            GameObject beam = Instantiate(swordBeamPrefab, attackPoint.position, Quaternion.identity);

            // 设置大小和方向
            Vector3 finalScale = beamScale;
            finalScale.x *= direction;
            beam.transform.localScale = finalScale;

            // 设置颜色
            SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = beamColor;

            // 获取剑气实际的存活时间，确保角色动作和剑气共存亡
            SwordBeam beamScript = beam.GetComponent<SwordBeam>();
            if (beamScript != null)
            {
                beamLifeTime = beamScript.lifeTime;
            }

            // 注意：Destroy由 SwordBeam 脚本自己管理，这里不需要写 Destroy
        }

        // 4. 【关键】等待剑气存在的时间 (后摇)
        // 只要剑气还没消失，代码就卡在这里，玩家就不能动
        yield return new WaitForSeconds(beamLifeTime);

        // 5. 剑气消失了，恢复状态
        if (skeletonAnimation != null)
        {
            skeletonAnimation.skeleton.R = 1;
            skeletonAnimation.skeleton.G = 1;
            skeletonAnimation.skeleton.B = 1;
            skeletonAnimation.skeleton.A = 1;
        }

        // 稍微多给 0.1秒 缓冲，让动作收尾更自然
        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        lastAttackEndTime = Time.time;

        if (isGrounded) SetAnimation(idleAnim, true);
    }

    // ... (HandleSpineAnimation, SetAnimation, OnDrawGizmos 不变) ...
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
        if (string.IsNullOrEmpty(name)) return;
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
