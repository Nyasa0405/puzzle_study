using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    
    int _time = 0;//時間管理するための変数を用意します。 残り時間のメンバー「_time」を整数化
    float _inv_time_max = 1.0f;//元の正規化時間を保存しておきます。ただし、CPUは割り算より掛け算の方が早い傾向にあるので、掛け算で処理できるように、最初に逆数にして保存します。

    public void Set(int max_time) //アニメーションする時間を設定する「Set」メソッドを追加 
    {
        Debug.Assert(0 < max_time);//負の遷移時間は不正 遷移時間を設定する「Set」メソッドの引数を整数に 
        //念のため、負の数が入ってこないかチェックする

        _time = max_time;
        _inv_time_max = 1.0f / max_time;//遷移時間設定時に「_inv_time_max」メンバーを初期化 浮動小数点数として処理する場所ではキャストを追加
    }

    //アニメーション中ならtrueを返す
    public bool Update() //時間を更新する「Update」メソッドを追加 「Update」メソッドの引数はなくす
    {
        _time = Math.Max(--_time, 0);//内部では、「_time」を一つずつ減らして0未満にならないようにMaxメソッドでクランプする
        //一つずつ減らすので、減らす値の上限の確認は削除
        return (0 < _time);
    }

    public float GetNormalized() //正規化時間を取得する「GetNormalized」メソッドを追加
    {
        return _time * _inv_time_max; //残り時間と、遷移時間の逆数をかけて正規化時間とします
    }
}

