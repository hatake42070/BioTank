using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField]
    private CanvasGroup mainMenuPanel;

    [SerializeField]
    private CanvasGroup optionsPanel;

    [Header("First Select Buttons")]
    [SerializeField]
    private GameObject firstMainMenuButton; // タイトル最初の選択ボタン

    [SerializeField]
    private GameObject firstOptionButton; // オプション最初の選択ボタン

    // パネルとボタンをセットで管理するための設計図
    [System.Serializable]
    public struct SubPanelData
    {
        public string panelName;              // 管理しやすいように名前
        public CanvasGroup panel;             // 対象のパネル
        public GameObject firstSelectedButton; // 最初に選択させたいボタン
    }

    [Header("Sub Panels")]
    [SerializeField] private SubPanelData[] subPanels;

    // 現在開いている設定パネルを記憶しておく変数
    private CanvasGroup _currentSubPanel = null;

    [Header("Option UI Elements")]
    [SerializeField]
    private Slider masterSlider;
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Slider seSlider;

    [FormerlySerializedAs("titleText")]
    [Header("心拍数など")]
    [SerializeField]
    private TextMeshProUGUI Player1HRText;
    [SerializeField]
    private TextMeshProUGUI Player2HRText;
    [SerializeField]
    private TextMeshProUGUI HrConnectLogText;

    private bool _isHRVisible = true; // 最初は表示状態にしておく

    void Start()
    {
        // 登録されているすべてのサブパネルを強制的に非表示にする
        foreach (var data in subPanels)
        {
            if (data.panel != null)
            {
                data.panel.alpha = 0f;
                data.panel.interactable = false;
                data.panel.blocksRaycasts = false;
            }
        }

        // オプションパネルを閉じる
        CloseOption();

        // GameManagerのセーブデータをUIに反映させる
        SyncSettingsToUI();
    }

    /// <summary>
    /// GameManagerのデータをスライダー等のUIに反映させる
    /// </summary>
    private void SyncSettingsToUI()
    {
        // GameManagerが存在しない場合は何もしない（エラー防止）
        if (GameManager.Instance == null) return;

        // GameManagerが保持しているロード済みの設定データを取得
        GameSettingsData settings = GameManager.Instance.currentSettings;

        // スライダーの値をセーブデータと同じにする
        // 値を入れた瞬間にスライダーの OnValueChanged が自動で動き、
        //  AudioManagerに設定が送られてゲームの音量も正しくなる
        if (masterSlider != null) masterSlider.value = settings.masterVolume;
        if (bgmSlider != null) bgmSlider.value = settings.bgmVolume;
        if (seSlider != null) seSlider.value = settings.seVolume;
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 隠しコマンド（Tabキー）で心拍数表示を切り替える
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            _isHRVisible = !_isHRVisible; // trueとfalseを反転
            Player1HRText.enabled = _isHRVisible; // Textコンポーネント自体のON/OFF
            Player2HRText.enabled = _isHRVisible;
        }

        // 2. 心拍数のテキスト更新（表示されている時だけ処理する）
        if (_isHRVisible)
        {
            // 毎フレーム、BioSignalManagerの文字列をそのままUIテキストに入れる
            if (HrConnectLogText != null)
            {
                HrConnectLogText.text = BioSignalManager.Instance.HrConnectLog;
            }

            // BioSignalManagerがスキャン処理を実行中の時
            if (BioSignalManager.Instance.IsScanning)
            {
                Player1HRText.text = "1P: スキャン中";
                Player2HRText.text = "2P: スキャン中";
            }
            // 接続に失敗している、または未接続の時
            else if (!BioSignalManager.Instance.IsConnected)
            {
                Player1HRText.text = "1P: 未接続";
                Player2HRText.text = "2P: 未接続";
            }
            // 接続が成功している時（※1台でも繋がればここに来る）
            else
            {
                // --- 1P の表示 ---
                int p1Hr = BioSignalManager.Instance.RealHeartRates[0];
                if (p1Hr == -1)
                {
                    Player1HRText.text = "1P: 未接続 (電源を確認してください)";
                }
                else if (p1Hr == 0)
                {
                    Player1HRText.text = "1P: 接続済 (心拍データ待機中...)";
                }
                else
                {
                    Player1HRText.text = $"1P: {p1Hr}";
                }

                // --- 2P の表示 ---
                int p2Hr = BioSignalManager.Instance.RealHeartRates[1];
                if (p2Hr == -1)
                {
                    Player2HRText.text = "2P: 未接続 (電源を確認してください)";
                }
                else if (p2Hr == 0)
                {
                    Player2HRText.text = "2P: 接続済 (心拍データ待機中...)";
                }
                else
                {
                    Player2HRText.text = $"2P: {p2Hr}";
                }
            }

            // Rキーによる強制再スキャン（いつでも可能）
            if (!BioSignalManager.Instance.IsScanning && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                // 拡張メソッド Forget() を使って安全に呼び出す
                BioSignalManager.Instance.ForceRescanAsync().AsUniTask().Forget();
            }
        }

        bool isCancelPressed = false;
        // キーボードのキャンセルボタン（ESC）が押されたかのチェック
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isCancelPressed = true;
        }

        // 繋がっているゲームパッドの「B（キャンセル）ボタン」が押されたかチェック
        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            isCancelPressed = true;
        }

        // キャンセルボタンが押されたら戻る処理を行う
        if (isCancelPressed)
        {
            if (_currentSubPanel != null)
            {
                // 設定パネル（サウンドなど）を開いているときは，閉じる
                CloseCurrentSubPanel();
            }
            else
            {
                CloseOption();
            }
        }
    }

    // 2P対戦モードへの遷移
    public void OnClickVersusMode()
    {
        GameSetting.IsSoloMode = false;
        SceneManager.LoadScene("MainScene");
    }

    // ソロモードへの遷移
    public void OnClickSoloMode()
    {
        GameSetting.IsSoloMode = true;
        // SceneManager.LoadScene("MainScene");
    }

    public void OpenOption()
    {
        // メインメニューを隠す
        //mainMenuPanel.alpha = 0; 隠さずにオプションパネルで透かせる
        mainMenuPanel.interactable = false;
        mainMenuPanel.blocksRaycasts = false;
        // オプション画面を表示する
        optionsPanel.alpha = 1;
        optionsPanel.interactable = true;
        optionsPanel.blocksRaycasts = true;
        // オプション画面の最初のボタンを強制的に選択状態にする
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstOptionButton);
    }

    // キャンセルボタンで呼び出す
    public void CloseOption()
    {
        // 念のためサブパネルも閉じておく
        CloseCurrentSubPanel();

        // オプション画面を透明・操作不可にする
        optionsPanel.alpha = 0;
        optionsPanel.interactable = false; // パネルの中にあるUIを操作不能にする
        optionsPanel.blocksRaycasts = false; // パネル全体の当たり判定をなくす

        // メインメニューを表示・操作可能にする
        //mainMenuPanel.alpha = 1;
        mainMenuPanel.interactable = true;
        mainMenuPanel.blocksRaycasts = true;
        // メインメニューに戻ったら、再びメインメニューの最初のボタンを選択状態にする
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuButton);
    }


    /// <summary>
    /// 指定したサブパネルを開き、指定したボタンをフォーカスする
    /// </summary>
    public void OpenSubPanel(string panelName)
    {
        // 配列から一致する名前のデータを検索する
        SubPanelData targetData = System.Array.Find(subPanels, x => x.panelName == panelName);

        if (targetData.panel == null)
        {
            Debug.LogWarning($"指定されたサブパネル '{panelName}' が見つかりません！");
            return;
        }

        CloseCurrentSubPanel();

        _currentSubPanel = targetData.panel;
        _currentSubPanel.alpha = 1;
        _currentSubPanel.interactable = true;
        _currentSubPanel.blocksRaycasts = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(targetData.firstSelectedButton);
    }

    /// <summary>
    /// 現在開いているサブパネルを閉じて、左側のボタン（GameやSound）にフォーカスを戻す
    /// </summary>
    public void CloseCurrentSubPanel()
    {
        if (_currentSubPanel == null) return;

        // パネルを閉じる
        _currentSubPanel.alpha = 0;
        _currentSubPanel.interactable = false;
        _currentSubPanel.blocksRaycasts = false;

        // 記憶をリセット
        _currentSubPanel = null;

        // フォーカスを左側のオプションボタンに戻す
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstOptionButton);

        // オプションを閉じるタイミングで、最後にまとめてJSONファイルに書き込む
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveSettings();
        }
    }

    /// <summary>
    /// UIボタンの OnClick() から呼ぶ用（0: Normal, 1: BioReal, 2: BioFake）
    /// </summary>
    public void OnClickSetBioMode(int modeIndex)
    {
        if (GameManager.Instance != null)
        {
            // int の数字を BioGameMode (enum) に変換して渡す
            BioGameMode selectedMode = (BioGameMode)modeIndex;
            GameManager.Instance.SetBioMode(selectedMode);
            BioSignalManager.Instance.UpdateProvider();

            Debug.Log($"ゲームモードを {selectedMode} に変更しました！");
        }
    }
}