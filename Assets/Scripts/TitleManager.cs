using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

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
        public string panelName;              // 管理しやすいように名前（例: "Sound", "Game"）
        public CanvasGroup panel;             // 対象のパネル
        public GameObject firstSelectedButton; // 最初に選択させたいボタン
    }
    
    [Header("Sub Panels")]
    [SerializeField] private SubPanelData[] subPanels;
    
    // 現在開いている設定パネルを記憶しておく変数
    private CanvasGroup _currentSubPanel = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
    }

    // Update is called once per frame
    void Update()
    {
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
    }
}