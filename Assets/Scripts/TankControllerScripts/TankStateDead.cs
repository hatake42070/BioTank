using UnityEngine;
namespace TankControllerScripts
{
    public class TankStateDead : ITankState
    {
        public void EnterState(TankController player)
        {
            // player (TankController) がアタッチされている大元のゲームオブジェクトを破壊する
            Object.Destroy(player.gameObject);
            
            // ※もし将来、爆発エフェクト（パーティクル）を出したり、
            // 爆発音を鳴らしたりする場合は、Destroyの直前にここに書きます！
        }

        public void UpdateState(TankController player)
        {
            
        }

        public void ExitState(TankController player)
        {
            
        }
    }
}