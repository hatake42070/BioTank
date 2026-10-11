using Assets.Scripts.Bio;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

// 最終的にシングルトンなのでタイトルシーンにのみ置く
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

    private void Awake()
    {
        // 自分が最初の1個目なら、絶対に破棄されないように設定する
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
    public async Task TryConnectAsync()
    {
        if (IsScanning || IsConnected) return;

        IsScanning = true;
        HrConnectLog = "BLE Scan Started... (5秒間スキャンします)";
        Debug.Log(HrConnectLog);

        try
        {
            // 5秒間のスキャンを非同期で実行
            bool scanSuccess = await Task.Run(() => StartHeartRateScan());

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
    public async Task ForceRescanAsync()
    {
        if (IsScanning) return; // すでにスキャン中なら無視

        HrConnectLog = "既存の接続をリセットし、強制再スキャンを開始します...";
        Debug.Log(HrConnectLog);

        IsConnected = false;
        StopBLE(); // C++側に「今の接続を切れ」と命令する

        // デバイス側のBluetooth切断処理が完全に終わるのを0.5秒待つ
        await Task.Delay(500);

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