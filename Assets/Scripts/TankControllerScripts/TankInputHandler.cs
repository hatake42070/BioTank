using UnityEngine;
using UnityEngine.InputSystem;

namespace TankControllerScripts
{
    /// <summary>
    /// 入力の窓口となるクラス
    /// </summary>
    public class TankInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool IsMouseAim { get; private set; }
        public bool AttackTriggered { get; private set; }
        private Vector2 _padAimInput;
        public Vector2 PointerScreenPosition { get; private set; }
        public float padCursorSpeed = 1000f;

        /// <summary>
        /// 移動入力があった時に自動で呼ばれる
        /// </summary>
        /// <param name="context"></param>
        public void  OnMove(InputAction.CallbackContext context)
        {
            // InputSystem側で設定したVector2の値をそのまま取得
            MoveInput = context.ReadValue<Vector2>();
            
            Debug.Log($"【{gameObject.name}】が移動を受信！ 値: {MoveInput} / デバイス: {context.control.device.name}");
        }

        /// <summary>
        /// 攻撃ボタンが操作されたときに自動で呼ばれる
        /// </summary>
        /// <param name="context"></param>
        public void OnAttack(InputAction.CallbackContext context)
        {
            // context.startedは「ボタンが押されたフレーム」だけtrueになる(旧wasPressedThisFrameと同じ)
            if (context.started)
            {
                AttackTriggered = true;
            }
        }

        /// <summary>
        /// State側で攻撃処理を実行した直後に、このフラグを下すために呼ぶ
        /// </summary>
        public void ConsumeAttack()
        {
            AttackTriggered = false;
        }

        /// <summary>
        /// 照準操作があった時に自動で呼ばれる
        /// </summary>
        /// <param name="context"></param>
        public void OnAim(InputAction.CallbackContext context)
        {
            if (context.control.device.name == "Mouse")
            {
                // マウスなら絶対座標を代入
                PointerScreenPosition = context.ReadValue<Vector2>();
                _padAimInput = Vector2.zero; // スティックの傾きは0にする
            }
            else
            {
                // パッドなら傾きを一時保存
                _padAimInput = context.ReadValue<Vector2>();
            }
        }

        private void Update()
        {
            if (_padAimInput.sqrMagnitude > 0.01f)
            {
                PointerScreenPosition += _padAimInput * (padCursorSpeed * Time.deltaTime);
            }
            // カーソルが画面の外に出ないようにする
            PointerScreenPosition = new Vector2(
                Mathf.Clamp(PointerScreenPosition.x, 0, Screen.width),
                Mathf.Clamp(PointerScreenPosition.y, 0, Screen.height)
            );
        }
    }
}