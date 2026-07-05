using System;
using UnityEngine;

namespace TankControllerScripts
{
    public class TankMovement : MonoBehaviour
    {
        private CharacterController _tankController;

        private void Awake()
        {
            _tankController = GetComponent<CharacterController>();
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
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
            }
            
            //「入力方向」ではなく「キャラクターの正面方向」に進ませる
            // 体は回転しながら正面に進むので、慣性がのる
            Vector3 moveVelocity = transform.forward * moveSpeed;
            
            // 最終的な移動を実行
            _tankController.Move(moveVelocity * Time.deltaTime);
        }
    }
}