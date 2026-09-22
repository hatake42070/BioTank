using UnityEngine;
using UnityEngine.EventSystems; // UIの選択状態を検知するために必要
using UnityEngine.UI;           // Outlineを使うために必要

public class FocusOutlineUpdater : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("光らせたい背景のOutline")]
    [SerializeField] private Outline targetOutline;

    private void Start()
    {
        // ゲーム開始時は確実にオフにしておく
        if (targetOutline != null)
        {
            targetOutline.enabled = false;
        }
    }

    // このUI（スライダー）が十字キーなどで選択された瞬間に呼ばれる
    public void OnSelect(BaseEventData eventData)
    {
        if (targetOutline != null)
        {
            targetOutline.enabled = true; // 輪郭を表示
        }
    }

    // 他のUIに移動して、選択が外れた瞬間に呼ばれる
    public void OnDeselect(BaseEventData eventData)
    {
        if (targetOutline != null)
        {
            targetOutline.enabled = false; // 輪郭を隠す
        }
    }
}