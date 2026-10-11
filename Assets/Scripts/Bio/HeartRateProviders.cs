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
    public class FakeProvider : IHeartRateProvider
    {
        public int GetHeartRate(int playerIndex) { return 0; }
    }

}
