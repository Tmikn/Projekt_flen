using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("怪物属性")]
    public int maxHealth = 100;
    public int currentHealth;

    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        // 如果是 Spine 角色，可能是在 MeshRenderer 上，这里假设是简单 Sprite
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null) originalColor = sr.color;
    }

    // --- 核心：受伤函数 ---
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("怪物受伤！剩余血量：" + currentHealth);

        // 1. 受伤视觉反馈 (闪烁红色)
        StartCoroutine(FlashColor());

        // 2. 死亡判定
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashColor()
    {
        if (sr != null)
        {
            sr.color = Color.red; // 变红
            yield return new WaitForSeconds(0.1f); // 等0.1秒
            sr.color = originalColor; // 恢复
        }
    }

    void Die()
    {
        Debug.Log("怪物挂了！");
        // 播放死亡特效或者直接销毁
        Destroy(gameObject);
    }
}
