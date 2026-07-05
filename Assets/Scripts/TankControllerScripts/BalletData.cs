using UnityEngine;

namespace TankControllerScripts
{
    [CreateAssetMenu(fileName = "NewBulletData", menuName = "TankGame/BulletData")]
    public class BulletData : ScriptableObject
    {
        [Header("見た目と判定")]
        public GameObject bulletPrefab; // 弾のプレハブ

        [Header("基本性能")]
        public int damage = 1;          // 攻撃力
        public float speed = 10f;       // 弾の飛ぶスピード
        public int balletHp;            //  弾のHP
        public float lifeTime = 5f;     // 何秒で消滅するか（射程距離の代わり）

        [Header("特殊挙動")]
        public int maxBounces = 1;      // 壁に反射する回数（0なら当たってすぐ消える）
        public bool canDestroyOtherBullets = false; // 弾同士がぶつかった時に相手を消せるか
    }
}