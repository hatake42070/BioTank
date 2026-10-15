using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Log
{
    internal class ExperimentLogger : MonoBehaviour
    {
        public static ExperimentLogger Instance { get; private set; }

        private StreamWriter _streamWriter;
        private string _currentFilePath;
        private bool _isLogging = false;    // 現在ログを記録中か
        private float _startTime;
        private float _timeSinceLastLog;
        private float _logInterval = 0.5f; // 0.5秒ごとに記録

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 環境に応じで最適なログファイルパスを作成する
        /// </summary>
        /// <summary>
        /// 環境に応じて、一番アクセスしやすい保存先パスを決定する
        /// </summary>
        private string GetLogDirectoryPath()
        {
            string baseDir;

#if UNITY_EDITOR
            // ① エディタ実行時: "プロジェクトフォルダ/Assets" の親 ➔ プロジェクトフォルダ直下
            baseDir = Directory.GetParent(Application.dataPath).FullName;

#elif UNITY_STANDALONE_OSX
            // ② Macビルド時: "アプリ名.app/Contents" の親は ".app"、さらにその親 ➔ .appと同じ階層
            string appContentsPath = Application.dataPath;
            string appPath = Directory.GetParent(appContentsPath).FullName;
            baseDir = Directory.GetParent(appPath).FullName;

#else
            // ③ Windowsビルド時: "実行ファイル名_Data" の親 ➔ .exeと同じ階層
            baseDir = Directory.GetParent(Application.dataPath).FullName;
#endif

            // 最終的な保存先パスを作成
            string logDirPath = Path.Combine(baseDir, "ExperimentLogs");

            if (!Directory.Exists(logDirPath))
            {
                Directory.CreateDirectory(logDirPath);
            }

            return logDirPath;
        }

        /// <summary>
        /// ログ記録を開始する
        /// </summary>
        public void StartLogging(string phaseName = "Match")
        {
            if (GameManager.Instance != null && !GameManager.Instance.currentSettings.isLog)
            {
                return;
            }

            if (_isLogging) return;

            string dirPath = GetLogDirectoryPath(); // ファイルの保存場所を取得(Path)

            // 例: Phase1_BioReal_20261011_120339.csv
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _currentFilePath = Path.Combine(dirPath, $"{phaseName}_{timestamp}.csv");

            _streamWriter = new StreamWriter(_currentFilePath, false, System.Text.Encoding.UTF8); // パス、追記するか、エンコード指定
            _streamWriter.WriteLine("Time,P1_RealHR,P1_GameHR,P2_RealHR,P2_GameHR,Event");

            _startTime = Time.time;
            _isLogging = true;
            _timeSinceLastLog = 0f;
        }

        /// <summary>
        /// 現在の記録を終了し，そのまま次のフェーズの記録を開始する
        /// </summary>
        public void SwitchNextPhase(string nextPhaseName)
        {
            StopLogging();
            StartLogging(nextPhaseName);
        }

        public void Update()
        {
            if (!_isLogging || BioSignalManager.Instance == null) return;

            _timeSinceLastLog += Time.deltaTime;
            if (_timeSinceLastLog > _logInterval)
            {
                _timeSinceLastLog = 0f;
                RecordDataRow("None");
            }
        }

        /// <summary>
        /// 外部からイベントタグをつけるときに使う
        /// </summary>
        public void LogEvent(string eventName)
        {
            if (!_isLogging) return;
            RecordDataRow(eventName);
        }

        private void RecordDataRow(string eventName)
        {
            // ログ記録開始からの経過時間
            float elapsedTime = Time.time - _startTime;

            int p1RealHR = BioSignalManager.Instance.RealHeartRates[0];
            int p2RealHR = BioSignalManager.Instance.RealHeartRates[1];
            int p1GameHR = BioSignalManager.Instance.GameplayHeartRates[0];
            int p2GameHR = BioSignalManager.Instance.GameplayHeartRates[1];

            string line = $"{elapsedTime:F2},{p1RealHR},{p1GameHR},{p2RealHR},{p2GameHR},{eventName}";
            _streamWriter.WriteLine(line);
        }

        public void StopLogging()
        {
            if (!_isLogging) return;

            _isLogging = false;
            if (_streamWriter != null)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
                _streamWriter.Dispose();
                _streamWriter = null;
            }

            Debug.Log("[Logger] 記録終了・保存完了");
        }

        private void OnDestroy()
        {
            StopLogging();
        }
    }
}
