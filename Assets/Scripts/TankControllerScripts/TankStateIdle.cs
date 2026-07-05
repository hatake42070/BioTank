using UnityEngine;

namespace TankControllerScripts
{
    public class TankStateIdle : ITankState
    {
        public void EnterState(TankController player)
        {
            Debug.Log("Idle状態に入る");
        }

        public void UpdateState(TankController player)
        {
            Vector2 input = player.GetInputHandler().MoveInput;
            // 入力がデッドゾーン以上になると呼び出す(ベクトルの2乗の長さ)
            if (input.sqrMagnitude > 0.01f)
            {
                // 入力があれば、Move状態へ切り替え
                player.ChangeState(typeof(TankStateMove));
                return; // これ以降の処理はしない
            }
        }

        public void ExitState(TankController player)
        {
            Debug.Log("Idle状態から抜ける");
        }
    }
}