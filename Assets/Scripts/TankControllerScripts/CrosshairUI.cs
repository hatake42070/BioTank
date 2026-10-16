using UnityEngine;
using UnityEngine.UI;

namespace TankControllerScripts
{
    public class CrosshairUI : MonoBehaviour
    {
        [Header("UIコンポーネント")]
        [SerializeField]
        private Image crosshairImage; // 実際の画像を描画するコンポーネント

        [SerializeField]
        private RectTransform crosshairRect; // UIを移動させるためのTransform

        [Header("プレイヤーごとのクロスヘア")]
        [SerializeField]
        private Sprite playerCrosshairSprites;

        // クロスヘアの色
        [SerializeField]
        private Color crosshairColor1P = new Color(0.2f, 0.6f, 1f, 1.0f); // 1P: 少し薄い青

        [SerializeField]
        private Color crosshairColor2P = new Color(1f, 0.3f, 0.3f, 1.0f); // 2P: 少し薄い赤

        [Header("点線エフェクト")]
        [SerializeField]
        private GameObject dotPrefab; // 丸い点(UIのImage)のプレハブ

        [SerializeField]
        private int dotCount = 3; // 点の数

        [Header("点線の色と濃度（アルファ値）")]
        [SerializeField]
        private Color color1P = new Color(0.2f, 0.6f, 1f, 0.5f); // 1P: 少し薄い青
        [SerializeField]
        private Color color2P = new Color(1f, 0.3f, 0.3f, 0.5f); // 2P: 少し薄い赤

        // クロスヘアの固定半径（ピクセル単位）
        [Header("クロスヘアの動作設定")]
        [SerializeField] private float fixedRadius = 150f;

        private TankInputHandler _myInputHandler;
        private bool _isInitialized = false;
        private GameObject[] _dots;
        private Camera _mainCamera;

        // 親のCanvasを記憶しておく変数
        private Canvas _parentCanvas;

        public void Initialize(TankInputHandler inputHandler, int playerIndex)
        {
            Color crosshairColor = (playerIndex == 0) ? crosshairColor1P : crosshairColor2P;
            _myInputHandler = inputHandler;
            _mainCamera = Camera.main;

            // 自分が所属しているCanvas（GameUIなど）を取得して記憶しておく
            if (transform.parent != null)
            {
                _parentCanvas = transform.parent.GetComponentInParent<Canvas>();
            }

            if (playerCrosshairSprites != null && crosshairImage != null)
            {
                crosshairImage.sprite = playerCrosshairSprites;
                crosshairImage.color = crosshairColor;
            }

            Color dotColor = (playerIndex == 0) ? color1P : color2P;
            _dots = new GameObject[dotCount];
            for (int i = 0; i < dotCount; i++)
            {
                _dots[i] = Instantiate(dotPrefab, transform.parent);

                // 点線のサイズや位置がバグらないようにスケールをリセットしておく
                _dots[i].GetComponent<RectTransform>().localScale = Vector3.one * 2.4f;

                Image dotImage = _dots[i].GetComponent<Image>();
                if (dotImage != null)
                {
                    dotImage.color = dotColor;
                }
            }

            _isInitialized = true;
        }

        // UpdateではなくLateUpdateに変更（チラつき防止）
        private void LateUpdate()
        {
            if (!_isInitialized) return;

            // ターゲットやカメラ、Canvasが存在しない場合は処理しない
            if (_myInputHandler == null || _mainCamera == null || _parentCanvas == null)
            {
                Destroy(gameObject);
                return;
            }

            if (crosshairRect == null) return;

            // 1. タンクの画面座標(ピクセル)を取得（Vector2ではなくVector3で受けるのがコツ）
            Vector3 tankScreenPos = _mainCamera.WorldToScreenPoint(_myInputHandler.transform.position);

            // 2. タンクの座標 ＋ (入力の方向 × 固定半径) でクロスヘアの画面座標を決定
            Vector3 crosshairScreenPos = tankScreenPos;
            crosshairScreenPos.x += _myInputHandler.AimDirection.x * fixedRadius;
            crosshairScreenPos.y += _myInputHandler.AimDirection.y * fixedRadius;

            // 3. Z軸（奥行き）を、Canvasの「カメラからの距離」に強制的に合わせる
            tankScreenPos.z = _parentCanvas.planeDistance;
            crosshairScreenPos.z = _parentCanvas.planeDistance;

            // 4. ピクセル座標を、Canvas上の正しいワールド座標に戻して直接代入する
            crosshairRect.position = _mainCamera.ScreenToWorldPoint(crosshairScreenPos);

            // 5. 点線の配置計算（点線も同じく安全な方法で座標変換する）
            if (_dots != null)
            {
                for (int i = 0; i < dotCount; i++)
                {
                    float t = (float)(i + 1) / (dotCount + 1);
                    // 画面上のピクセル座標空間で、戦車とクロスヘアの間の位置（Lerp）を計算
                    Vector3 dotScreenPos = Vector3.Lerp(tankScreenPos, crosshairScreenPos, t);

                    // ワールド座標に変換して代入
                    _dots[i].transform.position = _mainCamera.ScreenToWorldPoint(dotScreenPos);
                }
            }
        }

        private void OnDestroy()
        {
            if (_dots != null)
            {
                foreach (var dot in _dots)
                {
                    if (dot != null) Destroy(dot);
                }
            }
        }
    }
}