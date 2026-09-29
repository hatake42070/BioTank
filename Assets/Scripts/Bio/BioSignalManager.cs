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
    public int RealHeartRate { get; private set; } = 0;
    public int GameplayHeartRate { get; private set; } = 0;
    
    // 接続状態のフラグ
    public bool IsConnected { get; private set; } = false;
    
    // 現在スキャン中かどうかを外部から確認できるプロパティ
    public bool IsScanning { get; private set; } = false;

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
        // 起動時にも一応1回目のスキャンを走らせておく
        _ = TryConnectAsync();
    }

    // 外部（TitleManager等）から何度でも呼べる接続メソッド
    public async Task TryConnectAsync()
    {
        // 既にスキャン中、または接続済みの場合は何もしない
        if (IsScanning || IsConnected) return;

        IsScanning = true;
        Debug.Log("BLE Scan Started... (5秒間スキャンします)");

        // 5秒間のスキャンを非同期で実行
        bool scanSuccess = await Task.Run(() => StartHeartRateScan());

        if (scanSuccess)
        {
            Debug.Log("心拍計を発見！接続を開始します...");
            IsConnected = ConnectToDevices();
            
            if (IsConnected) 
            {
                Debug.Log("接続成功！心拍数の取得を開始します。");
            } 
            else 
            {
                Debug.LogWarning("心拍計は見つかりましたが、接続に失敗しました。");
            }
        }
        else
        {
            Debug.LogWarning("心拍計が見つからない、またはBluetoothがオフです。");
        }

        // スキャン終了
        IsScanning = false;
    }

    private void Update()
    {
        // 接続が完了している場合のみ、毎フレームC++の箱（latest_bpm）を見に行く
        if (IsConnected)
        {
            // C++の latest_bpm[0] を一瞬で読み取る
            RealHeartRate = GetHeartRate(0);
            
            // （本番ではここに GameManager のモード分岐を入れます）
            GameplayHeartRate = RealHeartRate; 

            // テスト用：値が更新されているかコンソールで確認（邪魔になったら消してください）
            Debug.Log($"現在の心拍数: {RealHeartRate}");
        }
    }

    private void OnDestroy()
    {
        // Unityのプレイボタンを停止した時に、必ずBLEの切断処理を呼ぶ
        // これを忘れると、次回再生時にデバイスが「使用中」になって接続できなくなる
        StopBLE();
        Debug.Log("BLEを安全に切断しました。");
    }
}