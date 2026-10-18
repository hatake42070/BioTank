using Assets.Scripts.Bio;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

// 最終的にシングルトンなのでタイトルシーンにのみ置く
/// <summary>
/// 心拍の収集・履歴の記憶を行う
/// </summary>
public class BioSignalManager : MonoBehaviour
{
    public static BioSignalManager Instance { get; private set; }

    // --- 1. C++ (DLL) の関数をUnityにインポート ---
    [DllImport("BlePlugin")]
    private static extern bool StartHeartRateScan();

    [DllImport("BlePlugin")]
    private static extern bool ConnectToDevices();

    [DllImport("BlePlugin")]
    private static extern int GetHeartRate(int playerIndex);

    [DllImport("BlePlugin")]
    private static extern void StopBLE();

    // --- 2. 他のスクリプトから読み取るためのプロパティ ---
    public int[] RealHeartRates { get; private set; } = { 0, 0 };
    public int[] GameplayHeartRates { get; private set; } = new int[2];
    public string HrConnectLog { get; private set; } = "Rキーで心拍計をスキャンします";

    // 接続状態のフラグ
    public bool IsConnected { get; private set; } = false;

    // 現在スキャン中かどうかを外部から確認できるプロパティ
    public bool IsScanning { get; private set; } = false;

    // 現在のプロバイダー（リアル心拍なのか，偽なのかなど）
    private IHeartRateProvider currentProvider;

    // --- ベースライン関係 ---
    public enum BaselineMode
    {
        Static,
        Dynamic
    }
    public BaselineMode currentBaselineMode = BaselineMode.Dynamic;

    // 事前計測で決めた固定ベースライン（1P, 2P）
    public float[] staticBaselines = new float[2];    // 2つの要素を確保し，0.0fで初期化
    // 動的ベースラインの履歴（1P, 2P）
    public Queue<int>[] dynamicBpmHistories = new Queue<int>[2];

    private float _timeSinceLastRecord = 0.0f;
    private float _recordInterval = 1.0f;

    [Header("動的ベースライン")]
    [Tooltip("何秒間（何サンプル）を平均の基準にするか")]
    [SerializeField] private int baselineHistorySize = 15;

    [Header("ベースライン確認用")]
    [SerializeField] private float[] dynamicBpmBaselineDisplay = new float[2];
    [SerializeField] private float[] staticBpmBaselineDisplay = new float[2];

