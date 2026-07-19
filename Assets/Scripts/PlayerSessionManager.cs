using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// GameManager(のPlayerInputManager)からプレイヤーが参加するたびに呼ばれるクラス
/// </summary>
public class PlayerSessionManager : MonoBehaviour
{
    [Header("戦車設定")]
    public GameObject[] myTankPrefabs; // 生成したいTankのプレハブをセット

    private PlayerInput _playerInput;
    public bool IsReady { get; private set; } = false; // 使用する戦車が決定したかどうかのフラグ
    private int _selectedTankIndex = 0;

    // 生成した戦車の「入力受け取り窓口」を覚えておくための変数
    private TankControllerScripts.TankInputHandler _spawnedTankInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        // 自分が何P（プレイヤー番号）として参加したかを取得してログに出す
        Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} が参加しました！");
    }

    private void Start()
    {
        // 自分が生成（Join）されたら、UIを「選択中」に切り替える
        if (LobbyUIManager.Instance != null)
        {
            LobbyUIManager.Instance.UpdatePlayerSelectUI(_playerInput.playerIndex, _selectedTankIndex);
        }

        // 自分が生成（Join）された瞬間に、GameManagerへ「参加したよ！」と登録に行く
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }
    }

    // GameManagerから呼ばれ、指定された場所に戦車を生成する
    public void SpawnMyTank(Transform spawnPoint)
    {
        // 指定された場所に自分の戦車を生成！
        GameObject myTank = Instantiate(myTankPrefabs[_selectedTankIndex], spawnPoint.position, spawnPoint.rotation);

        // 生成した戦車についている TankInputHandler を取得して記憶する
        _spawnedTankInput = myTank.GetComponent<TankControllerScripts.TankInputHandler>();

        // 戦車が生まれたら、操作モードをUIから「Player（ゲーム中）」に切り替える！
        _playerInput.SwitchCurrentActionMap("Player");

        Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} の戦車を生成完了！");
    }

    // 十字キーでタンク・マップを選ぶ処理
    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // 入力された方向をここで読み取る（両方のフェーズで使うため）
        Vector2 navInput = context.ReadValue<Vector2>();

        // ============================================
        // フェーズ１：ロビー（タンク選択）の場合
        // ============================================
        if (GameManager.Instance.CurrentPhase == GamePhase.Lobby)
        {
            // 安全対策：配列が空なら無視
            if (myTankPrefabs == null || myTankPrefabs.Length == 0) return;

            if (IsReady) return; // タンク準備完了済なら入力を無視する

            // タンク選択のためのインデックス処理
            if (navInput.x > 0.5f)
            {
                _selectedTankIndex = (_selectedTankIndex + 1) % myTankPrefabs.Length;
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} を選択中");
            }
            else if (navInput.x < -0.5f)
            {
                _selectedTankIndex--;
                if (_selectedTankIndex < 0)
                {
                    _selectedTankIndex = myTankPrefabs.Length - 1;
                }

                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} を選択中");
            }

            // タンクUIの更新はロビーフェーズでのみ行う
            if (LobbyUIManager.Instance != null)
            {
                LobbyUIManager.Instance.UpdatePlayerSelectUI(_playerInput.playerIndex, _selectedTankIndex);
            }
        }
        // ============================================
        // フェーズ２：マップ選択の場合
        // ============================================
        else if (GameManager.Instance.CurrentPhase == GamePhase.MapSelect)
        {
            // 1P（ホスト）しかマップ選択の操作ができないように制限する
            if (GameManager.Instance.IsPlayer1(this))
            {
                // GameManager側にあるマップの配列を切り替えるような処理を呼ぶ
                if (navInput.x > 0.5f)
                {
                    Debug.Log("1Pが次のマップを選択...");
                    GameManager.Instance.ChangeMapIndex(1); // 次のマップへ
                }
                else if (navInput.x < -0.5f)
                {
                    Debug.Log("1Pが前のマップを選択...");
                    GameManager.Instance.ChangeMapIndex(-1); // 前のマップへ
                }
            }
        }
    }

    // 決定ボタン（Join/Ready/マップ決定）
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        // ============================================
        // フェーズ１：ロビー（タンク選択）の場合
        // ============================================
        if (GameManager.Instance.CurrentPhase == GamePhase.Lobby)
        {
            if (!IsReady)
            {
                IsReady = true;

                if (LobbyUIManager.Instance != null)
                {
                    LobbyUIManager.Instance.UpdatePlayerReadyUI(_playerInput.playerIndex);
                }

                // GameManagerに自分が参加（Ready）したことを伝える
                if (GameManager.Instance != null)
                {
                    // 全員揃ったか確認する
                    GameManager.Instance.CheckAllPlayersReady();
                }
            }
        }
        // ============================================
        // フェーズ２：マップ選択の場合
        // ============================================
        else if (GameManager.Instance.CurrentPhase == GamePhase.MapSelect)
        {
            if (GameManager.Instance.IsPlayer1(this))
            {
                Debug.Log("1Pがマップを決定しました！バトル開始！");

                // GameManager にマップ生成と出撃を命じる
                GameManager.Instance.SetupMap();

                // ここでロビー全体のUI（キャンバス）を非表示にする
            }
        }
    }

    // ==================================================
    // 🚀 ゲーム中操作（戦車への命令伝達＝ルーティング）
    // ==================================================
    public void OnMove(InputAction.CallbackContext context)
    {
        if (_spawnedTankInput != null)
        {
            _spawnedTankInput.OnMove(context);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (_spawnedTankInput != null)
        {
            _spawnedTankInput.OnAttack(context);
        }
    }

    public void OnDebugRespawn(InputAction.CallbackContext context)
    {
        if (context.started && IsReady && _spawnedTankInput == null)
        {
            Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : デバッグリスポーンを実行します！");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RespawnPlayer(this);
            }
        }
    }
}