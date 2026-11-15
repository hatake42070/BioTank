using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Bio
{
    // プロバイダーのインターフェース
    public interface IHeartRateProvider
    {
        int GetHeartRate(int playerIndex);
    }

    // 本物の心拍数を返すプロバイダー(供給者)
    public class RealHeartRateProvider : IHeartRateProvider
    {
        private BioSignalManager signalManager;
        // コンストラクタでBioSignalManagerのインスタンスを取得
        public RealHeartRateProvider(BioSignalManager manager)
        {
            signalManager = manager;
        }
        public int GetHeartRate(int playerIndex)
        {
            return signalManager.RealHeartRates[playerIndex];
        }
    }

    // 偽物（Sin波）を返すプロバイダー
    public class SinWaveFakeProvider : IHeartRateProvider
    {
        public int GetHeartRate(int playerIndex)
        {
            // 時間経過で 70 〜 130 を行ったり来たりするサイン波フェイク
            float wave = Mathf.Sin(Time.time * 0.5f);   // Time.time: ゲーム開始からの時間
            return (int)Mathf.Lerp(50f, 130f, (wave + 1f) / 2f);
        }
    }

    // 偽物（固定値）を返すプロバイダー
    public class FixedFakeProvider : IHeartRateProvider
    {
        public int GetHeartRate(int playerIndex) { return 0; }
    }

    // 偽物（あらかじめ取得した心拍数１）を返すプロバイダー
    public class CsvHeartRateProvider : IHeartRateProvider
    {
        private readonly List<(float time, int bpm)>[] _tracks;
        private readonly int[] _positions = new int[2];
        private readonly float _startedAt;

        public CsvHeartRateProvider(TextAsset playerOneCsv, TextAsset playerTwoCsv)
        {
            _tracks = new[] { ReadTrack(playerOneCsv), ReadTrack(playerTwoCsv) };
            _startedAt = Time.time;
        }

        public int GetHeartRate(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _tracks.Length) return 0;

            var track = _tracks[playerIndex];
            if (track.Count == 0) return 0;
            if (track.Count == 1) return track[0].bpm;

            const float lastSampleHoldSeconds = 1f;
            float loopDuration = track[track.Count - 1].time + lastSampleHoldSeconds;
            float elapsed = Mathf.Repeat(Time.time - _startedAt, loopDuration);

            if (elapsed < track[0].time) return 0;

            int index = _positions[playerIndex];

            // ループで再生位置が戻ったら、検索も先頭からやり直す。
            if (track[index].time > elapsed)
            {
                index = 0;
            }

            while (index + 1 < track.Count && track[index + 1].time <= elapsed)
            {
                index++;
            }

            _positions[playerIndex] = index;
            return track[index].bpm;
        }

        private static List<(float time, int bpm)> ReadTrack(TextAsset csv)
        {
            var track = new List<(float time, int bpm)>();
            if (csv == null) return track;

            using var reader = new StringReader(csv.text);
            reader.ReadLine(); // ヘッダー

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] columns = line.Split(',');
                if (columns.Length == 2
                    && float.TryParse(columns[0], NumberStyles.Float,
                        CultureInfo.InvariantCulture, out float time)
                    && time >= 0f && !float.IsInfinity(time)
                    && int.TryParse(columns[1], out int bpm)
                    && bpm >= 0)
                {
                    track.Add((time, bpm));
                }
            }

            track.Sort((a, b) => a.time.CompareTo(b.time));

            if (track.Count == 0)
            {
                Debug.LogWarning($"心拍CSVに有効なデータがありません: {csv.name}");
                return track;
            }

            float firstTime = track[0].time;
            for (int i = 0; i < track.Count; i++)
            {
                var sample = track[i];
                track[i] = (sample.time - firstTime, sample.bpm);
            }

            return track;
        }
    }

}