    private void Awake()
    {
        // 自分が最初の1個目なら、絶対に破棄されないように設定する
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Queueをそれぞれインスタンス化する
            dynamicBpmHistories[0] = new Queue<int>();
            dynamicBpmHistories[1] = new Queue<int>();
        }
        // もしタイトル画面に戻ってきた時など、2個目が生まれようとしたら自爆する
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateProvider();
        // 起動時にも一応1回目のスキャンを走らせておく
        _ = TryConnectAsync();
    }

    // 外部（TitleManager等）から何度でも呼べる接続メソッド
    public async UniTask TryConnectAsync()
    {
        if (IsScanning || IsConnected) return;

        IsScanning = true;
        HrConnectLog = "BLE Scan Started... (5秒間スキャンします)";
        Debug.Log(HrConnectLog);

        try
        {
            // 5秒間のスキャンを非同期で実行
            bool scanSuccess = await UniTask.RunOnThreadPool(() => StartHeartRateScan());

            if (scanSuccess)
            {
                HrConnectLog = "心拍系を発見！接続を開始します...";
                Debug.Log(HrConnectLog);
                IsConnected = ConnectToDevices();

                if (IsConnected)
                {
                    HrConnectLog = "接続成功！心拍数の取得を開始します";
                    Debug.Log(HrConnectLog);
                }
                else
                {
                    HrConnectLog = "心拍計は見つかりましたが、指定のMACアドレスとの接続に失敗しました";
                    Debug.LogWarning(HrConnectLog);
                }
            }
            else
            {
                HrConnectLog = "心拍計が見つからない、またはBluetoothがオフです";
                Debug.LogWarning(HrConnectLog);
            }
        }
        catch (System.Exception e)
        {
            // C++側でどんなエラーが起きてもここで受け止め、フリーズを防ぐ
            HrConnectLog = "BLE処理中にエラーが発生しました";
            Debug.LogError($"BLE処理中にエラーが発生しました: {e.Message}");
        }
        finally
        {
            // 成功しても、失敗しても、エラーで落ちても「絶対に」ここを通る
            // これにより確実にロックが解除され、Rキーが何度でも押せるようになる
            IsScanning = false;
        }
    }

    // いつでも強制的に再スキャンを行うメソッド
    public async UniTask ForceRescanAsync()
    {
        if (IsScanning) return; // すでにスキャン中なら無視

        HrConnectLog = "既存の接続をリセットし、強制再スキャンを開始します...";
        Debug.Log(HrConnectLog);

        IsConnected = false;
        StopBLE(); // C++側に「今の接続を切れ」と命令する

        // このManagerオブジェクトが破棄されたら、自動的に待機をキャンセルする安全装置
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        // デバイス側のBluetooth切断処理が完全に終わるのを0.5秒待つ
        await UniTask.Delay(TimeSpan.FromSeconds(0.5), cancellationToken: token);

        // 再びスキャン処理を呼ぶ
        await TryConnectAsync();
    }

    private void Update()
    {
        // 接続が完了している場合のみ、毎フレームC++の箱（latest_bpm）を見に行く
        if (IsConnected)
        {
            // C++の latest_bpm を一瞬で読み取る
            RealHeartRates[0] = GetHeartRate(0);
            RealHeartRates[1] = GetHeartRate(1);
        }

        if (currentProvider != null)
        {
            GameplayHeartRates[0] = currentProvider.GetHeartRate(0);
            GameplayHeartRates[1] = currentProvider.GetHeartRate(1);
        }

        _timeSinceLastRecord += Time.deltaTime;
        if (_timeSinceLastRecord >= _recordInterval)
        {
            _timeSinceLastRecord = 0f;

            // Queueへの記録
            dynamicBpmHistories[0].Enqueue(GameplayHeartRates[0]);
            dynamicBpmHistories[1].Enqueue(GameplayHeartRates[1]);

            if (dynamicBpmHistories[0].Count > baselineHistorySize)
            {
                dynamicBpmHistories[0].Dequeue();
            }
            if (dynamicBpmHistories[1].Count > baselineHistorySize)
            {
                dynamicBpmHistories[1].Dequeue();
            }
        }
    }

    public void UpdateProvider()
    {
        // ゲームモードによってカセットを変える
        BioGameMode mode = GameManager.Instance != null ? GameManager.Instance.CurrentBioMode : BioGameMode.Normal;

        // 疑似心拍モードのみ疑似心拍プロバイダー
        if (mode == BioGameMode.BioFake)
        {
            // --- ここでプロバイダーを切り替えてテストする ---
            currentProvider = new SinWaveFakeProvider();
        }
        else
        {
            currentProvider = new RealHeartRateProvider(this);
        }
    }

    // BioFeedbackControllerから呼ばれ，ベースラインを計算して返す
    public float GetDynamicBaselineAverage(int playerIndex)
    {
        if (dynamicBpmHistories[playerIndex].Count > 0)
        {
            float dynamicBpmBaseline = (float)dynamicBpmHistories[playerIndex].Average();
            dynamicBpmBaselineDisplay[playerIndex] = dynamicBpmBaseline;
            return dynamicBpmBaseline;
        }
        else
        {
            dynamicBpmBaselineDisplay[playerIndex] = GameplayHeartRates[playerIndex];
            return GameplayHeartRates[playerIndex];
        }
    }

    // UIのボタンやドロップダウンから呼ぶ用
    public void SetBaselineMode(BaselineMode mode)
    {
        currentBaselineMode = mode;
        Debug.Log($"ベースラインモードを {mode} に変更しました");
    }

    // タイトルのキャリブレーションボタンが押されたときに呼ばれる
    public async UniTask CalibrateStaticBaselineAsync(int playerIndex, int durationSeconds = 60)
    {
        Debug.Log($"{playerIndex + 1}PのStaticベースライン計測を{durationSeconds}秒計測します");

        long sumBpm = 0;
        int count = 0;

        // このManagerオブジェクトが破棄されたら、自動的に待機をキャンセルする安全装置
        CancellationToken token = this.GetCancellationTokenOnDestroy();

        // 指定秒数間、1秒ごとに心拍を足していく
        for (int i = 0; i < durationSeconds; i++)
        {
            // TimeSpan を使うと「1秒」という意図がより明確になります
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            sumBpm += GameplayHeartRates[playerIndex];
            count++;
        }

        // 平均を求めて、固定ベースラインとして保存する
        staticBaselines[playerIndex] = (float)sumBpm / count;
        Debug.Log($"{playerIndex + 1}Pの固定ベースラインが {staticBaselines[playerIndex]} に設定されました");
        staticBpmBaselineDisplay[playerIndex] = staticBaselines[playerIndex];
    }

    private void OnDestroy()
    {
        // Unityのプレイボタンを停止した時に、必ずBLEの切断処理を呼ぶ
        // これを忘れると、次回再生時にデバイスが「使用中」になって接続できなくなる
        if (Instance == this)
        {
            StopBLE();
            Debug.Log("BLEを安全に切断しました。");
        }
    }
}