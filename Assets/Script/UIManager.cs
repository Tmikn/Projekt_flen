using UnityEngine;
using UnityEngine.UI; // 必须引入 UI 命名空间

public class UIManager : MonoBehaviour
{
    // 单例模式：让我们在任何地方都能直接找到 UIManager
    public static UIManager Instance;

    [Header("UI 组件")]
    public Image avatarImage; // 头像图片
    public Slider hpSlider;   // 血条滑动条

    void Awake()
    {
        // 初始化单例
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // --- 核心功能：更新血条 ---
    // current: 当前血量, max: 最大血量
    public void UpdateHealthBar(float current, float max)
    {
        if (hpSlider != null)
        {
            // Slider 的值是 0 到 1 之间的小数
            // 例如：50 / 100 = 0.5 (一半)
            hpSlider.value = current / max;
        }
    }
}