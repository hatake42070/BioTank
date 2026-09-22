using UnityEngine;
using UnityEngine.UI; // ImageやSliderを操作するために必要

public class MuteToggleUI : MonoBehaviour
{
    [Header("UI設定")]
    [SerializeField] private Image targetImage; // 画像を切り替えるImageコンポーネント
    [SerializeField] private Sprite muteOffSprite; // 通常時（音が出る）の画像
    [SerializeField] private Sprite muteOnSprite;  // ミュート中（×マークなど）の画像
    
    [Header("音量復帰用")]
    [SerializeField] private Slider masterSlider; // ミュート解除時に元の音量に戻すためのスライダー

    private bool _isMuted = false; // 現在ミュート中かどうかを記憶する変数（最初はfalse）

    // ボタンの OnClick() からこのメソッドを1つだけ呼び出す
    public void ToggleMute()
    {
        _isMuted = !_isMuted; // trueとfalseを反転させる（スイッチ機能）

        if (_isMuted)
        {
            // ミュートにする時の処理
            targetImage.sprite = muteOnSprite; // 画像をON（ミュート状態）に差し替え
            
            // AudioManager経由で音量を最小（ほぼ0）にする
            // ※スライダーの仕様に合わせて 0.0001f か 1f（100段階の場合） を渡してください
            if (AudioManager.Instance != null)
                AudioManager.Instance.SetMasterVolume(0.0001f); 
        }
        else
        {
            // ミュート解除する時の処理
            targetImage.sprite = muteOffSprite; // 画像を通常状態に戻す
            
            // スライダーの「現在の値」を取得して、元の音量に戻す！
            if (AudioManager.Instance != null && masterSlider != null)
                AudioManager.Instance.SetMasterVolume(masterSlider.value);
        }
    }
}