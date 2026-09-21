using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UI Panels")] 
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup optionsPanel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            if (optionsPanel.alpha > 0f)
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
        mainMenuPanel.alpha = 0;
        mainMenuPanel.interactable = false;
        mainMenuPanel.blocksRaycasts = false;
        
        optionsPanel.alpha = 1;
        optionsPanel.interactable = true;
        optionsPanel.blocksRaycasts = true;
    }

    // キャンセルボタンで呼び出す
    public void CloseOption()
    {
        // オプション画面を透明・操作不可にする
        optionsPanel.alpha = 0;
        optionsPanel.interactable = false; // パネルの中にあるUIを操作不能にする
        optionsPanel.blocksRaycasts = false; // パネル全体の当たり判定をなくす
        
        // メインメニューを表示・操作可能にする
        mainMenuPanel.alpha = 1;
        mainMenuPanel.interactable = true;
        mainMenuPanel.blocksRaycasts = true;
    }
}
