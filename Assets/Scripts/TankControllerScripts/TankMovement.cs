using System;
using UnityEngine;

namespace TankControllerScripts
{
    public class TankMovement : MonoBehaviour
    {
        private CharacterController _tankController;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Move(Vector2 input, float moveSpeed)
        {
            Vector3 moveDirection = new Vector3(input.x, 0, input.y);
            
            // 振り向き処理
            if (moveDirection.sqrMagnitude > 0.1f)
            {
                // 方向Vector3を姿勢に変換する.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                
                // Vector3.Lerp()と違い、円弧を描くように補完される
                // Rigidbodyを使う場合は、MoveRotationを使うと物理演算的に安全に回転できます
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRotation, 8f * Time.deltaTime));
            }
            
            //「入力方向」ではなく「キャラクターの正面方向」に進ませる
            // 体は回転しながら正面に進むので、慣性がのる
            // 速度の計算
            // 現在のRigidbodyの速度（重力による落下スピードなど）を取得
            Vector3 currentVelocity = _rb.linearVelocity;

            // 入力がある時だけ前進する速度を作り、入力がない時はゼロ（ピタッと止まる）にする
            Vector3 targetVelocity = Vector3.zero;
            if (moveDirection.sqrMagnitude > 0.1f)
            {
                // 「入力方向」ではなく「キャラクターの正面方向」に進ませる
                targetVelocity = transform.forward * moveSpeed;
            }

            // 重力（Y軸の落下スピード）だけは、現在の物理演算の数値をそのまま引き継ぐ
            targetVelocity.y = currentVelocity.y;

            // 計算した速度をRigidbodyに直接叩き込んで上書きする（キビキビ動く！）
            _rb.linearVelocity = targetVelocity;
        }
        
        /// <summary>
        /// 移動を強制的に停止する（Idleステートなどで呼ぶ）
        /// </summary>
        public void StopMovement()
        {
            // 現在の落下速度（Y軸）だけは取得しておく（空中でIdleになった時に不自然に浮かないため）
            float currentFallSpeed = _rb.linearVelocity.y;

            // 前後左右の速度（XとZ）を強制的に0にして、落下速度だけを維持する
            _rb.linearVelocity = new Vector3(0f, currentFallSpeed, 0f);
            //回転も止める
            _rb.angularVelocity = Vector3.zero;
        }
    }
}