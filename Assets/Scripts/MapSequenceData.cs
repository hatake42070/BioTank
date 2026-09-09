using UnityEngine;
using MapEditorSystem.Runtime;

[CreateAssetMenu(fileName = "NewMapSequence", menuName = "TankGame/MapSequence")]
public class MapSequenceData : ScriptableObject
{
    [Header("シーケンス名（実験パターン名）")]
    public string sequenceName = "パターンA (マップA -> B -> C)";

    [Header("この順番でプレイするマップ群")]
    public MapData[] sequenceMaps;
}
