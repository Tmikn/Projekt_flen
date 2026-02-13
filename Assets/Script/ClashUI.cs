using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class ClashUI : MonoBehaviour
{
    public static ClashUI Instance;

    [Header("UI 组件")]
    public GameObject clashPanel;

    // 这里改成了 TMP_Text，兼容性最强
    public TMP_Text playerScoreText;
    public TMP_Text enemyScoreText;
    public TMP_Text resultText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // --- 核心功能：开始显示拼点动画 ---
    // duration: 动画持续多久 (秒)
    // finalPlayerRoll: 玩家最终掷出的点数
    // finalEnemyRoll: 怪物最终掷出的点数
    public void ShowClash(float duration, int finalPlayerRoll, int finalEnemyRoll)
    {
        clashPanel.SetActive(true); // 打开面板
        StartCoroutine(RollDiceAnimation(duration, finalPlayerRoll, finalEnemyRoll));
    }

    IEnumerator RollDiceAnimation(float duration, int finalP, int finalE)
    {
        float timer = 0f;
        resultText.text = "VS"; // 中间显示 VS
        resultText.color = Color.white;

        // --- 阶段 1: 疯狂滚动 (制造紧张感) ---
        // 只要时间还没到，就一直随机变数字
        while (timer < duration)
        {
            // 随机显示 1-20 和 1-6 的数字
            playerScoreText.text = Random.Range(1, 21).ToString();
            enemyScoreText.text = Random.Range(1, 7).ToString();

            // 稍微让这种变化有点间隔，别闪瞎眼
            // 注意：因为此时 Time.timeScale 是 0，必须用 realtime
            yield return new WaitForSecondsRealtime(0.05f);

            timer += 0.05f;
        }

        // --- 阶段 2: 定格结果 ---
        playerScoreText.text = finalP.ToString();
        enemyScoreText.text = finalE.ToString();

        // --- 阶段 3: 显示输赢 ---
        if (finalP >= finalE)
        {
            resultText.text = "WIN!";
            resultText.color = Color.green;
            playerScoreText.color = Color.green;
        }
        else
        {
            resultText.text = "LOSE...";
            resultText.color = Color.red;
            playerScoreText.color = Color.red;
        }

        // 稍微停顿一下让玩家看清结果，然后再关闭
        yield return new WaitForSecondsRealtime(0.5f);

        clashPanel.SetActive(false); // 关闭面板
    }
}
