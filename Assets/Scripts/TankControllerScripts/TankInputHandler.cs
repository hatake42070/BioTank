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
        public bool AttackTriggered { get; private set; }
        
        // 絶対座標ではなく「狙っている方向ベクトル（長さ1）」を保持する
        public Vector2 AimDirection { get; private set; } = Vector2.up; // 初期値は上向き
        
        private Vector2 _padAimInput;
        private Vector2 _mouseScreenPos;
        private bool _isUsingMouse = false;
        private Camera _mainCamera;
        
        private void Start()
        {
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// 移動入力があった時に自動で呼ばれる
        /// </summary>
        /// <param name="context"></param>
        public void  OnMove(InputAction.CallbackContext context)
        {
            // InputSystem側で設定したVector2の値をそのまま取得
            MoveInput = context.ReadValue<Vector2>();
            
            //Debug.Log($"【{gameObject.name}】が移動を受信！ 値: {MoveInput} / デバイス: {context.control.device.name}");
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
                _isUsingMouse = true;
                _mouseScreenPos = context.ReadValue<Vector2>();
            }
            else
            {
                _isUsingMouse = false;
                _padAimInput = context.ReadValue<Vector2>();
            }
        }

        private void Update()
        {
            if (_isUsingMouse)
            {
                if (_mainCamera != null)
                {
                    // マウスの場合：戦車の現在位置からマウスカーソルへの「方向」を計算する
                    Vector2 tankScreenPos = _mainCamera.WorldToScreenPoint(transform.position);
                    Vector2 dir = _mouseScreenPos - tankScreenPos;
                    
                    // タンクとマウスが完全に重なっていない時だけ方向を更新
                    if (dir.sqrMagnitude > 0.1f) 
                    {
                        AimDirection = dir.normalized;
                    }
                }
            }
            else
            {
                // パッドの場合：スティックの傾き自体がすでに「方向」なので、そのまま使う
                if (_padAimInput.sqrMagnitude > 0.01f)
                {
                    AimDirection = _padAimInput.normalized;
                }
            }
        }
    }
}