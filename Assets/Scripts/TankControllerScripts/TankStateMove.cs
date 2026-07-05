using UnityEngine;

namespace TankControllerScripts
{
    public class TankStateMove : ITankState
    {
        public void EnterState(TankController player)
        {
            Debug.Log("Move状態に入る");
            
            // アニメーションの再生
            
        }

        public void UpdateState(TankController player)
        {
            // 入力を取得
            Vector2 input = player.GetInputHandler().MoveInput;
            // 入力がデッドゾーン以下になったら、Idleに戻る
            if (input.sqrMagnitude <= 0.01f)
            {
                player.ChangeState(typeof(TankStateIdle));
                return; // これ以降の処理はしない
            }
            // 移動命令
            player.GetMovement().Move(input, player.TankData.baseMoveSpeed);
        }

        public void ExitState(TankController player)
        {
            Debug.Log("Move状態から抜ける");
        }
    }
}