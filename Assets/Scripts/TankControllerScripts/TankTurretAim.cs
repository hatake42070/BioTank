using UnityEngine;

namespace TankControllerScripts
{
    public class TankTurretAim : MonoBehaviour
    {
        [SerializeField]
        private Transform turretTransform; 
        
        /// <summary>
        /// 砲塔の向きを操作するメソッド
        /// </summary>
        /// <param name="aimInput">マウス座標 or スティックの傾き</param>
        /// <param name="isMouseAim">マウス操作かどうか</param>
        public void AimTurret(Vector2 aimInput, bool isMouseAim)
        {
            // 自分の場所とaimInputから、砲塔の向きを決定する
            if (isMouseAim)
            {
                // ==========================================
                // ① マウス操作の場合（絶対座標から方角を計算）
                // ==========================================
                
                // カメラから、マウスの画面座標（aimInput）に向かって見えないレーザーを作る
                Ray ray = Camera.main.ScreenPointToRay(aimInput);

                // 砲台と同じ高さ(Y)に、見えない仮想の床（Plane）を作る
                Plane groundPlane = new Plane(Vector3.up, new Vector3(0, turretTransform.position.y, 0));

                float distance;
                // レーザーが仮想の床にぶつかったら
                if (groundPlane.Raycast(ray, out distance))
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
            else
            {
                // ==========================================
                // ② コントローラー操作の場合（傾きから直接方角を計算）
                // ==========================================
                
                // 入力が少しでもあれば（スティックの遊び/デッドゾーン対策）
                if (aimInput.sqrMagnitude > 0.01f)
                {
                    // 2Dの入力を、3D空間の方向(X, 0, Z)に変換する
                    Vector3 lookDirection = new Vector3(aimInput.x, 0f, aimInput.y);

                    // 向きを適用する
                    turretTransform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }
    }
}