using UnityEngine;

namespace TankControllerScripts
{
    /// <summary>
    /// 砲台を回す処理だけを行う
    /// </summary>
    public class TankTurretAim : MonoBehaviour
    {
        [SerializeField]
        private Transform turretTransform; 
        
        /// <summary>
        /// 砲塔の向きを操作するメソッド
        /// </summary>
        /// <param name="pointerScreenPosition">マウス座標 or スティックの傾き</param>
        public void AimTurret(Vector2 pointerScreenPosition)
        {
            // カメラから、マウスの画面座標（aimInput）に向かって見えないレーザーを作る
            Ray ray = Camera.main.ScreenPointToRay(pointerScreenPosition);

            // 砲台と同じ高さ(Y)に、見えない仮想の床（Plane）を作る
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, turretTransform.position.y, 0));
            
            // レーザーが仮想の床にぶつかったら
            if (groundPlane.Raycast(ray, out float distance))
            {
                // ぶつかった3D空間上の座標を取得
                Vector3 targetPoint = ray.GetPoint(distance);

                // 自分の座標から、ターゲットの座標への「方向」を計算する
                Vector3 lookDirection = targetPoint - turretTransform.position;
                lookDirection.y = 0; // 上下には傾かないようにYを0にする

                // 向きを適用する
                if (lookDirection.sqrMagnitude > 0.01f)
                {
                    turretTransform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
            
        }
    }
}