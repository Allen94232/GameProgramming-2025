using UnityEngine;

/// <summary>
/// 診斷工具：檢查 LeaderboardManager 配置
/// 將此腳本附加到任何 GameObject 上，按 T 鍵即可檢查配置
/// </summary>
public class LeaderboardConfigTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestLeaderboardConfig();
        }
    }

    void TestLeaderboardConfig()
    {
        Debug.Log("========== LeaderboardManager 配置檢查 ==========");
        
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("❌ LeaderboardManager.Instance 是 null！");
            Debug.LogError("請確認：");
            Debug.LogError("1. 場景中有 LeaderboardManager GameObject");
            Debug.LogError("2. LeaderboardManager 腳本已附加到該 GameObject");
            Debug.LogError("3. GameObject 是啟用狀態（勾選框有打勾）");
            return;
        }

        Debug.Log("✅ LeaderboardManager.Instance 存在");
        Debug.Log($"Default Leaderboard ID: {LeaderboardManager.Instance.defaultLeaderboardId}");

        if (LeaderboardManager.Instance.leaderboardConfigs == null)
        {
            Debug.LogError("❌ leaderboardConfigs 是 null！");
            Debug.LogError("請在 Inspector 中設定 Leaderboard Configs 陣列");
            return;
        }

        Debug.Log($"✅ leaderboardConfigs 不是 null");
        Debug.Log($"配置數量: {LeaderboardManager.Instance.leaderboardConfigs.Length}");

        if (LeaderboardManager.Instance.leaderboardConfigs.Length == 0)
        {
            Debug.LogWarning("⚠️ leaderboardConfigs 陣列長度為 0");
            Debug.LogWarning("請在 Inspector 中：");
            Debug.LogWarning("1. 展開 Leaderboard Configs");
            Debug.LogWarning("2. 設定 Size = 1 (或更多)");
            Debug.LogWarning("3. 填入 Level Name 和 Leaderboard Id");
            return;
        }

        Debug.Log("\n📋 已配置的排行榜：");
        for (int i = 0; i < LeaderboardManager.Instance.leaderboardConfigs.Length; i++)
        {
            var config = LeaderboardManager.Instance.leaderboardConfigs[i];
            
            if (config == null)
            {
                Debug.LogError($"  [{i}] ❌ Config 是 null！");
                continue;
            }

            string levelName = config.levelName ?? "(null)";
            int leaderboardId = config.leaderboardId;

            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning($"  [{i}] ⚠️ Level Name 是空的！ID: {leaderboardId}");
            }
            else if (leaderboardId == 0)
            {
                Debug.LogWarning($"  [{i}] ⚠️ {levelName} - Leaderboard ID 是 0（可能未設定）");
            }
            else
            {
                Debug.Log($"  [{i}] ✅ {levelName} - ID: {leaderboardId}");
            }
        }

        Debug.Log("\n========== 檢查完成 ==========");
        Debug.Log("💡 如果看到警告或錯誤，請按照提示修正後再測試");
    }
}
