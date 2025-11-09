using UnityEngine;
using LootLocker.Requests;

/// <summary>
/// 簡單的測試腳本，用來查看排行榜
/// 按 L 鍵查看排行榜
/// 按 P 鍵查看自己的排名
/// </summary>
public class LeaderboardTest : MonoBehaviour
{
    [Header("測試設定")]
    [Tooltip("顯示前幾名")]
    public int topCount = 10;
    
    [Tooltip("指定要查看的關卡名稱（留空則使用目前場景）")]
    public string specificLevelName = "";

    void Update()
    {
        // 按 L 鍵查看排行榜
        if (Input.GetKeyDown(KeyCode.L))
        {
            ViewLeaderboard();
        }

        // 按 P 鍵查看自己的排名
        if (Input.GetKeyDown(KeyCode.P))
        {
            ViewPlayerRank();
        }
    }

    /// <summary>
    /// 查看排行榜前幾名
    /// </summary>
    public void ViewLeaderboard()
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardManager 不存在！請確認場景中有 LeaderboardManager。");
            return;
        }

        string levelToView = string.IsNullOrEmpty(specificLevelName) ? "目前場景" : specificLevelName;
        Debug.Log($"=== 正在獲取 {levelToView} 排行榜前 {topCount} 名 ===");
        
        LeaderboardManager.Instance.GetTopPlayers(topCount, (members) =>
        {
            if (members == null || members.Length == 0)
            {
                Debug.Log("排行榜目前沒有資料。");
                return;
            }

            Debug.Log($"=== 排行榜 (共 {members.Length} 筆) ===");
            
            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                float timeInSeconds = member.score / 1000f;
                string timeString = FormatTime(timeInSeconds);
                
                string medal = "";
                if (member.rank == 1) medal = "🥇";
                else if (member.rank == 2) medal = "🥈";
                else if (member.rank == 3) medal = "🥉";
                
                Debug.Log($"{medal} 第 {member.rank} 名 - {member.member_id} - 剩餘時間: {timeString}");
            }
            
            Debug.Log("=== 排行榜結束 ===");
        }, specificLevelName);
    }

    /// <summary>
    /// 查看自己的排名
    /// </summary>
    public void ViewPlayerRank()
    {
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogError("LeaderboardManager 不存在！");
            return;
        }

        string levelToView = string.IsNullOrEmpty(specificLevelName) ? "目前場景" : specificLevelName;
        Debug.Log($"=== 正在獲取你在 {levelToView} 的排名 ===");
        
        LeaderboardManager.Instance.GetPlayerRank((rank, score) =>
        {
            if (rank <= 0)
            {
                Debug.Log("你還沒有完成遊戲紀錄！完成一次遊戲後就會出現在排行榜上。");
                return;
            }

            float timeInSeconds = score / 1000f;
            string timeString = FormatTime(timeInSeconds);
            
            Debug.Log($"=== 你的排名 ===");
            Debug.Log($"排名: 第 {rank} 名");
            Debug.Log($"剩餘時間: {timeString}");
            Debug.Log($"=================");
        }, specificLevelName);
    }

    /// <summary>
    /// 格式化時間顯示
    /// </summary>
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 1000f) % 1000f);
        
        return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
    }
}
