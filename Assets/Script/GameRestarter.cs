using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用这个才能切换场景

public class GameRestarter : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // 快捷键支持：按 'R' 键也可以重开
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
    }

    // --- 供按钮调用的公共方法 ---
    public void ReloadScene()
    {
        // 1. 【最重要的一步】把时间流速恢复正常！
        // 否则如果在拼点暂停时重开，新游戏里时间还是停的
        Time.timeScale = 1f;

        // 如果你的 TimeManager 里有变量记录状态，最好也重置一下
        // 但由于 TimeManager 也是随场景销毁重建的（除非你用了DontDestroyOnLoad），
        // 所以通常只需要重置 Time.timeScale 就够了。

        // 2. 获取当前场景的名字并重新加载
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);

        Debug.Log("🔄 游戏已重置！");
    }
}