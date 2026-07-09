using UnityEngine;

namespace Gimmicks
{
    /// <summary>
    /// 壊せる壁にアタッチするスクリプト
    /// </summary>
    public class BreakableWall : MonoBehaviour, IDamageable
    {
        [SerializeField] private int  wallHp;
        public void TakeDamage(int damage)
        {
            wallHp -= damage;
            if (wallHp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}