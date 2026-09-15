using UnityEngine;
namespace TankControllerScripts
{
    public class TankStateDead : ITankState
    {
        public void EnterState(TankController player)
        {
            // player (TankController) がアタッチされている大元のゲームオブジェクトを破壊する
            AudioManager.Instance.PlaySE("Explosion2");
            
            // 2. 爆発エフェクトを生成する
            if (player.ExplosionPrefab != null)
            {
                // タンクの現在位置(player.transform.position)に、回転なし(Quaternion.identity)で生成
                Object.Instantiate(player.ExplosionPrefab, player.transform.position, Quaternion.identity);
            }
            
            Object.Destroy(player.gameObject);
        }

        public void UpdateState(TankController player)
        {
            
        }

        public void ExitState(TankController player)
        {
            
        }
    }
}