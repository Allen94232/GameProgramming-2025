using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 玩家名稱輸入 UI 控制器
/// 可以在遊戲開始前或設定選單中使用
/// </summary>
public class PlayerNameInput : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("名稱輸入框")]
    public TMP_InputField nameInputField;
    
    [Tooltip("確認按鈕")]
    public Button confirmButton;
    
    [Tooltip("顯示目前名稱的文字")]
    public TMP_Text currentNameText;

    private void Start()
    {
        // 載入目前的玩家名稱
        if (LeaderboardManager.Instance != null)
        {
            string currentName = LeaderboardManager.Instance.GetPlayerName();
            
            if (nameInputField != null)
            {
                nameInputField.text = currentName;
            }
            
            if (currentNameText != null)
            {
                currentNameText.text = $"目前名稱: {currentName}";
            }
        }
        
        // 設定按鈕事件
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
    }

    private void OnConfirmClicked()
    {
        if (nameInputField == null || LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("Missing references!");
            return;
        }

        string newName = nameInputField.text.Trim();
        
        // 驗證名稱
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("名稱不能為空！");
            return;
        }
        
        if (newName.Length < 2)
        {
            Debug.LogWarning("名稱至少需要 2 個字元！");
            return;
        }
        
        if (newName.Length > 20)
        {
            Debug.LogWarning("名稱不能超過 20 個字元！");
            newName = newName.Substring(0, 20);
        }
        
        // 設定新名稱
        LeaderboardManager.Instance.SetPlayerName(newName);
        
        // 更新顯示
        if (currentNameText != null)
        {
            currentNameText.text = $"目前名稱: {newName}";
        }
        
        Debug.Log($"✅ 名稱已更新為: {newName}");
    }

    /// <summary>
    /// 供外部呼叫：快速設定名稱（例如從按鈕）
    /// </summary>
    public void SetPlayerNameFromInput()
    {
        OnConfirmClicked();
    }

    /// <summary>
    /// 測試用：按 N 鍵快速輸入隨機名稱
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (nameInputField != null)
            {
                nameInputField.text = "Player" + Random.Range(1000, 9999);
            }
        }
    }
}
