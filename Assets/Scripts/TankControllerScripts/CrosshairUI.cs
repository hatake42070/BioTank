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
        private bool _isInitialized = false; // 初期化完了フラグを追加
        private GameObject[] _dots; // 生成した点を保存する配列
        private Camera _mainCamera;

        public void Initialize(TankInputHandler inputHandler, int playerIndex)
        {
            Color crosshairColor = (playerIndex == 0) ? crosshairColor1P : crosshairColor2P;
            _myInputHandler = inputHandler;
            _mainCamera = Camera.main; // カメラの参照をキャッシュ（毎フレーム探すと重いため）
            if (playerCrosshairSprites != null && crosshairImage != null)
            {
                crosshairImage.sprite = playerCrosshairSprites;
                crosshairImage.color = crosshairColor;
            }

            // 点のUIを生成
            Color dotColor = (playerIndex == 0) ? color1P : color2P;
            _dots = new GameObject[dotCount];
            for (int i = 0; i < dotCount; i++)
            {
                // クロスヘアと同じ親(Canvasなど)の中に点を生成
                _dots[i] = Instantiate(dotPrefab, transform.parent);

                // 生成した点の「Image」コンポーネントを取得して、色を適用する
                Image dotImage = _dots[i].GetComponent<Image>();
                if (dotImage != null)
                {
                    dotImage.color = dotColor;
                }
            }

            _isInitialized = true; // 初期化完了！
        }

        private void Update()
        {
            // まだ初期化されていなければ、何もしない（Initializeされる前にUpdate()での自爆防止）
            if (!_isInitialized) return;

            // 追従すべき対象（タンク）が破壊されてnullになっていたら...
            if (_myInputHandler == null)
            {
                // 自分自身（クロスヘアUI）も消滅させる
                Destroy(gameObject);
                return;
            }

            if (crosshairRect == null) return;
            
            // 1. タンクの画面座標を取得
            Vector2 tankScreenPos = _mainCamera.WorldToScreenPoint(_myInputHandler.transform.position);

            // 2. タンクの座標 ＋ (入力の方向 × 固定半径) でクロスヘアの位置を決定！
            Vector2 crosshairPos = tankScreenPos + _myInputHandler.AimDirection * fixedRadius;

            // クロスヘアUIを移動
            crosshairRect.position = crosshairPos;

            // 3. 点線の配置計算
            if (_dots != null)
            {
                for (int i = 0; i < dotCount; i++)
                {
                    float t = (float)(i + 1) / (dotCount + 1);
                    _dots[i].transform.position = Vector2.Lerp(tankScreenPos, crosshairPos, t);
                }
            }
        }

        // クロスヘアが消滅する時（タンク破壊時など）に、生成した点も一緒に削除する
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