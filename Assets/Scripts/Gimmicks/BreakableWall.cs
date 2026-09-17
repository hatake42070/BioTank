using System;
using UnityEngine;

namespace Gimmicks
{
    /// <summary>
    /// 壊せる壁にアタッチするスクリプト
    /// </summary>
    public class BreakableWall : MonoBehaviour, IDamageable
    {
        [SerializeField] private int  maxHp = 3; // 壁の最大HP
        private int _currentHp; // 壁の現在HP
        [Header("演出")]
        [SerializeField] private GameObject destructionEffectPrefab; // 先ほど作った煙＋破片のプレハブ
        
        [Header("見た目の設定")]
        [Tooltip("ダメージごとのマテリアル．無傷，軽傷...とセットする")]
        [SerializeField] private Material[] damageMaterials;
        
        [SerializeField] private MeshRenderer targetMeshRenderer;

        private void Start()
        {
            _currentHp = maxHp;
            
            // 最初の見た目を設定
            UpdateMaterial();
        }

        public void TakeDamage(int damage)
        {
            _currentHp -= damage;
            AudioManager.Instance.PlaySE("LeafBreak");
            if (_currentHp <= 0)
            {
                // 破壊エフェクトが設定されていれば、壁と全く同じ位置・角度に生成する
                if (destructionEffectPrefab != null)
                {
                    Instantiate(destructionEffectPrefab, transform.position, transform.rotation);
                }
                
                Destroy(gameObject);
            }
            else
            {
                // まだ壊れていなければ、ひび割れを進行させる
                UpdateMaterial();
            }
        }
        
        // 現在のHPに応じてマテリアルを切り替える
        private void UpdateMaterial()
        {
            // 念のためのエラー回避
            if (targetMeshRenderer == null || damageMaterials.Length == 0) return;

            // ダメージレベル（どれくらい減ったか）を計算
            int damageLevel = maxHp - _currentHp;

            // 配列の範囲を超えないように安全対策をして切り替え
            if (damageLevel >= 0 && damageLevel < damageMaterials.Length)
            {
                targetMeshRenderer.material = damageMaterials[damageLevel];
            }
        }
    }
}