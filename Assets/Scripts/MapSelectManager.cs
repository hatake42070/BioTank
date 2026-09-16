using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectManager : MonoBehaviour
{
    private MapSequenceData[] mapSequences; // シーケンスの配列
    
    [Header("Panel References")]
    [SerializeField] private CanvasGroup mapSelectPanelGroup;

    [Header("UI References")]
    [SerializeField]
    private TextMeshProUGUI sequenceNameText; // 「チュートリアル」や「パターンA」などの大見出し
    [SerializeField]
    private TextMeshProUGUI statusText;       // 「マップを選択」などの案内
    
    [Header("Dynamic UI")]
    [SerializeField]
    private GameObject mapIconPrefab; // UIのパーツプレハブ（アイコン用）
    [SerializeField]
    private RectTransform iconContainer; // 並べるための箱
    [SerializeField]
    private GridLayoutGroup gridLayout;
    
    [Header("Size Settings")]
    [SerializeField]
    private float maxIconWidth = 400f; // アイコンの最大の横幅（これ以上は大きくならない）
    
    private bool _isConfirming = false; // ゲームスタートの確認中かどうか
    
    public static MapSelectManager Instance;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    
    // 最初に画面が読み込まれた時は非表示にしておく
    private void Start()
    {
        ClosePanel();
    }
    
    // パネルを表示するメソッド（GameManagerから呼ばれる）
    public void OpenPanel()
    {
        mapSelectPanelGroup.alpha = 1f;
        mapSelectPanelGroup.interactable = true;
        mapSelectPanelGroup.blocksRaycasts = true;
        
        // パネルを開くついでにUIの初期化
        ToggleConfirmUI(false);
        mapSequences = GameManager.Instance.experimentSequences;
        UpdateUI();
    }

    // パネルを非表示にするメソッド（GameManagerから呼ばれる）
    public void ClosePanel()
    {
        mapSelectPanelGroup.alpha = 0f;
        mapSelectPanelGroup.interactable = false;
        mapSelectPanelGroup.blocksRaycasts = false;
    }

    public void UpdateUI()
    {
        int currentIndex = GameManager.Instance.SelectedSequenceIndex;
        
        MapSequenceData currentData = mapSequences[currentIndex];

        // 大見出し（シーケンス名）とステータスの更新
        if (sequenceNameText != null)
        {
            sequenceNameText.text = currentData.sequenceName;
        }

        // 一旦、コンテナの中にある古いマップアイコンを全て削除する
        foreach (Transform child in iconContainer)
        {
            Destroy(child.gameObject);
        }

        // 今回のシーケンスに含まれるマップの数だけ、プレハブを生成して並べる
        foreach (MapSequenceStep step in currentData.sequenceSteps)
        {
            // コンテナの子オブジェクトとしてプレハブを生成
            GameObject iconObj = Instantiate(mapIconPrefab, iconContainer);
            
            // プレハブについているスクリプトを取得して、画像と名前を渡す
            MapIconElement iconElement = iconObj.GetComponent<MapIconElement>();
            if (iconElement != null)
            {
                iconElement.Setup(step.previewImage, step.displayName);
            }
        }
        
        // マップを並べ終わったら、数に合わせてサイズを自動調整
        AdjustIconSize(currentData.sequenceSteps.Length);
    }
    
    // UIの切り替え用メソッド（GameManagerから呼ばれる想定）
    public void ToggleConfirmUI(bool isConfirming)
    {
        _isConfirming = isConfirming;
        statusText.text = _isConfirming ? "ゲームスタート\n（もう一度押して開始）" : "マップを選択";
    }
    
    // マップの数に合わせてアイコンのサイズを自動調整するメソッド
    private void AdjustIconSize(int mapCount)
    {
        if (mapCount == 0 || gridLayout == null) return;

        // アイコンを並べる箱（コンテナ）の実際の横幅を取得
        float containerWidth = iconContainer.rect.width;

        // 左右の余白(Padding)と、アイコン同士の隙間(Spacing)の合計を計算
        float padding = gridLayout.padding.left + gridLayout.padding.right;
        float totalSpacing = gridLayout.spacing.x * (mapCount - 1);

        // アイコン表示に使える純粋な横幅
        float availableWidth = containerWidth - padding - totalSpacing;

        // 1個あたりの横幅を割り出す
        float calculatedWidth = availableWidth / mapCount;

        // 大きくなりすぎないように、最大幅（maxIconWidth）で制限をかける
        float finalWidth = Mathf.Min(calculatedWidth, maxIconWidth);

        // 縦横比を計算（例: 16:9の画像なら、高さ = 幅 * 9 / 16）
        // ※用意したマップ画像の縦横比に合わせて「9f / 16f」の部分は調整する
        float finalHeight = finalWidth * (9f / 16f); 

        // 計算したサイズを Grid Layout Group に適用！
        gridLayout.cellSize = new Vector2(finalWidth, finalHeight);
    }
}