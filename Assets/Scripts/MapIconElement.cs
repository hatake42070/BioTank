using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 1つのマップアイコンの見た目を管理する専用クラス
public class MapIconElement : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;

    // マネージャーからデータを受け取って見た目をセットする
    public void Setup(Sprite sprite, string mapName)
    {
        if (iconImage != null) iconImage.sprite = sprite;
        if (nameText != null) nameText.text = mapName;
    }
}