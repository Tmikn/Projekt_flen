using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("属性")]
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        // 游戏开始，满血
        currentHealth = maxHealth;

        // 通知 UI 更新一次
        UpdateUI();
    }

    void Update()
    {
        // --- 测试用：按 H 键扣血 ---
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("主角受伤！剩余血量：" + currentHealth);

        // 限制血量不能低于 0
        if (currentHealth < 0) currentHealth = 0;

        // 更新 UI
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        // 调用我们刚才写的 UI 管家
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        Debug.Log("主角挂了！");

        // 1. 切断大脑 (禁止玩家操作)
        // 这一步必须有，否则死人还能走路
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.enabled = false;
        }

        // 2. 物理刹车 (防止尸体滑行)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 速度归零，原地停下

            // 【关键点】不要把 rb.simulated 设为 false，也不要改 BodyType
            // 保持它现在的样子，重力依然有效，如果死在空中会掉下来摔在地上
        }

        // 3. 视觉处理 (定格或变灰)
        var anim = GetComponentInChildren<Spine.Unity.SkeletonAnimation>();
        if (anim != null)
        {
            // 方法 A: 如果没有死亡动画，直接让画面定格
            anim.timeScale = 0;

            // 方法 B: 把尸体变灰，提示玩家已死亡
            // 把 Unity 的灰色 (Color.gray) 拆开传给 Spine
            anim.skeleton.R = Color.gray.r;
            anim.skeleton.G = Color.gray.g;
            anim.skeleton.B = Color.gray.b;
            anim.skeleton.A = Color.gray.a;
        }


    }
}
