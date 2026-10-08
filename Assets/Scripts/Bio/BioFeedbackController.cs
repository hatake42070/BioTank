using TankControllerScripts;
using UnityEngine;

// タンクのプレハブにアタッチするコンポーネント
[RequireComponent(typeof(TankController))]
public class BioFeedbackController : MonoBehaviour
{
    private TankController _tankController;
    private int _playerIndex = -1; // 0 = 1P, 1 = 2P
    private bool _isBioModeEnabled = false;

    [Header("心拍フィードバック調整用パラメータ")]
    [SerializeField] private float minBpm = 50f;  // これ以下なら倍率変化なし（割合 0.0）
    [SerializeField] private float maxBpm = 110f; // これ以上なら最大変化（割合 1.0）
    
    [SerializeField] private float maxSpeedMultiplier = 2.0f;        // 最大で移動速度 1.5倍
    [SerializeField] private float maxFireIntervalMultiplier = 2.0f; // 最大で装填時間が 1.8倍（遅くなる）

    private void Awake()
    {
        _tankController = GetComponent<TankController>();
    }

    /// <summary>
    /// 戦車生成時に PlayerSessionManager から一度だけ呼ばれる初期化メソッド
    /// </summary>
    public void Initialize(int playerIndex, bool isBioModeEnabled)
    {
        _playerIndex = playerIndex;
        _isBioModeEnabled = isBioModeEnabled;

        // もし心拍連動オフのモードなら、初期状態（1.0倍）にリセットしておく
        if (!_isBioModeEnabled)
        {
            _tankController.ResetMultipliers();
        }
    }

    private void Update()
    {
        // 初期化されていない、または心拍連動オフのモードなら何もしない
        if (_playerIndex < 0 || !_isBioModeEnabled) return;

        // BioSignalManagerが存在しない場合は安全のためリセット
        if (BioSignalManager.Instance == null)
        {
            _tankController.ResetMultipliers();
            return;
        }

        // 1. 自分のプレイヤー番号(0 or 1)の心拍数を取得
        int currentBpm = BioSignalManager.Instance.GameplayHeartRates[_playerIndex];

        // 2. 未接続(-1)や待機中(0)なら、不公平にならないよう標準倍率(1.0)に戻す
        if (currentBpm <= 0)
        {
            _tankController.ResetMultipliers();
            return;
        }

        // 3. 心拍数を 0.0 〜 1.0 の割合（t）に変換
        float t = Mathf.InverseLerp(minBpm, maxBpm, currentBpm);

        // 4. トレードオフの倍率を計算
        float speedMult = Mathf.Lerp(1.0f, maxSpeedMultiplier, t);
        float fireIntervalMult = Mathf.Lerp(1.0f, maxFireIntervalMultiplier, t);

        // 5. TankControllerに計算結果を渡す
        _tankController.SetBioMultipliers(speedMult, fireIntervalMult);
    }
}