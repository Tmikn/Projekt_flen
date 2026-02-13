using UnityEngine;

public class SwordBeam : MonoBehaviour
{
    [Header("攻击属性")]
    public int damage = 20;
    public float lifeTime = 0.3f;

    void Start()
    {
        // 自动销毁，防止无限存在
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // --- 1. 拼点检测 (优先级最高) ---
        // 如果剑气撞到了怪物的攻击判定框 (Tag 为 Weapon_Enemy)
        if (hitInfo.CompareTag("Weapon_Enemy"))
        {
            Debug.Log("剑气撞到了怪物的攻击！触发拼点！");

            // 呼叫拼点系统
            if (ClashSystem.Instance != null)
            {
                ClashSystem.Instance.TriggerClash();
            }

            // 触发拼点后，剑气通常应该消失，或者被弹飞
            // 这里我们选择直接销毁剑气，表示被拦截了
            Destroy(gameObject);
            return;
        }

        // --- 2. 伤害检测 ---
        // 尝试获取怪物脚本 (Tag 为 Enemy)
        EnemyController enemy = hitInfo.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            // 造成伤害时的顿挫感
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.DoHitStop(0.1f);
            }

            // 打中敌人后销毁剑气 (如果不想穿透)
            // 如果你想做穿透攻击，就把下面这行注释掉
            Destroy(gameObject);
        }
    }
}
