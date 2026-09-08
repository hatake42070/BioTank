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
        
        [Header("エイム設定")]
        [SerializeField] private float aimRotateSpeed = 10f;
        
        private Vector2 _padAimInput;
        private Vector2 _mouseScreenPos;
        private bool _isUsingMouse = false;
        private Camera _mainCamera;
        
        // 入力された「目標の方向」を一時保存する変数
        private Vector2 _targetAimDirection = Vector2.up;
        
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
            // 1. まずは「目標の方向（_targetAimDirection）」を計算する
            if (_isUsingMouse)
            {
                if (_mainCamera != null)
                {
                    Vector2 tankScreenPos = _mainCamera.WorldToScreenPoint(transform.position);
                    Vector2 dir = _mouseScreenPos - tankScreenPos;
                    if (dir.sqrMagnitude > 0.1f) 
                    {
                        _targetAimDirection = dir.normalized;
                    }
                }
            }
            else
            {
                if (_padAimInput.sqrMagnitude > 0.01f)
                {
                    _targetAimDirection = _padAimInput.normalized;
                }
            }

            // 2. 現在の方向(AimDirection)を、目標の方向(_targetAimDirection)に向かって滑らかに回転させる！
            // Vector3.Slerp を使うと、クロスヘアが綺麗な「円」を描いて目標に追いつくように回転する
            AimDirection = Vector3.Slerp(AimDirection, _targetAimDirection, aimRotateSpeed * Time.deltaTime);

            // 念のため長さを常に1に揃えておく
            if (AimDirection.sqrMagnitude > 0f)
            {
                AimDirection.Normalize();
            }
        }
    }
}