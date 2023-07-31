using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    const int TRANS_TIME = 3;//移動速度遷移時間
    const int ROT_TIME = 3;//回転遷移時間

    // 落下制御
    const int FALL_COUNT_UNIT = 120; // ひとマス落下するカウント数
    const int FALL_COUNT_SPD = 10; // 落下速度
    const int FALL_COUNT_FAST_SPD = 20; // 高速落下時の速度
    const int GROUND_FRAMES = 50; // 接地移動可能時間
    //定数として設定します
    //平行移動と回転の移動速度を変えられるようにしておきました
    enum RotState
    {
        Up=0,
        Right=1,
        Down=2,
        Left=3,

        Invalid=-1,
    }
    //「状態」を導入して管理するのが定番。今回は、「RotState」という回転の状態を導入

    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };//ぷよのスクリプトを配列で持つ。　Default!を初期値として設定し設定もれを防ぐ
    [SerializeField] BoardController boardController = default!;//「BoardController」はあらかじめ設定しておくように「[SerializeField]」をつけてメンバー変数として用意しておきます
    LogicalInput _logicalInput = null;

    Vector2Int _position = new Vector2Int(2, 12);// 軸ぷよの位置
    RotState _rotate = RotState.Up;//角度は 0:上 1:右 2:下 3;左 で持つ（子ぷよの位置）

    AnimationController _animationController=new AnimationController(); //AnimationController の追加
    Vector2Int _last_position;
    RotState _last_rotate = RotState.Up;
    //遷移前の位置「_last_position」、向き「_last_rot」の保存

    // 落下制御
    int _fallCount = 0;
    int _groundFrame = GROUND_FRAMES;// 接地時間

    // 得点
    uint _additiveScore = 0;

    // Start is called before the first frame update

    void Start()
    {
        gameObject.SetActive(false);// ぷよの種類が設定されるまで眠る
    }

    public void SetLogicalInput(LogicalInput reference)
    {
        _logicalInput = reference;
    }
    public bool Spawn(PuyoType axis, PuyoType child)
    {
        // 初期位置に出せるか確認
        Vector2Int position = new(2, 12);// 初期位置
        RotState rotate = RotState.Up;// 最初は上向き
        if (!CanMove(position, rotate)) return false;

        // パラメータの初期化
        _position = _last_position = position;
        _rotate = _last_rotate = rotate;
        _animationController.Set(1);
        _fallCount = 0;
        _groundFrame = GROUND_FRAMES;

        // ぷよをだす
        _puyoControllers[0].SetPuyoType(axis);
        _puyoControllers[1].SetPuyoType(child);

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//ぷよぷよが登場する標準的な場所の位置
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));//表示するゲームオブジェクトの位置を設定

        gameObject.SetActive(true);

        return true;
    }

    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;
        //CanMove」メソッドでの子ゲームオブジェクトの位置の検証の修正

        return true;
    }

    void SetTransition(Vector2Int pos, RotState rot, int time) //遷移時間のを設定するメソッド「SetTransition」の追加
    {
        //補間のため保存しておく
        _last_position=_position;
        _last_rotate=_rotate;//呼ばれる前の位置と向きを保存しておく

        //値の更新
        _position = pos;
        _rotate = rot;//位置や向きを更新

        _animationController.Set(time);//AnimationController に時間を指定
    }
    private bool Translate(bool is_right)
    {
        //仮想的に移動できるか検証する
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;

        //実際に移動
        SetTransition(pos, _rotate, TRANS_TIME);//平行移動の目的値の設定 

        return true;
    }

    bool Rotate (bool is_right)
    {
        RotState rot = (RotState)(((int)_rotate + (is_right ? +1 : +3)) & 3);

        //仮想的に移動できるか検証する（上下左右にずらした時も確認）
        Vector2Int pos = _position;

        switch (rot)
        {
            case RotState.Down:
                //右（左)から下 :自分の下か右（左）下にブロックがあればひきあがる
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                    !boardController.CanSettle(pos + new Vector2Int(is_right ? 1 : -1, 0)))
                {
                    pos += Vector2Int.up;
                } break;//右（左）から下に回転：自分の下か右（左）下が埋まっていれば、位置を引き上げる 
            case RotState.Right:
                //右：右が埋まっていれば左に移動
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;//上（下）から右に回転：右が埋まっていれば、左に移動
            case RotState.Left:
                //左：左が埋まっていれば右に移動
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;//上（下）から左に回転：左が埋まっていれば、右に移動 
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }

        if (!CanMove(pos, rot)) return false;

        //実際に移動
        SetTransition(pos, rot, ROT_TIME);//回転の目的値の設定

        return true;
    }

    void Settle()
    {
        // 直接接地
        bool is_set0 = boardController.Settle(_position,
            (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);// 置いたのは空いていた場所のはず

        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate),
            (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);// 置いたのは空いていた場所のはず

        gameObject.SetActive(false);
    }

    //QuickDropの処理では、軸ぷよと子ぷよを一段ずつ仮想的に落としていって、設置したところでボードの情報を更新します

    //「QuickDrop」メソッドの実装
    void QuickDrop()
    {
        // 落ちれる一番下まで落ちる
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;// 一つ上の場所（最後に置けた場所）に戻す

        _position = pos;

        Settle();
    }





    bool Fall(bool is_fast)
    {
        _fallCount -= is_fast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

        // ブロックを飛び越えたら、行けるのかチェック
        while (_fallCount < 0)// ブロックが飛ぶ可能性がないこともない気がするので複数落下に対応
        {
            if (!CanMove(_position + Vector2Int.down, _rotate))
            {
                // 落ちれないなら
                _fallCount = 0; // 動きを止める
                if (0 < --_groundFrame) return true;// 時間があるなら、移動・回転可能

                // 時間切れになったら本当に固定
                Settle();
                return false;
            }

            // 落ちれるなら下に進む
            _position += Vector2Int.down;
            _last_position += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }

        if (is_fast) _additiveScore++; // 下に入れて、落ちれるときはボーナス追加

        return true;
    }

    static readonly Vector2Int[] rotate_tbl = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left, 
    };
    //CalcChildPuyoPos」を導入します
    //「CalcChildPuyoPos」の計算の高速化のために、定数の配列「rotate_tbl」を用意

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos,RotState rot)
    {
        return pos + rotate_tbl[(int)rot];//「_rotate」の値を整数に変換して、テーブル引き 
    }
    private bool CanMove(Vector2Int pos)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(pos+Vector2Int.up)) return false;//「BoardController」スクリプトに、移動した先の軸ぷよと子ぷよの状態が、空きになっているか問い合わせます
        
        //移動できないなら、処理を終了
        return true;
    }

    void Control() //キー入力処理を「Control」メソッドとして抜き出し
    {

        // 落とす
        if (!Fall(_logicalInput.IsRaw(LogicalInput.Key.Down))) return;// 接地したら終了

        // アニメ中はキー入力を受け付けない
        if (_animationController.Update()) return;

        //平行移動のキー入力取得
        if (_logicalInput.IsRepeat(LogicalInput.Key.Right))//左右移動はキーリピートで判定
        {
            if (Translate(true))return;//→ 右の入力判定を毎フレーム調べる。
        }
        if (_logicalInput.IsRepeat(LogicalInput.Key.Left))
        {
            if(Translate(false))return;//← 左の入力判定を毎フレーム調べる。
        }

        //回転のキー入力取得
        //回転は押した瞬間で判定
        if (_logicalInput.IsTrigger(LogicalInput.Key.RotR))//右回転
        {
            if(Rotate(true)) return;//「x」で右回転を受付け
        }
        if (_logicalInput.IsTrigger(LogicalInput.Key.RotL))//左回転
        {
            if(Rotate(false)) return;//「z」で左回転を受付け
        }

        //クイックドロップは話した瞬間で判定
        //クイックドロップのキー入力取得 押した瞬間に判定すると「あ、落とす場所がずれていた」という事が起きやすいので、押した後も補正できるように話した瞬間を使用

        if (_logicalInput.IsRelease(LogicalInput.Key.QuickDrop))
        {
            QuickDrop();//入力としては、↑が押された際に、処理「QuickDrop」を呼び出すようにします
        }
    }

    // rateが 1 -> 0 で、 pos_last -> pos, rot_last->rotに遷移。　rotがRotState.Invalidなら回転を考慮しない　（軸ぷよ用）
    static Vector3 Interpolate(Vector2Int pos,RotState rot,Vector2Int pos_last,RotState rot_last,float rate)
    {
        //平行移動
        Vector3 p = Vector3.Lerp( //平行移動は線形補間
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f), rate);

        if (rot == RotState.Invalid) return p;//軸ぷよは線形補間の結果を使ってもらう 

        //回転
        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = theta1 - theta0;

        //近い方向に回る
        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;//近い方向」は、回転の角度を [-π, π] の範囲に周期的に丸める

        theta = theta0 + rate * theta;//回転は最初の向きから目的の向きに、近い方向で回転する
        //距離1.0で角度0が上向きになるように平行移動の位置からずらす


        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }

    // Update is called once per frame
    //「UpdateInput」メソッドで入力処理をした後、「Control」メソッドで入力に応じた処理を行います。
    //「Control」メソッドの中では、今までの判定に「GetKeyDown」等を用いていたものを、論理入力に置き換えていきます

    // 得点の受け渡し
    public uint popScore()
    {
        uint score = _additiveScore;
        _additiveScore = 0;

        return score;
    }
    void FixedUpdate()
    {
        //「PlayerController」に用意した「UpdateInput」メソッドは、「PlayerController」の更新処理で呼び出します。 今回、固定フレームレートにするので、「Update」ではなく「FixedUpdate」メソッドを用意します。
        //入力を取り込む
     
        //「Update」内でアニメーションを更新
        // 操作を受けて動かす
        // 操作を受けて動かす
        Control();

        // 表示
        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;

        float anim_rate = _animationController.GetNormalized();//正規化時間を用いてぷよを補間しながら表示
        //正規化時間1で_last_pos, _last_rotの状態になり、正規化時間0で_pos, _rotの状態になる補間関数を導入して、位置を計算して設定
        _puyoControllers[0].SetPos(Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));//軸ぷよはrotの影響を受けないので、RotStateにInvalidを指定することで軸ぷよか判定できるようにする

        //毎フレーム位置を設定
        //強制的に位置を指定するので、確実ではあるが、オブジェクト指向としてはいまいち
    }
}
