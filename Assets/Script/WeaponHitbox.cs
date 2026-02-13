using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [Header("我是谁的武器？")]
    public bool isPlayerWeapon; // 勾选=玩家武器，不勾选=怪物武器
    public int damage = 10;

    [Header("碰撞器")]
    public Collider2D myCollider;

    void Start()
    {
        if (myCollider == null) myCollider = GetComponent<Collider2D>();
        // 默认先关闭碰撞，只有攻击时才打开
        if (myCollider != null) myCollider.enabled = false;
    }

    // --- 开启/关闭武器判定的方法 (给外部调用) ---
    public void EnableHitbox()
    {
        if (myCollider != null) myCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (myCollider != null) myCollider.enabled = false;
    }

    // --- 核心：碰撞检测 ---
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 检测拼点 (武器撞武器)
        if (isPlayerWeapon && other.CompareTag("Weapon_Enemy"))
        {
            // 呼叫拼点系统
            ClashSystem.Instance.TriggerClash();
            return; // 拼点优先，不再结算伤害
        }
        else if (!isPlayerWeapon && other.CompareTag("Weapon_Player"))
        {
            // 怪物撞到玩家武器，也是拼点，但通常由玩家那边触发就够了，
            // 为了防止双重触发，我们可以在 ClashSystem 里做防重保护
            ClashSystem.Instance.TriggerClash();
            return;
        }

        // 2. 检测伤害 (武器撞身体)
        // 如果我是玩家武器，撞到了怪物身体
        if (isPlayerWeapon && other.CompareTag("Enemy"))
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                // 此时也可以给一点顿挫感
                TimeManager.Instance.DoHitStop(0.05f);
            }
        }
        // 如果我是怪物武器，撞到了玩家身体
        else if (!isPlayerWeapon && other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);
                TimeManager.Instance.DoHitStop(0.05f);
            }
        }
    }
}
