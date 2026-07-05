namespace TankControllerScripts
{
    public interface ITankState
    {
        public void EnterState(TankController player);
        public void UpdateState(TankController player);
        public void ExitState(TankController player);
    }
}