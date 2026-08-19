using UnityEngine;
using UnityEngine.UI;

namespace TankControllerScripts
{
    public class CrosshairUI : MonoBehaviour
    {
        [Header("UIコンポーネント")]
        [SerializeField] private Image crosshairImage; // 実際の画像を描画するコンポーネント
        [SerializeField] private RectTransform crosshairRect; // UIを移動させるためのTransform
        
        [Header("プレイヤーごとのクロスヘア")]
        [SerializeField]
        private Sprite[] playerCrosshairSprites; // 1P青, 2P赤
        
        private TankInputHandler _myInputHandler;
        private bool _isInitialized = false; // 初期化完了フラグを追加
        public void Initialize(TankInputHandler inputHandler, int playerIndex)
        {
            _myInputHandler = inputHandler;
            if(playerCrosshairSprites != null && playerCrosshairSprites.Length > 0 && crosshairImage != null)
            {
                crosshairImage.sprite = playerCrosshairSprites[playerIndex % playerCrosshairSprites.Length];
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
            
            // タンクの照準位置にクロスヘアUIを移動させる
            crosshairRect.position = _myInputHandler.PointerScreenPosition;
        }
    }
}