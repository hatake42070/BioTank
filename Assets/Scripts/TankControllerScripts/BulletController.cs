using UnityEngine;

namespace TankControllerScripts
{
    // 玉のプレハブにアタッチするクラス（物理演算のRigidbodyが必須）
    [RequireComponent(typeof(Rigidbody))]
    public class BulletController : MonoBehaviour
    {
        private BulletData _data;
        private Rigidbody _rb;
        private Collider[] _myColliders;
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
            _myColliders = GetComponentsInChildren<Collider>();
            _ownerColliders = ownerColliders;
            
            // 総当たりで無視設定（二重ループ）
            if (_ownerColliders != null && _myColliders != null)
            {
                foreach (Collider myCol in _myColliders)
                {
                    foreach (Collider ownerCol in _ownerColliders)
                    {
                        Physics.IgnoreCollision(myCol, ownerCol, true);
                    }
                }
            }

            // 前方（transform.forward）に向かって、データで指定された速度で飛ばす
            _rb.linearVelocity = transform.forward * _data.speed;

            // 何にも当たらずに飛んでいった場合、寿命（lifeTime）が来たら自動で消滅させる
            Destroy(gameObject, _data.lifeTime);
        }

        /// <summary>
        /// 何かにぶつかった時に自動で呼ばれる（Unityの物理エンジン機能）
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            // 何に当たって消えたかをコンソールに表示する（犯人探し用）
            Debug.Log($"💥 弾が【{collision.gameObject.name}】にぶつかって消滅しました！");

            // ここに「ぶつかった相手が戦車ならダメージを与える」処理を書く
            
            // ぶつかった相手が壁だった場合
            if (collision.gameObject.CompareTag("Wall"))
            {
                if (_ownerColliders != null && _myColliders != null)
                {
                    foreach (Collider myCol in _myColliders)
                    {
                        foreach (Collider ownerCol in _ownerColliders)
                        {
                            Physics.IgnoreCollision(myCol, ownerCol, false);
                        }
                    }
                }

                if (_boundCount > 0)
                {
                    _boundCount--;
                    // （※ここで物理的に弾を跳ね返すための処理が別に必要になります）
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }
            
            // （今はまだ相手のHPを減らす処理がないので省略）

            // ぶつかったら自分自身（玉）を消滅させる
            //Destroy(gameObject);
        }
    }
}