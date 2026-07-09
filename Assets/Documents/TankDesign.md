```mermaid
classDiagram
    %% 状態のインターフェース定義
    class ITankState {
        <<interface>>
        +Enter()
        +Update()
        +Exit()
    }

    %% コアシステム（司令塔と窓口）
    class TankController : MonoBehaviour, IDamageable{
        -TankStateContext _stateContext
        -TankInputHandler _inputHandler
        -TankMovement _movement
        -TankShooter _shooter
        -TankData _tankData     %% タンク固有のデータ
        +int CurrentHP          %% 現在のHP
        +TankData TankData()
        -Start()
        -Initialize()
        -Update()
        -void HandleAttack()
        -bool CanFire()         %% 弾が打てるかどうか(CD)を判定
        -void ResetCooldown()     %% 弾を打った時にCD時間をリセット
        +TankInputHandler GetInputHandler()
        +TankMovement GetMovement()
        +void ChangeState(Type newStateType)
        +TakeDamage(int damage) %% 戦車の受けるダメージ処理
    }
    
    class TankInputHandler {
        +Vector2 moveInput
        +bool attackTriggered
        +OnMove(InputAction.CallbackContext context)
        +OnAttack(InputAction.CallbackContext context)
        +ConsumeAttack()
    }

    class TankStateContext {
        -ITankState currentState
        -TankController tank
        +Initialize(TankController tank, ITankState startingState)
        +ChangeState(ITankState newState)
        +Update()
    }

    %% 機能コンポーネント (筋肉・実行部)
    class TankMovement {
        -CharacterController _characterController
        +Move(Vector2 input, float speed)
    }

    class TankShooter {
        -Transform _muzzle
        -GameObject _bulletPrefab
        +Shoot(WeaponData weaponData)
    }
    
    %% 発射された弾の処理
    class Bullet {
        -int _damage
        -int _remainingBounces
        +Initialize(WeaponData data)
        -OnTriggerEnter(Collider other) %% 衝突判定
    }

    %% データクラス (ScriptableObjectで実装)
    class TankData {
        +int MaxHP              %% タンクの最大HP
        +float BaseMoveSpeed    %% 基本移動速度
    }

    class WeaponData {
        +int AttackPower        %% 弾の攻撃力
        +int MaxBounces         %% 弾の反射回数
        +float BulletSpeed      %% 弾速
        +float FireCooldown     %% 射撃間隔
    }

    %% 状態 (States)
    class TankStateIdle { }
    class TankStateMove { }
    class TankStateDead { }

    %% --- 関係性 ---
    TankController --> TankData : 参照
    TankController --> TankStateContext : 保持
    TankController --> TankInputHandler : 保持
    TankController --> TankMovement : 保持
    TankController --> TankShooter : 保持

    TankStateContext o-- ITankState : 管理
    ITankState <|.. TankStateIdle : 実装
    ITankState <|.. TankStateMove : 実装
    ITankState <|.. TankStateDead : 実装

    TankShooter --> Bullet : 生成(Instantiate)
    TankShooter --> WeaponData : 参照
    Bullet --> TankController : TakeDamage()を呼び出し