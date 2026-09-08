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
        
        // Raycastを使わないので、Cameraのキャッシュ等も不要になります！
        
        /// <summary>
        /// 砲塔の向きを操作するメソッド
        /// </summary>
        /// <param name="aimDirection">入力された「方向」（長さ1のベクトル）</param>
        public void AimTurret(Vector2 aimDirection)
        {
            // 入力が無い（方向が定まっていない）場合は何もしない
            if (aimDirection.sqrMagnitude < 0.01f) return;
            
            // 画面の2D方向（X, Y）を、そのまま3D空間の平面方向（X, 0, Z）に変換するだけ
            Vector3 lookDirection = new Vector3(aimDirection.x, 0f, aimDirection.y);

            // 向きを適用する
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                // 親（戦車ボディ）がどれだけ回転していても、
                // 常にワールド空間での絶対的な方向を向かせるためズレることがなくなる
                turretTransform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}