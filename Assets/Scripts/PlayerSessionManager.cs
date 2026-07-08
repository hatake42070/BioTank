using UnityEngine;
using UnityEngine.InputSystem;

namespace TankControllerScripts
{
    /// <summary>
    /// GameManagerからプレイヤーが参加するたびに呼ばれるクラス
    /// </summary>
    public class PlayerSessionManager : MonoBehaviour
    {
        [Header("戦車設定")] public GameObject[] myTankPrefabs; // 生成したいTankのプレハブをセット

        private PlayerInput _playerInput;
        private bool _isReady = false;  // 使用する戦車が決定したかどうかのフラグ
        private int _selectedTankIndex = 0;

        // 生成した戦車の「入力受け取り窓口」を覚えておくための変数
        private TankInputHandler _spawnedTankInput;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();

            // 自分が何P（プレイヤー番号）として参加したかを取得してログに出す
            Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} が参加しました！");
        }

        private void Start()
        {
            // 自分が生成されたら、GameManagerに「参加しました！」と報告に行く
        }

        // GameManagerから呼ばれ、指定された場所に戦車を生成する
        public void SpawnMyTank(Transform spawnPoint)
        {
            // 指定された場所に自分の戦車を生成！
            GameObject myTank = Instantiate(myTankPrefabs[_selectedTankIndex], spawnPoint.position, spawnPoint.rotation);

            // 生成した戦車についている TankInputHandler を取得して記憶する
            _spawnedTankInput = myTank.GetComponent<TankInputHandler>();

            // 戦車が生まれたら、操作モードをUIから「Player（ゲーム中）」に切り替える！
            _playerInput.SwitchCurrentActionMap("Player");

            Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} の戦車を生成完了！");
        }

        // 十字キーでタンクを選ぶテスト（UIなし）
        public void OnNavigate(InputAction.CallbackContext context)
        {
            Debug.Log("onNavigate");
            if (_isReady || !context.performed) return;

            // 安全対策：もしインスペクターで配列に何もセットされていなかったら操作を無視する
            if (myTankPrefabs == null || myTankPrefabs.Length == 0) return;

            Vector2 navInput = context.ReadValue<Vector2>();

            if (navInput.x > 0.5f)
            {
                // 右入力：+1 して、配列の要素数で割った「余り」を出す（最後まできたら0に戻る）
                _selectedTankIndex = (_selectedTankIndex + 1) % myTankPrefabs.Length;
                
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} を選択中");
            }
            else if (navInput.x < -0.5f)
            {
                // 左入力：-1 して、0より小さくなったら一番最後（Length - 1）に戻す
                _selectedTankIndex--;
                if (_selectedTankIndex < 0)
                {
                    _selectedTankIndex = myTankPrefabs.Length - 1;
                }
                
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} を選択中");
            }
        }

        // 決定ボタンでReady状態にするテスト
        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.started && !_isReady)
            {
                _isReady = true;
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} で準備完了 (Ready) !");

                //　GameManagerに使用する戦車が決定したので
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnPlayerJoinedGame(this);
                }
            }
        }

        // ==================================================
        // 🚀 ゲーム中操作（戦車への命令伝達＝ルーティング）
        // ==================================================
        // PlayerInputからの「Move」入力を受け取り、そのまま戦車に横流しする！
        public void OnMove(InputAction.CallbackContext context)
        {
            if (_spawnedTankInput != null)
            {
                _spawnedTankInput.OnMove(context);
            }
        }

        // PlayerInputからの「Attack」入力を受け取り、そのまま戦車に横流しする！
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (_spawnedTankInput != null)
            {
                _spawnedTankInput.OnAttack(context);
            }
        }
        
        // デバッグ用：リスポーンボタンが押された時に呼ばれる
        public void OnDebugRespawn(InputAction.CallbackContext context)
        {
            // ボタンが押された瞬間 ＆ 戦車が破壊されて存在しない時だけ実行
            if (context.started && _spawnedTankInput == null)
            {
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : デバッグリスポーンを実行します！");

                if (GameManager.Instance != null)
                {
                    // GameManagerにリスポーンをお願いする
                    GameManager.Instance.RespawnPlayer(this);
                }
            }
        }
    }
}