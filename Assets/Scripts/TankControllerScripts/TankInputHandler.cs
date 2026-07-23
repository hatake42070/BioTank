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
        public Vector2 AimInput { get; private set; }
        public bool IsMouseAim { get; private set; }
        public bool AttackTriggered { get; private set; }

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
            // InputSystem側で設定したVector2の値をそのまま取得
            AimInput = context.ReadValue<Vector2>();
            
            IsMouseAim = context.control.device.name.Contains("Mouse");
            
            Debug.Log($"【{gameObject.name}】が照準操作を受信！ 値: {AimInput} / デバイス: {context.control.device.name}");
        }
    }
}