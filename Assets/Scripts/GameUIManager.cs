using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    // どこからでもアクセスできるインスタンス
    public static GameUIManager Instance { get; private set; }

    // 自分のCanvas
    public Transform CanvasTransform { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            CanvasTransform = this.transform; // 自分がついているオブジェクト(GameUI)のTransformを記憶
        }
        else
        {
            Destroy(gameObject);
        }
    }
}