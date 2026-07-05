namespace TankControllerScripts
{
    public class TankStateContext
    {
        private ITankState _currentState;
        private TankController _tankController;
        
        /// <summary>
        /// 初期化メソッド．タンクコントローラーと初期状態を設定する．
        /// </summary>
        /// <param name="tankController"></param>
        /// <param name="startingState"></param>
        public void Initialize(TankController tankController, ITankState startingState)
        {
            _tankController = tankController;
            _currentState = startingState;
            _currentState.EnterState(_tankController);
        }

        public void ChangeState(ITankState newState)
        {
            // 今の状態の終了処理を呼び出す
            _currentState.ExitState(_tankController);
            // 新しい状態に切り替え
            _currentState = newState;
            // 新しい状態の開始処理を呼び出す
            _currentState.EnterState(_tankController);
        }

        public void Update()
        {
            // ControllerのUpdateから呼ばれ、現在のステートのUpdateを実行するだけ
            _currentState.UpdateState(_tankController);
        }
    }
}
