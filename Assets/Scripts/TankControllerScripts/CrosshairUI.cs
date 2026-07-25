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
        public void Initialize(TankInputHandler inputHandler, int playerIndex)
        {
            _myInputHandler = inputHandler;
            if(playerCrosshairSprites != null && playerCrosshairSprites.Length > 0 && crosshairImage != null)
            {
                crosshairImage.sprite = playerCrosshairSprites[playerIndex % playerCrosshairSprites.Length];
            }
        }

        private void Update()
        {
            if (_myInputHandler == null || crosshairRect == null) return;
            
            crosshairRect.position = _myInputHandler.PointerScreenPosition;
        }
    }
}