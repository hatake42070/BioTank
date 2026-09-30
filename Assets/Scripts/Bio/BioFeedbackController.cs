using System;
using TankControllerScripts;
using UnityEngine;

// タンクにアタッチするコンポーネント
// BioSignalManager から現在の心拍数を受け取り性能を計算してTankControllerやTankDataに渡す
public class BioFeedbackController : MonoBehaviour
{
    private TankController _tankController; // タンクのコントローラー
    private TankData _tankData;

    private void Awake()
    {
        _tankController = GetComponent<TankController>();
        _tankData = _tankController.TankData;
        
        // TankDataの値を直接書き換えると，ゲームプレイ後にも値が残ってしまうので
        // TankDataのスピード倍率変数をControllerに作りそれを操作するようにする
        // もしくは，TankDataの値をコピーしてController側で保持するようにする
    }
}
