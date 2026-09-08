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
        
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main; // カメラをキャッシュして処理を軽くする
        }
        
        /// <summary>
        /// 砲塔の向きを操作するメソッド
        /// </summary>
        /// <param name="aimDirection">入力された「方向」（長さ1のベクトル）</param>
        public void AimTurret(Vector2 aimDirection)
        {
            // 入力が無い（方向が定まっていない）場合は何もしない
            if (aimDirection.sqrMagnitude < 0.01f || _mainCamera == null) return;

            // 1. 戦車（砲塔）の現在の3D座標を、画面の2D座標に変換
            Vector2 tankScreenPos = _mainCamera.WorldToScreenPoint(turretTransform.position);

            // 2. 画面上で、戦車の位置から「入力方向」にうんと遠く離れた仮想のターゲット座標を作る
            Vector2 virtualTargetScreenPos = tankScreenPos + aimDirection * 1000f;

            // 3. カメラから、仮想ターゲット座標に向かって見えないレーザーを作る
            Ray ray = _mainCamera.ScreenPointToRay(virtualTargetScreenPos);

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