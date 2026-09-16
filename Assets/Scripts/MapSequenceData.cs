using UnityEngine;
using MapEditorSystem.Runtime;

// マップ選択画面に表示するための１マップ分の情報をまとめるクラスを定義
[System.Serializable]
public class MapSequenceStep
{
    [Header("UIに表示するマップ名")]
    public string displayName;

    [Header("UIに表示する画像")]
    public Sprite previewImage;

    [Header("実際に読み込むマップデータ")]
    public MapData mapData;
}

[CreateAssetMenu(fileName = "NewMapSequence", menuName = "TankGame/MapSequence")]
public class MapSequenceData : ScriptableObject
{
    [Header("シーケンス名（実験パターン名）")]
    public string sequenceName = "パターンA (マップA -> B -> C)";

    [Header("この順番でプレイするマップ群")]
    public MapSequenceStep[] sequenceSteps;
}
