using UnityEngine;

namespace TankControllerScripts
{
    // 玉のプレハブにアタッチするクラス（物理演算のRigidbodyが必須）
    [RequireComponent(typeof(Rigidbody))]
    public class BulletController : MonoBehaviour
    {
        private BulletData _data;
        private Rigidbody _rb;
        private Collider _myCollider;
        private int _boundCount;

        // 撃った主のコライダーを覚えておく
        private Collider[] _ownerColliders;

        /// <summary>
        /// TankShooterから生成直後に呼ばれ、弾の性能をセットする
        /// </summary>
        public void Initialize(BulletData bulletData, Collider[] ownerColliders)
        {
            _data = bulletData;
            _rb = GetComponent<Rigidbody>();

            _boundCount = _data.maxBounces;

            // 子オブジェクトにある横倒しのCapsuleColliderを取得する
            _myCollider = GetComponentInChildren<Collider>();
            _ownerColliders = ownerColliders;

            // 総当たりで無視設定
            if (_ownerColliders != null && _myCollider != null)
            {
                foreach (Collider ownerCol in _ownerColliders)
                {
                    Physics.IgnoreCollision(_myCollider, ownerCol, true);
                }
            }

            // 前方（transform.forward）に向かって、データで指定された速度で飛ばす
            _rb.linearVelocity = transform.forward * _data.speed;

            // 何にも当たらずに飛んでいった場合、寿命（lifeTime）が来たら自動で消滅させる
            Destroy(gameObject, _data.lifeTime);
        }

        /// <summary>
        /// センサー（Is TriggerがONのコライダー）に触れた時に自動で呼ばれる
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            // ぶつかった相手（Trigger）の親に IDamageable があるか探す
            Gimmicks.IDamageable target = other.GetComponentInParent<Gimmicks.IDamageable>();

            if (target != null)
            {
                // 戦車にダメージを与える
                target.TakeDamage(_data.damage);

                // ダメージを与えたら、弾自身は消滅する
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 何かにぶつかった時に自動で呼ばれる（Unityの物理エンジン機能）
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            // 何に当たって消えたかをコンソールに表示する（犯人探し用）
            Debug.Log($"💥 弾が【{collision.gameObject.name}】にぶつかって消滅しました！");
            
            // ぶつかった相手がIDamageableかチェックする(壊せる壁)
            Gimmicks.IDamageable target = collision.gameObject.GetComponentInParent<Gimmicks.IDamageable>();
            
            if (target != null)
            {
                // ダメージを与える(壊れる壁のHPが減る)
                target.TakeDamage(_data.damage);

                // ダメージを与えたら弾は消滅させる場合
                Destroy(gameObject);
                return; // これ以降の反射処理などは行わない
            }

            // ぶつかった相手が壁だった場合
            if (collision.gameObject.CompareTag("Wall"))
            {
                if (_ownerColliders != null && _myCollider != null)
                {
                    foreach (Collider ownerCol in _ownerColliders)
                    {
                        if (ownerCol != null)
                        {
                            Physics.IgnoreCollision(_myCollider, ownerCol, false);
                        }
                    }
                }

                if (_boundCount > 0)
                {
                    // 壁で反射をする処理
                    _boundCount--;

                    // ぶつかった地点の壁の向き(法線)を取得する
                    Vector3 wallNormal = collision.contacts[0].normal;
                    // 現在の「弾の進行方向」と「壁の向き」から、反射する方向を計算する
                    Vector3 reflectDir = Vector3.Reflect(transform.forward, wallNormal);

                    // 弾が上にフワッと浮かないよう、Y軸（上下）のズレを強制的に0にする
                    reflectDir.y = 0f;

                    // 計算した反射の方向に弾を向かせる（normalized で長さを1に整える）
                    transform.forward = reflectDir.normalized;

                    // 新しい正面方向に向かって、元のスピードのまま飛ばし直す！（減速させない）
                    _rb.linearVelocity = transform.forward * _data.speed;
                }
                else
                {
                    Destroy(gameObject);
                }

                return;
            }

            // ぶつかった相手が銃弾だった場合
            if (collision.gameObject.CompareTag("Bullet"))
            {
                Destroy(gameObject);
                return;
            }
        }
    }
}