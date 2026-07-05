using UnityEngine;

namespace TankControllerScripts
{
    // 右クリックメニューから簡単にこのデータファイルを作れるようにする属性
    [CreateAssetMenu(fileName = "NewTankData", menuName = "TankGame/TankData")]
    public class TankData : ScriptableObject
    {
        [Header("基本ステータス")]
        public int maxHp = 3;
        public float baseMoveSpeed = 5f;
        
        [Header("攻撃設定")]
        public BulletData currentBullet; // ← この戦車が現在撃つ弾のデータ！
        public float fireCooldown = 0.5f; // 発射間隔（連射速度）
        
        [Header("アニメーション設定")]
        public string moveAnimationTrigger = "Move";
    }
}