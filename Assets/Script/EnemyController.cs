using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("基础属性")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("AI 移动设置")]
    public float moveSpeed = 2f;
    public float detectionRange = 6f;
    public float attackTriggerRange = 3f;

    [Header("战斗设置 (核心)")]
    public float chargeTime = 0.6f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float attackCooldown = 2.0f;

    [Header("组件")]
    public Rigidbody2D rb;
    // 【新增】引用攻击判定框脚本
    public WeaponHitbox weaponHitbox;

    private SpriteRenderer sr;
    private Color originalColor;
    private Transform player;

    private Vector3 initialScale;
    private bool isAttacking = false;
    private float lastAttackTime;
    private bool isDashing = false;
    // 【新增】用来存储当前正在跑的协程，方便随时打断
    private Coroutine currentAttackCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        initialScale = transform.localScale;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        if (isAttacking || Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackTriggerRange)
        {
            StartCoroutine(PerformDashAttack());
            currentAttackCoroutine = StartCoroutine(PerformDashAttack());
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        FacePlayer();
    }

    // --- 核心：蓄力冲刺攻击流程 ---
    IEnumerator PerformDashAttack()
    {
        isAttacking = true;
        StopMoving();

        // --- 阶段 1: 蓄力 ---
        if (sr != null) sr.color = Color.yellow;
        transform.localScale = new Vector3(transform.localScale.x, initialScale.y * 0.8f, initialScale.z);

        yield return new WaitForSeconds(chargeTime);

        // --- 阶段 2: 冲刺攻击开始！ ---
        transform.localScale = new Vector3(transform.localScale.x, initialScale.y, initialScale.z);
        if (sr != null) sr.color = Color.red;

        // 【新增】打开伤害判定框！
        // 这一刻起，如果撞到玩家武器会触发拼点，撞到玩家身体会扣血
        if (weaponHitbox != null) weaponHitbox.EnableHitbox();

        // 【新增】进入红色冲刺，开启霸体！
        // 此时普通攻击无效，必须拼点
        isDashing = true;

        Vector2 direction = (player.position - transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * dashSpeed;
        }

        yield return new WaitForSeconds(dashDuration);

        // --- 阶段 3: 攻击结束 ---
        StopMoving();
        if (sr != null) sr.color = originalColor;

        // 【新增】冲刺结束，关闭霸体
        isDashing = false;


        lastAttackTime = Time.time;
        isAttacking = false;
        yield return null;
    }

    // --- 【核心新增】被拼点打断时的紧急刹车函数 ---
    public void InterruptAttack()
    {
        Debug.Log("怪物攻击被拼点打断！");

        // 1. 强制停止当前的攻击协程
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        // 2. 立即重置所有状态标志位
        isAttacking = false;
        isDashing = false;

        // 3. 关闭伤害判定框
        if (weaponHitbox != null) weaponHitbox.DisableHitbox();

        // 4. 视觉重置
        if (sr != null) sr.color = originalColor;
        transform.localScale = new Vector3(transform.localScale.x, initialScale.y, initialScale.z);

        // 5. 物理重置 
        StopMoving();
        if (rb != null)
        {
            Vector2 knockbackDir = (transform.position - player.position).normalized;
            rb.AddForce(knockbackDir * 5f, ForceMode2D.Impulse);
        }

        // 6. 重置攻击冷却 (这里修正了变量名)
        lastAttackTime = Time.time;
    }

    // 【删除】原来的 OnCollisionEnter2D 已经不需要了
    // 伤害和拼点逻辑全部由子物体上的 WeaponHitbox 脚本接管

    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
    }

    void StopMoving()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(int damage, bool isClashDamage = false)
    {
        // 核心逻辑：
        // 如果怪物正在冲刺 (isDashing) 并且 这次伤害不是拼点伤害 (!isClashDamage)
        // 那么就忽略这次伤害 (霸体生效)
        if (isDashing && !isClashDamage)
        {
            Debug.Log("怪物处于冲刺霸体中，免疫普通攻击！请拼点！");
            return;
        }

        currentHealth -= damage;
        StartCoroutine(FlashHurt());
        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashHurt()
    {
        if (sr != null && currentHealth > 0)
        {
            Color currentColorBeforeHurt = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);

            if (isAttacking && sr.color == Color.red)
            {
                if (currentColorBeforeHurt != Color.red) sr.color = currentColorBeforeHurt;
            }
            else if (sr != null)
            {
                sr.color = originalColor;
            }
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackTriggerRange);
    }
}
