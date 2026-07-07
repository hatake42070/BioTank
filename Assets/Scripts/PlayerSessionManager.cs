using UnityEngine;
using UnityEngine.InputSystem;

namespace TankControllerScripts
{
    public class PlayerSessionManager : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private bool _isReady = false;
        private int _selectedTankIndex = 0;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();

            // 自分が何P（プレイヤー番号）として参加したかを取得してログに出す
            Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} が参加しました！");
        }

        // 十字キーでタンクを選ぶテスト（UIなし）
        public void OnNavigate(InputAction.CallbackContext context)
        {
            Debug.Log("onNavigate");
            if (_isReady || !context.performed) return;

            Vector2 navInput = context.ReadValue<Vector2>();

            if (navInput.x > 0.5f)
            {
                _selectedTankIndex++;
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} を選択中");
            }
            else if (navInput.x < -0.5f)
            {
                _selectedTankIndex--;
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : タンク {_selectedTankIndex} を選択中");
            }
        }

        // 決定ボタンでReady状態にするテスト
        public void OnSubmit(InputAction.CallbackContext context)
        {
            Debug.Log("onSubmit");
            if (context.started && !_isReady)
            {
                _isReady = true;
                Debug.Log($"プレイヤー {_playerInput.playerIndex + 1} : 準備完了 (Ready) !");
            }
        }
    }
}