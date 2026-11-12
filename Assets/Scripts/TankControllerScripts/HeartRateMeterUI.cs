using TMPro;
using UnityEngine;

public class HeartRateMeterUI : MonoBehaviour
{
    [SerializeField] private BioFeedbackController source;
    [SerializeField] private RectTransform fill;
    [SerializeField] private TMP_Text statusText;

    [SerializeField] private float offsetBelowHearts = 28f;

    public void Bind(BioFeedbackController controller)
    {
        source = controller;

        var rect = (RectTransform)transform;
        var center = new Vector2(0.5f, 0.5f);

        rect.anchorMin = center;
        rect.anchorMax = center;
        rect.pivot = center;
        rect.anchoredPosition = new Vector2(0f, -offsetBelowHearts);
    }

    private void LateUpdate()
    {
        bool hasReading = source != null
            && source.CurrentBpm > 0
            && source.CurrentBaseline > 0f;

        fill.gameObject.SetActive(hasReading);

        if (!hasReading)
        {
            statusText.text = "心拍データ待ち";
            return;
        }

        float diff = source.CurrentBpm - source.CurrentBaseline;
        float position = Mathf.InverseLerp(-15f, 15f, diff);

        // 帯の中央から、現在の心拍差に対応する位置まで色を伸ばす。
        const float baselinePosition = 0.5f;
        fill.anchorMin = new Vector2(Mathf.Min(baselinePosition, position), 0f);
        fill.anchorMax = new Vector2(Mathf.Max(baselinePosition, position), 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;

        statusText.text = $"反射 {source.NextBulletBounces}回";
    }
}