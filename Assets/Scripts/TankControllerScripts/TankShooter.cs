using UnityEngine;

namespace TankControllerScripts
{
    public class TankShooter : MonoBehaviour
    {
        [Header("発射設定")]
        // 弾が発射される位置（砲口）をインスペクターで設定
        [SerializeField] private Transform firePoint;
        
        // コライダーの「配列」
        private Collider[] _myColliders;

        private void Awake()
        {
            // 戦車本体や子に付いている【全て】のコライダーをまるごと取得！
            _myColliders = GetComponentsInChildren<Collider>();
        }
        
        public void Fire(BulletData bulletData)
        {
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
        }
    }
}