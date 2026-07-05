namespace TankControllerScripts
{
    public class TankStateContext
    {
        public ITankState CurrentState { get; private set; }
        private TankController _tankController;
        
        /// <summary>
        /// 初期化メソッド．タンクコントローラーと初期状態を設定する．
        /// </summary>
        /// <param name="tankController"></param>
        /// <param name="startingState"></param>
        public void Initialize(TankController tankController, ITankState startingState)
        {
            _tankController = tankController;
            CurrentState = startingState;
            CurrentState.EnterState(_tankController);
        }

        public void ChangeState(ITankState newState)
        {
            // 今の状態の終了処理を呼び出す
            CurrentState.ExitState(_tankController);
            // 新しい状態に切り替え
            CurrentState = newState;
            // 新しい状態の開始処理を呼び出す
            CurrentState.EnterState(_tankController);
        }

        public void Update()
        {
            // ControllerのUpdateから呼ばれ、現在のステートのUpdateを実行するだけ
            CurrentState.UpdateState(_tankController);
        }
    }
}
