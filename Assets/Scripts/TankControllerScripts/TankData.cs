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
        
        // 必要であれば、アニメーションの再生速度倍率や、固有のトリガー名などもここに持たせられます
        [Header("アニメーション設定")]
        public string moveAnimationTrigger = "Move";
    }
}