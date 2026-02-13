using UnityEngine;

public class SwordBeam : MonoBehaviour
{
    [Header("攻击属性")]
    public int damage = 20;     // 伤害值
    public float lifeTime = 0.3f; // 存活时间 (这个很重要，决定判定框存在多久)

    void Start()
    {
        // 生成后，过一小段时间自动销毁
        // 这样它就像一个“瞬间闪过的刀光”
        Destroy(gameObject, lifeTime);
    }

    // Update 函数被删除了，所以它不会飞了

    // --- 碰撞检测 (保持不变) ---
    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // 尝试获取碰到的物体上的怪物脚本
        EnemyController enemy = hitInfo.GetComponent<EnemyController>();

        if (enemy != null)
        {
            // 造成伤害
            enemy.TakeDamage(damage);

            // 注意：对于原地攻击，通常打中人不需要销毁自己，
            // 否则这一刀只能打中一个敌人。
            // 如果你想实现“AOE群伤”，就把下面这行 Destroy 注释掉！
            // Destroy(gameObject); 
        }
    }
}
