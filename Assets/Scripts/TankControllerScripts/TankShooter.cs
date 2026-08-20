using UnityEngine;

namespace TankControllerScripts
{
    public class TankShooter : MonoBehaviour
    {
        [Header("発射設定")]
        // 弾が発射される位置（砲口）をインスペクターで設定
        [SerializeField] private Transform firePoint;
        
        [Header("壁貫通防止用の設定")]
        [SerializeField] private Transform tarretRoot; // 始点：砲台の根元
        [SerializeField] private LayerMask wallLayer;  // 壁のレイヤー
        
        // コライダーの「配列」
        private Collider[] _myColliders;

        private void Awake()
        {
            // 戦車本体や子に付いている【全て】のコライダーをまるごと取得！
            _myColliders = GetComponentsInChildren<Collider>();
        }
        
        public bool Fire(BulletData bulletData)
        {
            // --- 壁めり込み判定（Raycast） ---
            if (tarretRoot != null && firePoint != null)
            {
                Vector3 rayDirection = firePoint.position - tarretRoot.position;
                float rayDistance = rayDirection.magnitude;

                // 砲台の根元から銃口に向かってレーザーを撃ち、壁があるかチェック
                if (Physics.Raycast(tarretRoot.position, rayDirection.normalized, out RaycastHit hit, rayDistance, wallLayer))
                {
                    // 銃口が壁にめり込んでいるため発射キャンセル
                    Debug.Log("壁が近すぎて撃てません！");
                    return false; 
                }
            }
            
            // 安全対策：もし firePoint の設定を忘れていた場合は、戦車の中心から発射する
            Transform spawnPoint = firePoint != null ? firePoint : transform;

            // プレハブを生成
            GameObject bulletObj = Instantiate(bulletData.bulletPrefab, spawnPoint.position, spawnPoint.rotation);
    
            // 弾のスクリプトを取得して、データを渡す
            BulletController bullet = bulletObj.GetComponent<BulletController>();
            if (bullet != null)
            {
                bullet.Initialize(bulletData, _myColliders);
            }
            else
            {
                Debug.LogWarning("発射されたプレハブに BulletController がアタッチされていません！");
            }
            // 無事に発射されたので true を返す
            return true;
        }
    }
}