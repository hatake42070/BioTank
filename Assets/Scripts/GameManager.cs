using UnityEngine;
using System.Collections.Generic;
using TankControllerScripts;
using UnityEngine.InputSystem;
using MapEditorSystem.Runtime;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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
    // MapData単体ではなく、順番をまとめた「シーケンス」の配列を持つ
    public MapSequenceData[] experimentSequences;
    // public MapData[] allStages;       // 作成したマップデータの配列
    
    // スポーンポイントは Transform（オブジェクト）ではなく Vector3（座標）として記憶する
    private Vector3 _spawnPoint1P;
    private Vector3 _spawnPoint2P;
    
    // 参加したセッションを管理するリスト
    private List<PlayerSessionManager> _playerSessions = new List<PlayerSessionManager>();
    
    // 進行管理用の変数
    private int _selectedSequenceIndex = 0; // マップ選択画面で選んでいるパターンの番号
    private int _currentMapIndexInSequence = 0; // そのパターン内で現在何戦目か
    private GameObject _currentMapInstance; // 現在画面にあるマップオブジェクト（破棄用）
    
    // GameManager.cs の変数にタイマーを追加
    [Header("試合設定")]
    public float matchTimeLimit = 60f; 
    private float _currentMatchTime;
    private bool _isMatchActive = false;
    
    private string _resultMessage = ""; // 勝敗を描画するテキスト

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    // PlayerSessionManagerが酸化していないとき（０人）のキャンセルボタンの反応処理
    private void Update()
    {
        if(CurrentPhase == GamePhase.Lobby && _playerSessions.Count == 0)
        {
            bool isCancelPressed = false;
            // 1. キーボードの「Esc」キーが押されたかチェック
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                isCancelPressed = true;
            }

            // 2. 繋がっているゲームパッドの「B（東）ボタン」が押されたかチェック
            if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                isCancelPressed = true;
            }

            // キャンセルボタンが押されたら、タイトルへ戻る！
            if (isCancelPressed)
            {
                Debug.Log("参加者ゼロの状態でキャンセルされました。タイトルに戻ります。");
                GoBack();
            }
        }
        
        // バトル中の勝敗・タイムアップ監視
        if (CurrentPhase == GamePhase.Battle && _isMatchActive)
        {
            // プレイヤーが2人未満の時は判定処理を行わない（エラー落ち防止）
            if (_playerSessions.Count < 2) return;
            
            _currentMatchTime -= Time.deltaTime;
            
            // UIManagerに現在の時間を渡す（軽量化済みなので毎フレーム呼び出して大丈夫）
            if (GameUIManager.Instance != null)
            {
                GameUIManager.Instance.UpdateTimerDisplay(_currentMatchTime);
            }

            // SessionManager を通じて戦車が生きているか（Destroyされていないか）チェック
            bool p1Dead = (_playerSessions[0].SpawnedTank == null);
            bool p2Dead = (_playerSessions[1].SpawnedTank == null);

            // どちらかが死んだ、または時間切れになったらラウンド終了！
            if (_currentMatchTime <= 0 || p1Dead || p2Dead)
            {
                EndRound();
            }
        }
    }
    
    // ラウンド終了メソッド
    private void EndRound()
    {
        _isMatchActive = false; // 監視ストップ
        
        // ここでも念のためチェック
        if (_playerSessions.Count < 2) return;

        // SessionManager が記憶している「最終HP」を取得
        int hp1 = _playerSessions[0].CurrentHp;
        int hp2 = _playerSessions[1].CurrentHp;

        // 勝敗判定
        if (hp1 > hp2)
        {
            Debug.Log("1P WIN!!!");
            _resultMessage = "1P WIN!";
        }
        else if (hp2 > hp1)
        {
            Debug.Log("2P WIN!!!");
            _resultMessage = "2P WIN!";
        }
        else
        {
            Debug.Log("DRAW (引き分け)!!!");
            _resultMessage = "DRAW!";
        }

        // TODO: ここで勝利UI(Canvas)を表示し、数秒後に StartNextRound() を呼ぶ処理を入れる
        // UIManagerにテキストを渡して表示をお願いする
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowResult(_resultMessage);
        }

        // 3秒待機して次へ進むコルーチンを開始
        StartCoroutine(TransitionToNextRoundRoutine());
    }
    
    // 数秒待機してから次のラウンドへ移行するコルーチン
    private System.Collections.IEnumerator TransitionToNextRoundRoutine()
    {
        yield return new WaitForSeconds(3f);

        // ▼ UIManager に非表示をお願いする
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.HideResult();
        }

        // 次のラウンドへ進行
        StartNextRound();
    }

    public void ChangePhaseLobby()
    {
        CurrentPhase = GamePhase.Lobby;
        // ロビーに戻った時、まだ2人揃っていなければ参加受付を再開(EnableJoining)する
        if (_playerSessions.Count < 2)
        {
            if (UnityEngine.InputSystem.PlayerInputManager.instance != null)
            {
                UnityEngine.InputSystem.PlayerInputManager.instance.EnableJoining();
                //Debug.Log("参加受付を再開しました！");
            }
        }
    }
    
    // 戦車が決定されたときに呼ばれるメソッド
    public void RegisterPlayer(PlayerSessionManager newSession)
    {
        if (!_playerSessions.Contains(newSession))
        {
            _playerSessions.Add(newSession);
            // ここでリストに追加されるため、純粋な「参加順」で1P, 2Pが確定する
            
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
        if (experimentSequences == null || experimentSequences.Length == 0) return;

        if (direction > 0)
        {
            _selectedSequenceIndex = (_selectedSequenceIndex + 1) % experimentSequences.Length;
        }
        else if (direction < 0)
        {
            _selectedSequenceIndex--;
            if (_selectedSequenceIndex < 0)
            {
                _selectedSequenceIndex = experimentSequences.Length - 1;
            }
        }
        
        // どの実験パターンが選ばれているかログ出し
        MapSequenceData currentSequence = experimentSequences[_selectedSequenceIndex];
        Debug.Log($"パターン選択中: {currentSequence.sequenceName} をセットしました");
        
        // ゆくゆくはここで「マップ選択UI」の画像やテキストを更新する処理を呼ぶ
    }
    
    // マップ選択画面でマップが確定したときに呼ばれる
    public void SetupMap()
    {
        _currentMatchTime = matchTimeLimit; 
        _isMatchActive = true;
        
        CurrentPhase = GamePhase.Battle; // 状態をバトル中へ
        
        // マップ生成・出撃時にロビーのUI（キャンバス）を丸ごと非表示にする！
        if (LobbyUIManager.Instance != null)
        {
            LobbyUIManager.Instance.gameObject.SetActive(false);
        }
        
        _currentMapIndexInSequence = 0; // 1戦目にリセット

        // 実際のマップ生成処理を呼び出す
        LoadCurrentMapInSequence();
        
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }
    
    // 実際に指定されたマップを生成するコア処理（次ラウンド開始時にも使い回す）
    public void LoadCurrentMapInSequence()
    {
        _currentMatchTime = matchTimeLimit; 
        _isMatchActive = true;
        
        // 前のラウンドのマップが残っていれば消去する
        if (_currentMapInstance != null)
        {
            Destroy(_currentMapInstance);
        }

        // 現在選ばれているシーケンスと、その中の何戦目かを取得
        MapSequenceData currentSequence = experimentSequences[_selectedSequenceIndex];
        MapData mapToLoad = currentSequence.sequenceMaps[_currentMapIndexInSequence];

        // マップを生成（戻り値として生成された GameObject を受け取って保存する前提）
        _currentMapInstance = MapGenerator.GenerateMap(mapToLoad, commonPalette, out _spawnPoint1P, out _spawnPoint2P, out Vector3 mapCenter);
        
        if (Camera.main != null)
        {
            Camera.main.transform.rotation = Quaternion.Euler(70f, 0f, 0f);
            float pullBackDistance = 20f; 
            Camera.main.transform.position = mapCenter - (Camera.main.transform.forward * pullBackDistance);
        }

        // プレイヤーの配置（毎回新規生成）
        for (int i = 0; i < _playerSessions.Count; i++)
        {
            Vector3 targetSpawnPos = (i == 0) ? _spawnPoint1P : _spawnPoint2P;

            // 古い戦車の破棄はSessionManager内部で自動的にやってくれる
            _playerSessions[i].SpawnMyTank(targetSpawnPos);
        }
    }
    
    // 次のマップ（ラウンド）へ進む処理
    public void StartNextRound()
    {
        _currentMapIndexInSequence++;

        MapSequenceData currentSequence = experimentSequences[_selectedSequenceIndex];

        // 用意されたマップをすべて消化したか？
        if (_currentMapIndexInSequence >= currentSequence.sequenceMaps.Length)
        {
            Debug.Log("すべての実験シーケンスが終了しました！タイトルへ戻ります。");
            SceneManager.LoadScene("TitleScene");
            return;
        }

        // 次のマップを読み込む
        LoadCurrentMapInSequence();
    }
    
    // 特定のプレイヤーが1P（ホスト）かどうかを判定する便利関数
    public bool IsPlayer1(PlayerSessionManager session)
    {
        return _playerSessions.Count > 0 && _playerSessions[0] == session;
    }
    
    // 全員の準備が完了したかチェックする
    public void CheckAllPlayersReady()
    {
        // 0人の時だけでなく、「2人未満（1人の時）」も弾くようにする
        if (_playerSessions.Count < 2) return;
        
        bool isAllReady = _playerSessions.TrueForAll(s => s.IsReady);

        if (isAllReady)
        {
            Debug.Log("2人揃って準備完了！マップ選択に移行します。");
            CurrentPhase = GamePhase.MapSelect; // 状態を移行！
            
            // 3人目以降の参加受付をシャットアウトし、Jボタン連打による警告を防ぐ
            if (UnityEngine.InputSystem.PlayerInputManager.instance != null)
            {
                UnityEngine.InputSystem.PlayerInputManager.instance.DisableJoining();
            }
            
            // ここでUIをマップ選択画面に切り替える処理を呼ぶ
        }
    }
    
    // 戻る処理
    public void GoBack()
    {
        switch (CurrentPhase)
        {
            case GamePhase.Lobby:
                // ロビー画面で戻るボタンを押した場合、タイトル画面に戻る処理を呼ぶ
                SceneManager.LoadScene("TitleScene");
                break;
            case GamePhase.MapSelect:
                // マップ選択中に戻るボタンを押した場合、ロビー画面に戻す
                CurrentPhase = GamePhase.Lobby;
                // ここでUIをロビー画面に切り替える処理を呼ぶ
                break;
            case GamePhase.Battle:
                // 戦闘中に戻るボタンを押した場合、何もしない（もしくは確認ダイアログを出す）
                Debug.Log("戦闘中は戻れません！");
                break;
        }
    }
    
    // デバック用
    // 倒されたプレイヤーを再出撃（リスポーン）させる関数
    // public void RespawnPlayer(PlayerSessionManager session)
    // {
    //     // 自分がリストの何番目にいるか（0番目なら1P、1番目なら2P）を調べる
    //     int playerIndex = _playerSessions.IndexOf(session);
    //
    //     // 1Pなら _spawnPoint1P、2Pなら _spawnPoint2P を割り当てる
    //     Vector3 targetSpawnPos = (playerIndex == 0) ? _spawnPoint1P : _spawnPoint2P;
    //     
    //     // Sessionに再度戦車を作らせる
    //     session.SpawnMyTank(targetSpawnPos);
    // }
}
