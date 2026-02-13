using UnityEngine;
using System.Collections;

public class ClashSystem : MonoBehaviour
{
    // --- 关键点：加了这一行，别的脚本才能找到它 ---
    public static ClashSystem Instance;

    [Header("骰子规则")]
    public int playerDiceSides = 20;
    public int enemyDiceSides = 6;

    private bool isClashing = false;

    void Awake()
    {
        // 初始化单例
        if (Instance == null) Instance = this;
    }

    // --- 外部调用接口 ---
    public void TriggerClash()
    {
        if (isClashing) return;
        StartCoroutine(ProcessClash());
    }

    IEnumerator ProcessClash()
    {
        isClashing = true;
        Debug.Log("⚔️ 真实碰撞！拼点开始！");

        // 1. 暂停时间
        if (TimeManager.Instance != null)
            TimeManager.Instance.StartClashPause();

        // 2. 计算结果
        int playerRoll = Random.Range(1, playerDiceSides + 1);
        int enemyRoll = Random.Range(1, enemyDiceSides + 1);

        // 3. UI 表演
        if (ClashUI.Instance != null)
        {
            ClashUI.Instance.ShowClash(1.0f, playerRoll, enemyRoll);
        }

        yield return new WaitForSecondsRealtime(1.5f);

        // 4. 恢复时间
        if (TimeManager.Instance != null)
            TimeManager.Instance.EndClashPause();

        // 5. 结算
        if (playerRoll >= enemyRoll)
        {
            Debug.Log("拼点胜利！打断怪物！");

            EnemyController ec = FindAnyObjectByType<EnemyController>();
            if (ec != null)
            {
                // 1. 先打断它的动作！(停止冲刺，关闭霸体)
                ec.InterruptAttack();

                // 2. 再造成伤害 (因为 InterruptAttack 已经关闭了 isDashing 霸体，
                //    所以这里其实用普通伤害 TakeDamage(20) 也可以了，
                //    但为了保险还是保留 true 参数)
                ec.TakeDamage(20, true);
            }

            if (TimeManager.Instance != null) TimeManager.Instance.DoHitStop(0.2f);
        }
        else
        {
            Debug.Log("拼点失败！");
            PlayerHealth ph = FindAnyObjectByType<PlayerHealth>();
            if (ph != null) ph.TakeDamage(10);

            if (TimeManager.Instance != null) TimeManager.Instance.DoHitStop(0.1f);
        }

        yield return new WaitForSecondsRealtime(0.5f);
        isClashing = false;
    }
}