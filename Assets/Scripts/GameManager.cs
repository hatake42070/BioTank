using UnityEngine;
using System.Collections.Generic;
using TankControllerScripts;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // どこからでもアクセスできるようにするためのシングルトン
    public static GameManager Instance;

    [Header("マップ設定")] public GameObject[] mapPrefabs;
    
    // 1P,2Pのスポーンポイント
    private Transform _spawnPoint1P;
    private Transform _spawnPoint2P;
    
    // 参加したセッションを管理するリスト
    private List<PlayerSessionManager> _playerSessions = new List<PlayerSessionManager>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // ゲーム開始直後にマップを原点に生成する
        GameObject currentMap = Instantiate(mapPrefabs[0], Vector3.zero, Quaternion.identity);
        // マップからスポーン地点を探して記憶する
        _spawnPoint1P = currentMap.transform.Find("SpawnPoint_1");
        _spawnPoint2P = currentMap.transform.Find("SpawnPoint_2");

        if (_spawnPoint1P == null || _spawnPoint2P == null)
        {
            Debug.Log("マップ内にSpawnPoint_1またはSpawnPoint_2が見つかりません");
        }
    }
    
    // Sessionが生成された時（プレイヤーが参加した時）に呼ばれる関数
    public void OnPlayerJoinedGame(PlayerSessionManager newSession)
    {
        _playerSessions.Add(newSession);

        // 人数に応じて割り当てるスポーン地点を変える
        Transform targetSpawn = (_playerSessions.Count == 1) ? _spawnPoint1P : _spawnPoint2P;

        // Sessionに「ここで戦車を作れ！」と命令を出す
        newSession.SpawnMyTank(targetSpawn);
    }
    
    // デバック用
    // 倒されたプレイヤーを再出撃（リスポーン）させる関数
    public void RespawnPlayer(PlayerSessionManager session)
    {
        // 自分がリストの何番目にいるか（0番目なら1P、1番目なら2P）を調べる
        int playerIndex = _playerSessions.IndexOf(session);

        // 1Pなら _spawnPoint1P、2Pなら _spawnPoint2P を割り当てる
        Transform targetSpawn = (playerIndex == 0) ? _spawnPoint1P : _spawnPoint2P;

        // Sessionに再度戦車を作らせる
        session.SpawnMyTank(targetSpawn);
    }
}
