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

    // UIを描画している大元のCanvasを記憶する変数
    private Canvas _parentCanvas;

    public void Initialize(Transform tankTransform, int maxHp)
    {
        _targetTank = tankTransform;
        _mainCamera = Camera.main;

        // 自分が所属しているCanvas（GameUIなど）を取得して記憶しておく
        if (transform.parent != null)
        {
            _parentCanvas = transform.parent.GetComponentInParent<Canvas>();
        }

        for (int i = 0; i < maxHp; i++)
        {
            GameObject segmentObj = Instantiate(hpSegmentPrefab, hpContainer);
            Image redHeartImage = segmentObj.transform.GetChild(0).GetComponent<Image>();
            _hpSegments.Add(redHeartImage);
        }
    }

    public void UpdateHpDisplay(int currentHp)
    {
        for (int i = 0; i < _hpSegments.Count; i++)
        {
            if (i < currentHp) continue;
            else _hpSegments[i].enabled = false;
        }
    }

    // Updateではなく「LateUpdate」を使う
    // （戦車の移動が完全に終わった"後"にUIを動かすため、チラつきが消滅します）
    private void LateUpdate()
    {
        // ターゲットやカメラ、Canvasが存在しない場合は処理しない
        if (_targetTank == null || _mainCamera == null || _parentCanvas == null)
        {
            Destroy(gameObject);
            return;
        }

        // 1. 戦車の3D空間の座標を、画面のピクセル座標（2D）に変換
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetTank.position);

        // 2. オフセット（ズレ）をピクセル単位で足す
        screenPos.x += offset.x;
        screenPos.y += offset.y;

        // 3. 【最重要】Z軸（奥行き）を、Canvasが設定している「カメラからの距離(Plane Distance)」に強制的に合わせる
        screenPos.z = _parentCanvas.planeDistance;

        // 4. ピクセル座標を、Canvas上の正しいワールド座標に戻して直接代入する
        transform.position = _mainCamera.ScreenToWorldPoint(screenPos);
    }
}