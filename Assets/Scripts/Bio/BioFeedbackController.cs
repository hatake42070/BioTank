using TankControllerScripts;
using UnityEngine;

/// <summary>
/// Tankにアタッチして心拍数をパラメータに変換する計算を行う
/// </summary>
[RequireComponent(typeof(TankController))]
public class BioFeedbackController : MonoBehaviour
{
    private TankController _tankController;
    private int _playerIndex = -1;
    private bool _isBioModeEnabled = false;

    [Header("デバッグ・確認用（再生中に即時反映）")]
    [Tooltip("チェックを入れると、スライダーのBPMで強制テストできます")]
    [SerializeField] private bool useDebugBpm = false;
    [Range(40, 160)]
    [SerializeField] private int debugBpm = 80;

    [Space(5)]  // Inspectorで値を確認する用
    [SerializeField] private int currentBpmDisplay;
    [SerializeField] private float currentSpeedMultDisplay;
    [SerializeField] private float currentBaselineDisplay; // ベースとする心拍数

    //[Header("心拍数の基準範囲")]
    //[SerializeField] private float minBpm = 60f;  // 安静時（割合 0.0）
    //[SerializeField] private float maxBpm = 130f; // 最大興奮時（割合 1.0）

    [Tooltip("平均からいくつ上がったら最大スピードにするか")]
    [SerializeField] private float upperThreshold = 15f;

    [Tooltip("平均からいくつ(BPM)下がったら「最低スピード」にするか")]
    [SerializeField] private float lowerThreshold = -15f;

    [Header("移動スピード倍率")]
    [SerializeField] private float minSpeedMultiplier = 0.5f;
    [SerializeField] private float maxSpeedMultiplier = 2.0f;

    [Header("発射間隔倍率")]
    [SerializeField] private float minFireIntervalMultiplier = 0.5f;
    [SerializeField] private float maxFireIntervalMultiplier = 2.0f;

    [Header("立体メッシュパーティクル演出")]
    [SerializeField] private GameObject auraParticlePrefab;
    private ParticleSystem _auraParticlesInstance;

    [Header("立体の太さ・長さ・密度設定")]
    [Tooltip("等倍より遅い時の色（青・水色系）")]
    [SerializeField] private Color slowColor = new Color(0.1f, 0.6f, 1.0f, 1.0f);

    [Tooltip("速くなるにつれて変化する色（黄 ➔ オレンジ ➔ 赤）")]
    [SerializeField] private Gradient fastColorGradient;

    [Tooltip("最大時の1秒あたりの柱の本数")]
    [SerializeField] private float maxLineCount = 80f;

    [Tooltip("柱の太さ（直径）")]
    [SerializeField] private float columnThickness = 0.15f;

    [Tooltip("最大時の柱の長さ（高さ）")]
    [SerializeField] private float maxColumnHeight = 2.5f;

    [SerializeField] private float maxLineSpeed = 7.0f;
    [SerializeField] private float particleHeightOffset = 1.5f;

    private ParticleSystem.MainModule _psMain;
    private ParticleSystem.EmissionModule _psEmission;
    private ParticleSystem.VelocityOverLifetimeModule _psVelocity;
    private ParticleSystem.ShapeModule _psShape;

    private void Awake()
    {
        _tankController = GetComponent<TankController>();

        if (auraParticlePrefab != null)
        {
            // Tankにパーティクルを生成
            GameObject obj = Instantiate(auraParticlePrefab, this.transform);   // Prefabをもとに新たなGameObjectを生成する
            obj.transform.localPosition = Vector3.zero; // 親の相対位置
            _auraParticlesInstance = obj.GetComponentInChildren<ParticleSystem>();

            if (_auraParticlesInstance != null)
            {
                _psMain = _auraParticlesInstance.main;
                _psEmission = _auraParticlesInstance.emission;
                _psVelocity = _auraParticlesInstance.velocityOverLifetime;
                _psShape = _auraParticlesInstance.shape;

                // 立体制御（3D Start Size）を強制有効化
                _psMain.startSize3D = true;
                _psEmission.enabled = true;
                _psVelocity.enabled = true;
                _psShape.enabled = true;
            }
        }

        if (fastColorGradient == null || fastColorGradient.colorKeys.Length <= 2)
        {
            fastColorGradient = new Gradient();
            var colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.yellow, 0.0f);
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f);
            colorKeys[2] = new GradientColorKey(Color.red, 1.0f);

            var alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
            fastColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    /// <summary>
    /// PlayerSessionManagerからTankが生成されたときに呼ばれる
    /// </summary>
    /// <param name="playerIndex">Tankナンバー</param>
    /// <param name="isBioModeEnabled">ノーマルモードのときfalse</param>
    public void Initialize(int playerIndex, bool isBioModeEnabled)
    {
        _playerIndex = playerIndex;
        _isBioModeEnabled = isBioModeEnabled;

        if (!_isBioModeEnabled && !useDebugBpm) // ノーマルモード && デバック心拍不使用時
        {
            _tankController.ResetMultipliers(); // スピード倍率を1.0
            if (_auraParticlesInstance != null) _auraParticlesInstance.gameObject.SetActive(false);
        }
        else  // ノーマルモード以外のとき
        {
            if (_auraParticlesInstance != null)
            {
                _auraParticlesInstance.gameObject.SetActive(true);
                _auraParticlesInstance.Play();
            }
        }
    }

