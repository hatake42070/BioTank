using UnityEngine;
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

    // オプションパネルの中の閉じるボタンを押された時に呼ばれる
    public void CloseOption()
    {
        // オプション画面を透明・操作不可にする
        optionsPanel.alpha = 0;
        optionsPanel.interactable = false; // パネルの中にあるUIを操作不能にする
        optionsPanel.blocksRaycasts = false; // パネル全体の当たり判定をなくす
        
        // メインメニューを表示・操作可能にする
        mainMenuPanel.alpha = 1;
        mainMenuPanel.interactable = true;
        optionsPanel.blocksRaycasts = true;
    }
}
