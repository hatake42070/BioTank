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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
        /// 【重要】環境に応じて、一番アクセスしやすい保存先パスを決定する
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
            if (_isLogging) return;
            
            string dirPath = GetLogDirectoryPath(); // ファイルの保存場所を取得(Path)
            
            // 例: Phase1_BioReal_20261011_120339.csv
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss"); 
            _currentFilePath = Path.Combine(dirPath, $"{phaseName}_{timestamp}.csv");
            
            _streamWriter = new StreamWriter(_currentFilePath, false, System.Text.Encoding.UTF8); // パス、追記するか、エンコード指定
            _streamWriter.WriteLine("Time,P1_RealHR,P1_GameHR,P2_RealHR,P2_GameHR,Event");
        }
    }
}