    private void Update()
    {
        // 再生中にデバッグフラグを切り替えた場合も即座に対応
        if (!useDebugBpm)
        {
            if (_playerIndex < 0 || !_isBioModeEnabled) return;
        }

        int currentBpm = -1;

        if (useDebugBpm)
        {
            currentBpm = debugBpm;
            if (_auraParticlesInstance != null && !_auraParticlesInstance.gameObject.activeSelf)
            {
                _auraParticlesInstance.gameObject.SetActive(true);
                _auraParticlesInstance.Play();
            }
        }
        else if (BioSignalManager.Instance != null)
        {
            currentBpm = BioSignalManager.Instance.GameplayHeartRates[_playerIndex];
        }

        currentBpmDisplay = currentBpm;

        // エラー時（未接続など）は等倍に戻して終了
        if (currentBpm <= 0)
        {
            _tankController.ResetMultipliers();
            currentSpeedMultDisplay = 1.0f;
            currentBaselineDisplay = 0f;
            UpdateParticleVisuals(1.0f);
            return;
        }

        // --- ベースラインの記録と更新 ---
        float currentBaseline = 0f;
        if (BioSignalManager.Instance.currentBaselineMode == BioSignalManager.BaselineMode.Static)
        {
            currentBaseline = BioSignalManager.Instance.staticBaselines[_playerIndex];
        }
        else
        {
            currentBaseline = BioSignalManager.Instance.GetDynamicBaselineAverage(_playerIndex);
        }
        currentBaselineDisplay = currentBaseline;

        // --- ベースラインからのずれで性能を変化させる計算ロジック ---
        float speedMult = 1.0f;
        float fireIntervalMult = 1.0f;

        // 基準値からの「ズレ（差分）」を計算する
        float diff = currentBpm - currentBaseline;

        if (diff < 0)
        {
            // ① ベースラインより低い場合（落ち着いている時）
            // lowerThreshold（-15）に対して、今どれくらい下がっているかの割合 (0.0 ~ 1.0)
            float t = Mathf.Clamp01(diff / lowerThreshold);

            // 1.0(等倍) から 0.5(最小) へ向かって変化させる
            speedMult = Mathf.Lerp(1.0f, minSpeedMultiplier, t);
            fireIntervalMult = Mathf.Lerp(1.0f, minFireIntervalMultiplier, t);
        }
        else
        {
            // ② ベースライン以上の場合（焦っている時）
            // upperThreshold（+15）に対して、今どれくらい上がっているかの割合 (0.0 ~ 1.0)
            float t = Mathf.Clamp01(diff / upperThreshold);

            // 1.0(等倍) から 2.0(最大) へ向かって変化させる
            speedMult = Mathf.Lerp(1.0f, maxSpeedMultiplier, t);
            fireIntervalMult = Mathf.Lerp(1.0f, maxFireIntervalMultiplier, t);
        }

        currentSpeedMultDisplay = speedMult;
        _tankController.SetBioMultipliers(speedMult, fireIntervalMult);
        UpdateParticleVisuals(speedMult);

        //float t = Mathf.InverseLerp(minBpm, maxBpm, currentBpm);
        //float speedMult = Mathf.Lerp(minSpeedMultiplier, maxSpeedMultiplier, t);
        //float fireIntervalMult = Mathf.Lerp(minFireIntervalMultiplier, maxFireIntervalMultiplier, t);

        //currentSpeedMultDisplay = speedMult;
        //_tankController.SetBioMultipliers(speedMult, fireIntervalMult);
        //UpdateParticleVisuals(speedMult);
    }

    private void UpdateParticleVisuals(float currentSpeedMult)
    {
        if (_auraParticlesInstance == null) return;

        // 平常時（等倍付近）は消す
        if (Mathf.Abs(currentSpeedMult - 1.0f) < 0.05f)
        {
            _psEmission.rateOverTime = 0f;
            return;
        }

        // 3Dメッシュの太さ（X/Z）を固定適用
        _psMain.startSizeX = columnThickness;
        _psMain.startSizeZ = columnThickness;

        if (currentSpeedMult < 1.0f)
        {
            // 減速具合を 0.0 ~ 1.0 に変換
            float slowRatio = Mathf.InverseLerp(1.0f, minSpeedMultiplier, currentSpeedMult);
            _psMain.startColor = slowColor;

            // 遅いほど長く太くする
            _psMain.startSizeY = Mathf.Lerp(0.5f, maxColumnHeight, slowRatio);

            _psShape.position = new Vector3(0f, particleHeightOffset, 0f);
            _psVelocity.y = -Mathf.Lerp(2.0f, maxLineSpeed, slowRatio);
            _psEmission.rateOverTime = Mathf.Lerp(8f, maxLineCount * 0.7f, slowRatio);
        }
        else
        {
            // 【興奮時】下から上へ黄〜赤の杭が突き昇る
            float fastRatio = Mathf.InverseLerp(1.0f, maxSpeedMultiplier, currentSpeedMult);
            _psMain.startColor = fastColorGradient.Evaluate(fastRatio);

            // 速いほど長くする
            _psMain.startSizeY = Mathf.Lerp(0.5f, maxColumnHeight, fastRatio);

            _psShape.position = new Vector3(0f, 0.1f, 0f);
            _psVelocity.y = Mathf.Lerp(2.0f, maxLineSpeed, fastRatio);
            _psEmission.rateOverTime = Mathf.Lerp(12f, maxLineCount, fastRatio);
        }
    }
}