using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicalInput
{
    const int KEY_REPEAT_START_TIME = 12;//押しっぱなしでキーリピートに入るフレーム数
    const int KEY_REPEAT_ITERATION_TIME = 1;//キーリピートに入った後の更新フレーム数
    [Flags]
    public enum Key
    {
        Right = 1 << 0,
        Left = 1 << 1,
        RotR = 1 << 2,
        RotL = 1 << 3,
        QuickDrop = 1 << 4,
        Down = 1 << 5,//将来的に下に素早く移動できるように「下ボタン」も定義

        MAX = 6, //個数
        //「[flags]」をつけて、enum を宣言
        //テストするコードが書きやすくなるように、最大値「MAX」も定義
    }

    Key inputRaw;//現在の値
    Key inputTrg;//入力が入った時の値
    Key inputRel;//入力が抜けたときの値
    Key inputRep;//連続入力
    int[] _trgWaitingTime = new int[(int)Key.MAX];//また、キーリピート情報を生成するために、残り無効時間を保持するための整数をボタンごとに保持します

   public void Clear()//Clear: 初期化 関係する全ての値を0クリアする
    {
        inputRaw = 0;
        inputTrg = 0;
        inputRel = 0;
        inputRep = 0;
        for(int i=0;i< (int)Key.MAX;i++)
        {
            _trgWaitingTime[i] = 0;
        }
    }

    public void Update(Key inputDev) //Update: デバイスからの入力を受けての内部状態の更新
    {
        //入力が入った/抜けた
        inputTrg = (inputDev ^ inputRaw) & inputDev; //押された瞬間、離された瞬間の検出  xor 演算で前のボタンの状態と現在のボタンの状態を比較して異なる値になっているボタンを検出した後、ボタンの「現在」の状態でorを取って、「押した瞬間」を判定する
        inputRel = (inputDev ^ inputRaw) & inputRaw;

        //生データの生成
        inputRaw = inputDev; //現在のデバイスの状態を更新 押された瞬間の導出に必要なので、押された瞬間を計算した後で上書きする

        //キーリピートの生成
        inputRep = 0;
        for (int i = 0; i < (int)Key.MAX; i++)
        {
            if(inputTrg.HasFlag((Key)(1<<i)))
            {
                inputRep |= (Key)(1 << i);
                _trgWaitingTime[i] = KEY_REPEAT_START_TIME; //押された瞬間の後は押しっぱなしでもONにならない時間を設定 
            }
            //押された瞬間はON
            else if (inputTrg.HasFlag((Key)(1 << i))) //押された瞬間でなくて押しっぱなしなら、ボタンに応じたカウンタを減らす 
            {
                if (--_trgWaitingTime[i] <= 0)//カウンタが0になったら、リピートをONにする 
                {
                    inputRep |= (Key)(1 << i);
                    _trgWaitingTime[i] = KEY_REPEAT_START_TIME; //2度目以降はONになるまでの時間は短くする
                }
            }
        } //今回は、左右移動の為のキーリピート時間なので２度目以降は１フレでONにしていますが、メニューでのカーソル移動等はもっと長い時間の方が自然な動きになるかもしれません
    }
    //メンバー変数としては、入力の値を保持する変数を用意します。用意する変数は次の通りです
    public bool IsRaw(Key k)
    {
        return inputRaw.HasFlag(k);//inputRaw: ゲームロジック的なON/OFFを保持する
    }

    public bool IsTrigger(Key k)
    {
        return inputTrg.HasFlag(k);//inputTrg: 各ボタンが押された瞬間だけON
    }

    public bool IsRelease(Key k)
    {
        return inputRel.HasFlag(k);//inputRel: 各ボタンが離された瞬間だけON
    }

    public bool IsRepeat(Key k)
    {
        return inputRep.HasFlag(k);//inputRep: キーリピート的なON/OFFを保持する
    }

    //なお、外部からこれらの変数にアクセスするには、public公開された「Is***」メソッドを通して、各フラグが立っているかどうかを確認します。


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
