using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // AudioMixerを使うために必要

// Inspectorで音源を登録するための専用クラス
[System.Serializable]
public class SoundData
{
    public string name;      // 呼び出す時の名前 ("Shoot", "Explosion" など)
    public AudioClip clip;   // 音源データ
    [Range(0f, 1f)] 
    public float volume = 1.0f; // 個別の音量調整用
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup seMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource; // ひとまずSE用は1つでPlayOneShotを使う

    [Header("Sound Lists")]
    [SerializeField] private List<SoundData> bgmList = new List<SoundData>();
    [SerializeField] private List<SoundData> seList = new List<SoundData>();

    // 高速検索用の辞書
    private Dictionary<string, SoundData> _bgmDictionary = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> _seDictionary = new Dictionary<string, SoundData>();

    private void Awake()
    {
        // シングルトン化とシーン間引き継ぎ（DontDestroyOnLoad）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン遷移しても音を途切れさせない
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 起動時にListからDictionaryへ変換し、検索速度を最速にする
        foreach (var bgm in bgmList) _bgmDictionary.Add(bgm.name, bgm);
        foreach (var se in seList)   _seDictionary.Add(se.name, se);

        // Mixerの割り当て
        if (bgmSource != null) bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        if (seSource != null)  seSource.outputAudioMixerGroup = seMixerGroup;
    }

    /// <summary>
    /// SEを再生する（外部から AudioManager.Instance.PlaySE("名前") で呼ぶ）
    /// </summary>
    public void PlaySE(string seName)
    {
        if (_seDictionary.TryGetValue(seName, out SoundData data))
        {
            // PlayOneShotを使うことで、1つのAudioSourceで複数のSEを重ねて鳴らせる
            seSource.PlayOneShot(data.clip, data.volume);
        }
        else
        {
            Debug.LogWarning($"SE '{seName}' が見つかりません！");
        }
    }

    /// <summary>
    /// BGMを再生する
    /// </summary>
    public void PlayBGM(string bgmName)
    {
        if (_bgmDictionary.TryGetValue(bgmName, out SoundData data))
        {
            if (bgmSource.clip == data.clip) return; // 同じ曲なら何もしない

            bgmSource.clip = data.clip;
            bgmSource.volume = data.volume;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }
}