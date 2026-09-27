using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

public class BioSignalManager : MonoBehaviour
{
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

    private async void Start()
    {
        Debug.Log("BLE Scan Started... (5秒間スキャンします)");

        // ★超重要: 5秒間処理が止まる StartHeartRateScan() を、
        // Unityのメイン処理とは別のスレッドに丸投げして非同期で待つ（フリーズ回避）
        bool scanSuccess = await Task.Run(() => StartHeartRateScan());

        if (scanSuccess)
        {
            Debug.Log("心拍計を発見！接続を開始します...");
            
            // 接続処理は一瞬で終わるのでそのまま実行
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
        // ★重要: Unityのプレイボタンを停止した時に、必ずBLEの切断処理を呼ぶ
        // これを忘れると、次回再生時にデバイスが「使用中」になって接続できなくなります
        StopBLE();
        Debug.Log("BLEを安全に切断しました。");
    }
}