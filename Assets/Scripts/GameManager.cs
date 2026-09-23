using UnityEngine;
using System.IO;
using UnityEngine.Rendering; // ファイルの読み書き

// 3つのゲームモード
public enum BioGameMode
{
    Normal, // 心拍連動なし
    BioReal, // 実際の心拍センサーを使用
    BioFake // 疑似心拍を使用
}

// JSONに変換するためのデータだけの箱
// [System.Serializable] を付けることで、UnityのJsonUtilityでシリアライズ可能になる
[System.Serializable]
public class GameSettingsData
{
    public float masterVolume;
    public float bgmVolume;
    public float seVolume;
    public bool isMuted = false;
    public BioGameMode bioMode =  BioGameMode.Normal;
}

// ゲームモード（通常、心拍リアル、心拍フェイク）の現在の状態を記憶する
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("現在の設定データ")]
    public GameSettingsData currentSettings = new GameSettingsData();

    private string _saveFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // OSごとに最適な保存場所を自動で決定してパスを作る
            // (Windowsなら AppData/LocalLow/DefaultCompany/プロジェクト名/gamesettings.json)
            _saveFilePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            
            // 起動時にデータをロードする
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 現在の currentSettings の中身を Json ファイルとして保存する
    /// </summary>
    public void SaveSettings()
    {
        // オブジェクトをJSON形式の文字列に変換（trueで改行される）
        string json = JsonUtility.ToJson(currentSettings,  true);
        
        // テキストファイルとして書き出し
        File.WriteAllText(_saveFilePath, json);
        
        Debug.Log($"設定を保存しました。保存先:\n{_saveFilePath}");
    }

    /// <summary>
    /// JSON ファイルから設定を読み込み， currentSettings に上書きする
    /// </summary>
    public void LoadSettings()
    {
        // セーブファイルが存在するかチェック
        if (File.Exists(_saveFilePath))
        {
            // ファイルのテキストを読み込む
            string json = File.ReadAllText(_saveFilePath);
            
            // 文字列をオブジェクトに復元する
            currentSettings = JsonUtility.FromJson<GameSettingsData>(json);
            
            Debug.Log("設定を読み込みました。");
        }
        else
        {
            Debug.Log("セーブデータが見つかりません。デフォルト設定を使用します。");
            currentSettings = new GameSettingsData();
        }
    }
}
