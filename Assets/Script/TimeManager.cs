using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("当前时间流速")]
    [Range(0f, 2f)]
    public float globalTimeScale = 1f;

    // 记录暂停前的时间流速，用于恢复
    private float timeScaleBeforePause;
    private bool isPaused = false;

    void Awake()
    {
        // 单例模式：保证全场只有一个时间管理者
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // 只有在非暂停状态下，才允许通过 Inspector 滑动条实时调整时间
        // 这样你在测试时拉动滚动条，游戏速度就会变
        if (!isPaused)
        {
            Time.timeScale = globalTimeScale;
        }
    }

    // --- 功能 1: 瞬间顿挫 (Hit Stop) ---
    // 用于攻击命中时的“卡肉感”，或者拼点结束那一瞬间的打击感
    // duration: 顿挫持续的真实时间 (秒)
    public void DoHitStop(float duration)
    {
        if (isPaused) return; // 如果已经是拼点状态，不要覆盖
        StartCoroutine(HitStopCoroutine(duration));
    }

    IEnumerator HitStopCoroutine(float duration)
    {
        // 1. 瞬间冻结时间
        float originalScale = Time.timeScale;
        Time.timeScale = 0f;

        // 2. 等待真实世界的几秒 (不受 timeScale 影响)
        yield return new WaitForSecondsRealtime(duration);

        // 3. 恢复时间
        Time.timeScale = originalScale;
    }

    // --- 功能 2: 拼点开始 (无限期暂停) ---
    // 当双方攻击判定相遇时调用
    public void StartClashPause()
    {
        if (isPaused) return;

        isPaused = true;
        timeScaleBeforePause = Time.timeScale; // 记住现在的速度
        Time.timeScale = 0f; // 完全静止

        Debug.Log("【时间停止】进入拼点阶段！");
    }

    // --- 功能 3: 拼点结束 (恢复时间) ---
    // 骰子扔完，由于惯性，我们可以选择瞬间恢复，或者给一个慢动作恢复
    public void EndClashPause()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = timeScaleBeforePause; // 恢复之前的速度(通常是1)

        Debug.Log("【时间恢复】拼点结束！");
    }

    // --- 功能 4: 慢动作 (Slow Motion) ---
    // 比如拼点成功者打出终结一击时，可以给个 0.5倍速
    public void SetSlowMotion(float scale, float duration)
    {
        StartCoroutine(SlowMotionCoroutine(scale, duration));
    }

    IEnumerator SlowMotionCoroutine(float scale, float duration)
    {
        globalTimeScale = scale;
        Time.timeScale = scale;

        yield return new WaitForSecondsRealtime(duration);

        globalTimeScale = 1f;
        Time.timeScale = 1f;
    }
}
