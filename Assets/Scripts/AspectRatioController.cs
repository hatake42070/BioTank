using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    // ターゲットとする縦横比（1920x1080 ＝ 16:9 ＝ 1.777...）
    [SerializeField] private float targetAspect = 16.0f / 9.0f;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        // 現在のウィンドウの縦横比を計算
        float currentWindowAspect = (float)Screen.width / (float)Screen.height;
        // 目標とする縦横比とのズレ（倍率）を計算
        float scaleHeight = currentWindowAspect / targetAspect;

        // ウィンドウが横長すぎる場合（左右に黒帯を入れる）
        if (scaleHeight < 1.0f)
        {
            Rect rect = _camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            _camera.rect = rect;
        }
        // ウィンドウが縦長すぎる場合（上下に黒帯を入れる）
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = _camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            _camera.rect = rect;
        }
    }
}