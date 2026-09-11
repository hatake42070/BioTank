using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    // どこからでもアクセスできるインスタンス
    public static GameUIManager Instance { get; private set; }

    // 自分のCanvas
    public Transform CanvasTransform { get; private set; }

    [Header("リザルトUI")]
    [SerializeField]
    private GameObject resultUIPanel; // 背景パネルなど

    [SerializeField]
    private TextMeshProUGUI resultText; // 勝敗テキスト

    [Header("ゲーム内情報表示")]
    [SerializeField]
    private GameObject gameUIPanel;

    [SerializeField]
    private TextMeshProUGUI timerText; // タイマーを表示
    [SerializeField]
    private TextMeshProUGUI timerTextShadow; // タイマーを表示

    [Header("スコア表示")]
    [SerializeField]
    private TextMeshProUGUI p1ScoreText; // 画面上部のスコア用テキスト
    [SerializeField]
    private TextMeshProUGUI p1ScoreTextShadow; // 画面上部のスコア用テキスト

    [SerializeField]
    private TextMeshProUGUI p2ScoreText;
    [SerializeField]
    private TextMeshProUGUI p2ScoreTextShadow;

    private int _lastDisplayedSeconds = -1; // 最後に表示した秒数を記憶する用

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CanvasTransform = this.transform; // 自分がついているオブジェクト(GameUI)のTransformを記憶

            if (resultUIPanel != null) resultUIPanel.SetActive(false);
            if (gameUIPanel != null) gameUIPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// タイマー表示を更新する（1秒ごとにしかテキスト書き換えを行わない軽量版）
    /// </summary>
    public void UpdateTimerDisplay(float currentTime)
    {
        // 残り時間を切り上げて整数にする（例: 59.8秒 → 60秒）
        int seconds = Mathf.CeilToInt(currentTime);

        // 前回表示した秒数から変化があった時だけ、テキストを書き換える！
        if (seconds != _lastDisplayedSeconds)
        {
            _lastDisplayedSeconds = seconds;

            if (timerText != null)
            {
                // 文字列の変換はここでしか起きないので非常に軽い
                timerText.text = seconds.ToString();
            }
            if (timerTextShadow != null)
            {
                // 文字列の変換はここでしか起きないので非常に軽い
                timerTextShadow.text = seconds.ToString();
            }
        }
    }

    /// <summary>
    /// GameManagerから呼ばれて画面上部のスコアを書き換える
    /// </summary>
    public void UpdateScoreDisplay(int p1Score, int p2Score)
    {
        if (p1ScoreText != null &&  p2ScoreText != null && p1ScoreTextShadow != null && p2ScoreTextShadow != null)
        {
            // 色分けしたい場合は "<color=blue>"+p1Score+"</color> - <color=red>"+p2Score+"</color>" なども可能
            p1ScoreText.text = $"{p1Score}";
            p1ScoreTextShadow.text = $"{p1Score}";
            p2ScoreText.text = $"{p2Score}";
            p2ScoreTextShadow.text = $"{p2Score}";
        }
    }

    /// <summary>
    /// 試合開始時にUIを表示する用（GameManagerから呼ぶ）
    /// </summary>
    public void ShowGameUI()
    {
        if (gameUIPanel != null) gameUIPanel.SetActive(true);
        _lastDisplayedSeconds = -1; // リセットしておく
    }

    /// <summary>
    /// リザルト画面を表示する
    /// </summary>
    public void ShowResult(string message)
    {
        if (resultText != null)
        {
            resultText.text = message;
        }

        if (resultUIPanel != null)
        {
            // 自分自身をヒエラルキーの一番下に移動させる（最前面に表示）
            resultUIPanel.transform.SetAsLastSibling();
            
            resultUIPanel.SetActive(true);
        }
    }

    /// <summary>
    /// リザルト画面を非表示にする
    /// </summary>
    public void HideResult()
    {
        if (resultUIPanel != null)
        {
            resultUIPanel.SetActive(false);
        }
    }
}