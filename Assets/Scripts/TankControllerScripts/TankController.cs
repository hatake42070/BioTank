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
            //_inputHandler.ReadInput();
            _stateContext.Update();
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