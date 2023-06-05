using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController 
{
    const float DELTA_TIME_MAX = 1.0f;//更新時間の上限の値の設定
    float _time = 0.0f;//時間管理するための変数を用意します。
    float _inv_time_max = 1.0f;//元の正規化時間を保存しておきます。ただし、CPUは割り算より掛け算の方が早い傾向にあるので、掛け算で処理できるように、最初に逆数にして保存します。

    public void Set(float max_time) //アニメーションする時間を設定する「Set」メソッドを追加 
    {
        Debug.Assert(0.0f < max_time);//負の遷移時間は不正
        //念のため、負の数が入ってこないかチェックする

        _time = max_time;
        _inv_time_max = 1.0f / max_time;//遷移時間設定時に「_inv_time_max」メンバーを初期化
    }

    //アニメーション中ならtrueを返す
    public bool Update(float delta_time) //時間を更新する「Update」メソッドを追加
    {
        //前に呼ばれた時からの更新時間（delta_time）を受け取る

        //あまり時間が立った結果は怪しいので、更新時間の上限を導入する
        if (DELTA_TIME_MAX<delta_time)delta_time = DELTA_TIME_MAX; //更新時間を上限で抑える 

        _time -= delta_time;
        //delta_time だけ時間を減らす
        //0になったら終了
        if (_time <= 0.0f)
        {
            _time = 0.0f;//負の数にしない
            //今後、何度も呼ばれても大丈夫なように負になったら0にしておく
            return false;
        }
        return true;
    }

    public float GetNormalized() //正規化時間を取得する「GetNormalized」メソッドを追加
    {
        return _time * _inv_time_max; //残り時間と、遷移時間の逆数をかけて正規化時間とします
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
