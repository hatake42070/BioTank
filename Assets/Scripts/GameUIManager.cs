using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    // どこからでもアクセスできるインスタンス
    public static GameUIManager Instance { get; private set; }

    // 自分のCanvas
    public Transform CanvasTransform { get; private set; }
    
    [Header("リザルトUI")]
    [SerializeField] private GameObject resultUIPanel;      // 背景パネルなど
    [SerializeField] private TextMeshProUGUI resultText;    // 勝敗テキスト

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CanvasTransform = this.transform; // 自分がついているオブジェクト(GameUI)のTransformを記憶
            
            if (resultUIPanel != null)
            {
                resultUIPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
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