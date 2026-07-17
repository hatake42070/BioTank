using UnityEngine;

/// <summary>
/// ロビー画面のUIを管理するクラス
/// 画面全体のキャンバスにアタッチし、GameManagerとやり取りして
/// 1P / 2P のPlayerSlotUIを管理する
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager Instance;

    [SerializeField] private PlayerSlotUI[] playerSlots; // 0番目が1P用、1番目が2P用

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 最初は全員「Join前」状態にしておく[cite: 1]
        foreach (var slot in playerSlots)
        {
            slot.SetupUnjoinedState();
        }
    }

    // プレイヤーが参加した時（または戦車を切り替えた時）に呼ばれる
    public void UpdatePlayerSelectUI(int playerIndex, int tankIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerSlots.Length)
        {
            playerSlots[playerIndex].SetSelectingState(tankIndex);
        }
    }

    // プレイヤーがReadyになった時に呼ばれる
    public void UpdatePlayerReadyUI(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerSlots.Length)
        {
            playerSlots[playerIndex].SetReadyState();
        }
    }
}