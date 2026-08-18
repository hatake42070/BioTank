using UnityEngine;
using System.Collections.Generic;
using TankControllerScripts;
using UnityEngine.InputSystem;
using MapEditorSystem.Runtime;

/// <summary>
/// ゲームのフェーズを管理
/// </summary>
public enum GamePhase
{
    Lobby,       // タンク選択中
    MapSelect,   // 1Pがマップ選択中
    Battle       // 戦闘中
}

/// <summary>
/// 一番初めから存在し、ゲーム全体の状態を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    // どこからでもアクセスできるようにするためのシングルトン
    public static GameManager Instance;
    // 現在のフェーズ
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Lobby;
    
    [Header("マップ設定 (自作エディタ用)")]
    public TilePalette commonPalette; // 共通パレット
    public MapData[] allStages;       // 作成したマップデータの配列
    
    // スポーンポイントは Transform（オブジェクト）ではなく Vector3（座標）として記憶する
    private Vector3 _spawnPoint1P;
    private Vector3 _spawnPoint2P;
    
    // 参加したセッションを管理するリスト
    private List<PlayerSessionManager> _playerSessions = new List<PlayerSessionManager>();
    
    // 現在選択されているマップの番号
    private int _selectedMapIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    // 戦車が決定されたときに呼ばれるメソッド
    public void RegisterPlayer(PlayerSessionManager newSession)
    {
        if (!_playerSessions.Contains(newSession))
        {
            _playerSessions.Add(newSession);
            // ※ここでリストに追加されるため、純粋な「参加順」で1P, 2Pが確定します！
            
            // 2人（最大人数）揃った時点で、これ以上の新規参加受付を完全にストップする
            if (_playerSessions.Count >= 2)
            {
                if (UnityEngine.InputSystem.PlayerInputManager.instance != null)
                {
                    UnityEngine.InputSystem.PlayerInputManager.instance.DisableJoining();
                }
            }
        }
    }

    // 1Pから呼ばれるマップ切り替え関数
    public void ChangeMapIndex(int direction)
    {
        if (allStages == null || allStages.Length == 0) return;

        if (direction > 0)
        {
            _selectedMapIndex = (_selectedMapIndex + 1) % allStages.Length;
        }
        else if (direction < 0)
        {
            _selectedMapIndex--;
            if (_selectedMapIndex < 0)
            {
                _selectedMapIndex = allStages.Length - 1;
            }
        }
        
        Debug.Log($"マップ選択中: {_selectedMapIndex} 番のマップ候補");
        
        // ゆくゆくはここで「マップ選択UI」の画像やテキストを更新する処理を呼ぶ
    }
    
    // マップ選択画面でマップが確定したときに呼ばれる
    public void SetupMap()
    {
        CurrentPhase = GamePhase.Battle; // 状態をバトル中へ
        
        // マップ生成・出撃時にロビーのUI（キャンバス）を丸ごと非表示にする！
        if (LobbyUIManager.Instance != null)
        {
            LobbyUIManager.Instance.gameObject.SetActive(false);
        }

        // マップを生成してスポーン地点を取得
        // 新しい MapGenerator に生成を依頼し、out引数で1P/2Pの座標を受け取る
        MapData selectedData = allStages[_selectedMapIndex];
        MapGenerator.GenerateMap(selectedData, commonPalette, out _spawnPoint1P, out _spawnPoint2P, out Vector3 mapCenter);
        //GameObject currentMap = Instantiate(mapPrefabs[_selectedMapIndex], Vector3.zero, Quaternion.identity);
        //_spawnPoint1P = currentMap.transform.Find("SpawnPoint_1");
        //_spawnPoint2P = currentMap.transform.Find("SpawnPoint_2");
        
        if (Camera.main != null)
        {
            // 1. 画像で設定されている「最高の角度（X: 62, Y: 0, Z: 0）」を強制的にセットする
            Camera.main.transform.rotation = Quaternion.Euler(62f, 0f, 0f);

            // 2. カメラをマップの中心(mapCenter)から、「カメラが向いている方向の真後ろ」へ下げる
            // ※Orthographicの場合、どれだけ後ろに下がってもモノの大きさは変わらないため、
            // Clipping Planes (Near 0.3 ~ Far 100) の範囲内に収まる「適当な距離」でOKです。
            float pullBackDistance = 40f; 
            
            Camera.main.transform.position = mapCenter - (Camera.main.transform.forward * pullBackDistance);
            
            // 3. （ップごとにサイズが違う場合、カメラの「Size」も自動調整
            // Camera.main.orthographicSize = 25f; // 必要に応じてプログラムから上書きも可能
        }

        // 全員分の戦車をそれぞれのスポーン座標に生成！
        for (int i = 0; i < _playerSessions.Count; i++)
        {
            Vector3 targetSpawnPos = (i == 0) ? _spawnPoint1P : _spawnPoint2P;
            // ※注意：PlayerSessionManager側の引数も Transform から Vector3 に変更する必要があります
            _playerSessions[i].SpawnMyTank(targetSpawnPos);
        }
        
        // マウスカーソルを非表示にして、画面内に閉じ込める
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }
    
    // 特定のプレイヤーが1P（ホスト）かどうかを判定する便利関数
    public bool IsPlayer1(PlayerSessionManager session)
    {
        return _playerSessions.Count > 0 && _playerSessions[0] == session;
    }
    
    // 全員の準備が完了したかチェックする
    public void CheckAllPlayersReady()
    {
        if (_playerSessions.Count == 0) return;
        
        bool isAllReady = _playerSessions.TrueForAll(s => s.IsReady);

        if (isAllReady)
        {
            Debug.Log("全員準備完了！マップ選択に移行します。");
            CurrentPhase = GamePhase.MapSelect; // 状態を移行！
            
            // 3人目以降の参加受付をシャットアウトし、Jボタン連打による警告を防ぐ
            if (UnityEngine.InputSystem.PlayerInputManager.instance != null)
            {
                UnityEngine.InputSystem.PlayerInputManager.instance.DisableJoining();
            }
            
            // ここでUIをマップ選択画面に切り替える処理を呼ぶ
        }
    }
    
    // デバック用
    // 倒されたプレイヤーを再出撃（リスポーン）させる関数
    public void RespawnPlayer(PlayerSessionManager session)
    {
        // 自分がリストの何番目にいるか（0番目なら1P、1番目なら2P）を調べる
        int playerIndex = _playerSessions.IndexOf(session);

        // 1Pなら _spawnPoint1P、2Pなら _spawnPoint2P を割り当てる
        Vector3 targetSpawnPos = (playerIndex == 0) ? _spawnPoint1P : _spawnPoint2P;
        
        // Sessionに再度戦車を作らせる
        session.SpawnMyTank(targetSpawnPos);
    }
}
