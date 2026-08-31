using TankControllerScripts;
using UnityEngine;
using TMPro;

/// <summary>
/// 1P,2P用の画面枠を管理するクラス
/// Join前、選択中、準備完了の表示切替を担当する
/// </summary>
public class PlayerSlotUI : MonoBehaviour
{
    [Header("UIパーツ")]
    [SerializeField]
    private GameObject joinPromptPanel; // 「Press[J/O] to Join」の表示

    [SerializeField]
    private GameObject tankSelectPanel; // Tank Dataや選択UIの親

    [Header("Tank Data表示用")]
    [SerializeField]
    private TextMeshProUGUI tankNameText;

    [SerializeField]
    private TextMeshProUGUI hpValueText; // HPの値を表示する用
    [SerializeField]
    private TextMeshProUGUI speedValueText; // Speedの値を表示する用
    [SerializeField]
    private TextMeshProUGUI attackValueText; // 攻撃力の値を表示する用
    [SerializeField]
    private TextMeshProUGUI cdValueText; // 連射間隔の値を表示する用
    [SerializeField]
    private TextMeshProUGUI bulletSpeedValueText; // 弾速の値を表示する用
    [SerializeField]
    private TextMeshProUGUI boundValueText; // 銃弾の反射回数の値を表示する用

    [Header("状態表示")]
    [SerializeField]
    private TextMeshProUGUI statusText; // 「Ready!」などを出す用

    // 初期状態（Join前）
    public void SetupUnjoinedState()
    {
        joinPromptPanel.SetActive(true);
        tankSelectPanel.SetActive(false);
        statusText.text = "";
    }

    // 参加完了 ＆ タンク選択中
    public void SetSelectingState(int tankIndex, TankData selectedData)
    {
        joinPromptPanel.SetActive(false);
        tankSelectPanel.SetActive(true);
        
        tankNameText.text = $"タンク: {tankIndex}";
        statusText.text = "Selecting...";
        
        // TankDataの表示
        hpValueText.text = selectedData.maxHp.ToString();
        speedValueText.text = selectedData.baseMoveSpeed.ToString();
        attackValueText.text = selectedData.currentBullet.damage.ToString();
        cdValueText.text = selectedData.fireCooldown.ToString();
        bulletSpeedValueText.text = selectedData.currentBullet.speed.ToString();
        boundValueText.text = selectedData.currentBullet.maxBounces.ToString();
    }

    // 決定（Ready）状態
    public void SetReadyState()
    {
        statusText.text = "READY!";
        // 選択UIを少し暗くするなどの演出を入れるとさらに良くなります
    }
}