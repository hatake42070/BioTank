using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [Header("何秒後に自分自身を削除するか")]
    [SerializeField] private float lifetime = 1f;

    private void Start()
    {
        // Startが呼ばれた瞬間からカウントダウンを開始し、
        // 指定した lifetime 秒後に自分自身（gameObject）を破壊する
        Destroy(gameObject, lifetime);
    }
}