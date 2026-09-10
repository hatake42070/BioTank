using UnityEngine;

namespace Gimmicks
{
    /// <summary>
    /// 壊せる壁にアタッチするスクリプト
    /// </summary>
    public class BreakableWall : MonoBehaviour, IDamageable
    {
        [SerializeField] private int  wallHp;
        [Header("演出")]
        [SerializeField] private GameObject destructionEffectPrefab; // 先ほど作った煙＋破片のプレハブ
        public void TakeDamage(int damage)
        {
            wallHp -= damage;
            if (wallHp <= 0)
            {
                // 破壊エフェクトが設定されていれば、壁と全く同じ位置・角度に生成する
                if (destructionEffectPrefab != null)
                {
                    Instantiate(destructionEffectPrefab, transform.position, transform.rotation);
                }
                
                Destroy(gameObject);
            }
        }
    }
}