using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankHpUI : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private GameObject hpSegmentPrefab; // 1目盛り分のImageプレハブ
    [SerializeField] private RectTransform hpContainer;  // HorizontalLayoutGroupがついた自分自身
    
    [Header("表示設定")]
    [SerializeField] private Vector2 offset = new Vector2(0, -60); // タンクから下にどれくらいズラすか

    private List<Image> _hpSegments = new List<Image>();
    private Transform _targetTank;
    private Camera _mainCamera;

    /// <summary>
    /// 戦車生成時に自動で呼ばれる初期化処理
    /// </summary>
    public void Initialize(Transform tankTransform, int maxHp)
    {
        _targetTank = tankTransform;
        _mainCamera = Camera.main;
        
        // 最大HPの数だけメモリを生成して並べる
        for (int i = 0; i < maxHp; i++)
        {
            GameObject segmentObj = Instantiate(hpSegmentPrefab, hpContainer);
            Image segmentImage = segmentObj.GetComponent<Image>();
            _hpSegments.Add(segmentImage);
        }
    }
    
    /// <summary>
    /// ダメージを受けた時に呼ばれる処理
    /// </summary>
    public void UpdateHpDisplay(int currentHp)
    {
        for (int i = 0; i < _hpSegments.Count; i++)
        {
            if (i < currentHp)
            {
                continue; // 表示を維持する場合は何もしない
            }
            else
            {
                _hpSegments[i].enabled = false; // 非表示にする場合
            }
        }
    }
    
    private void Update()
    {
        // タンクが破壊されたらこのHPバーも一緒に消去
        if (_targetTank == null || _mainCamera == null)
        {
            Destroy(gameObject);
            return;
        }

        // タンクの3D座標を2Dスクリーン座標に変換し、少し下(offset)に配置
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(_targetTank.position);
        transform.position = screenPos + offset;
    }
}
