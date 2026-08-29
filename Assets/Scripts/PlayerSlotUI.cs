using UnityEngine;
using TMPro;

/// <summary>
/// 1P,2P用の画面枠を管理するクラス
/// Join前、選択中、準備完了の表示切替を担当する
/// </summary>
public class PlayerSlotUI : MonoBehaviour
{
    [Header("UIパーツ")]
    [SerializeField] private GameObject joinPromptPanel; // 「Press[J/O] to Join」の表示
    [SerializeField] private GameObject tankSelectPanel; // Tank Dataや選択UIの親
        
    [Header("Tank Data表示用")]
    [SerializeField] private TextMeshProUGUI tankNameText;
    // 今後、HPや攻撃力などのステータス表示用テキストを追加

    [Header("状態表示")]
    [SerializeField] private TextMeshProUGUI statusText; // 「Ready!」などを出す用

    // 初期状態（Join前）
    public void SetupUnjoinedState()
    {
        joinPromptPanel.SetActive(true);
        tankSelectPanel.SetActive(false);
        statusText.text = "";
    }

    // 参加完了 ＆ タンク選択中
    public void SetSelectingState(int tankIndex)
    {
        joinPromptPanel.SetActive(false);
        tankSelectPanel.SetActive(true);
            
        // ゆくゆくはTankData(ScriptableObject)を受け取って名前やステータスを更新する
        tankNameText.text = $"タンク: {tankIndex}";
        statusText.text = "Selecting...";
    }

    // 決定（Ready）状態
    public void SetReadyState()
    {
        statusText.text = "READY!";
        // 選択UIを少し暗くするなどの演出を入れるとさらに良くなります
    }
}