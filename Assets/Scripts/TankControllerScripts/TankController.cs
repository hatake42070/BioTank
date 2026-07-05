using System;
using System.Collections.Generic;
using UnityEngine;

namespace TankControllerScripts
{
    public class TankController : MonoBehaviour
    {
        private TankStateContext _stateContext;
        private TankInputHandler _inputHandler;
        private TankMovement _tankMovement;
        private TankShooter _tankShooter;
        [SerializeField] private TankData tankData;

        public TankData TankData => tankData;

        // 初期値は昔の時間
        private float _lastFireTime = -9999f;
        private Dictionary<System.Type, ITankState> _stateDictionary; // 状態の辞書

        public int currentHp;

        private void Start()
        {
            Initialize();
        }

        // 全ての部品を組み立てる、一番最初のスタート地点
        private void Initialize()
        {
            _inputHandler = GetComponent<TankInputHandler>();
            _tankMovement = GetComponent<TankMovement>();
            _tankShooter = GetComponent<TankShooter>();

            _stateContext = new TankStateContext();
            _stateDictionary = new Dictionary<Type, ITankState>()
            {
                { typeof(TankStateIdle), new TankStateIdle() },
                { typeof(TankStateMove), new TankStateMove() },
                { typeof(TankStateDead), new TankStateDead() }
            };

            _stateContext.Initialize(this, _stateDictionary[typeof(TankStateIdle)]);
        }

        private void Update()
        {
            // ここでIdleやMoveなどの状態ごとの更新の処理を行う
            _stateContext.Update();

            // 攻撃の判定(stateとは独立させる)
            HandleAttack();
        }

        private void HandleAttack()
        {
            if (_stateContext.CurrentState is TankStateDead)
            {
                return;
            }

            // 入力ハンドラを見て、攻撃ボタンが押されていたら発射
            if (_inputHandler.AttackTriggered && CanFire())
            {
                // TankShooterクラスの発射メソッドを呼ぶ
                _tankShooter.Fire(TankData.currentBullet);

                // フラグを下ろす（連続で弾が出ないようにする）
                _inputHandler.ConsumeAttack();
                ResetCooldown();
            }
        }

        /// <summary>
        /// 今、弾が打てるかどうかを判定する
        /// </summary>
        /// <returns></returns>
        private bool CanFire()
        {
            // 「現在のゲーム内時刻」が「最後に撃った時刻 ＋ クールダウン時間」を過ぎているか？
            return Time.time >= _lastFireTime + TankData.fireCooldown;
        }

        /// <summary>
        /// 撃った直後に呼ばれ、最後に撃った時刻を「今」に更新する
        /// </summary>
        private void ResetCooldown()
        {
            _lastFireTime = Time.time;
        }

        public TankInputHandler GetInputHandler()
        {
            return _inputHandler;
        }

        public TankMovement GetMovement()
        {
            return _tankMovement;
        }

        // 状態クラスからの「切り替えお願い窓口」
        public void ChangeState(Type newStateType)
        {
            // 辞書から新しい状態を取り出して、Contextに切り替えを命じる
            _stateContext.ChangeState(_stateDictionary[newStateType]);
        }

        public void TakeDamage(int damage)
        {
        }
    }
}