using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
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

    Vector2Int? position;//軸ぷよの位置
    RotState _rotate = RotState.Up;//角度は 0:上 1:右 2:下 3;左 で持つ（子ぷよの位置）


    Vector2Int _position;//軸ぷよの位置　_positionを導入

    // Start is called before the first frame update

    void Start()
    {
       // Start」メソッドでの子ゲームオブジェクトの位置の設定
        //ひとまず決め打ちで色を決定
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);

        _position = new Vector2Int(2, 12);
        _rotate = RotState.Up;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//ぷよぷよが登場する標準的な場所の位置
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y,0.0f));//表示するゲームオブジェクトの位置を設定
    }

    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;
        //CanMove」メソッドでの子ゲームオブジェクトの位置の検証の修正

        return true;
    }

    private bool Translate(bool is_right)
    {
        //仮想的に移動できるか検証する
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;

        //実際に移動
        _position = pos;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));
        //「Translate」メソッドでの子ゲームオブジェクトの位置の検証の修正

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
        _position = pos;
        _rotate = rot;
        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));

        return true;
    }

    //QuickDropの処理では、軸ぷよと子ぷよを一段ずつ仮想的に落としていって、設置したところでボードの情報を更新します

    //「QuickDrop」メソッドの実装
    void QuickDrop()
    {
        //堕ちれる一番下まで落ちる
        Vector2Int pos = _position;//一段ずつ落とせるか確認して、落とせなくなる直前の場所を取得します 
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;//一つ上の場所（最後に置けた場所)に戻す

        _position = pos; //接地する場所が判明したら、ボードの情報を更新します

        //直接着地
        bool is_Set0 = boardController.Settle(_position,
            (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_Set0);//置いたのは空いていた場所のはず

        bool is_Set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate),
            (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_Set1);//置いたのは空いていた場所のはず

        //念のため、本当に置けたかどうかをアサーションを使ってすぐわかるようにしました 
        gameObject.SetActive(false);
        //ボードにぷよを置いたら、（プレイヤーの方で表示しているぷよを消すため）自分のゲームオブジェクトを非アクティブにします 
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

    // Update is called once per frame
    void Update()
    {
        //平行移動のキー入力取得
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Translate(true);//→ 右の入力判定を毎フレーム調べる。
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Translate(false);//← 左の入力判定を毎フレーム調べる。
        }

        //回転のキー入力取得
        if (Input.GetKeyDown(KeyCode.X))//右回転
        {
            Rotate(true);//「x」で右回転を受付け
        }
        if (Input.GetKeyDown(KeyCode.Z))//左回転
        {
            Rotate(false);//「z」で左回転を受付け
        }

        //クイックドロップのキー入力取得
        if(Input.GetKey(KeyCode.UpArrow))
        {
            QuickDrop();//入力としては、↑が押された際に、処理「QuickDrop」を呼び出すようにします
        }


    }
}
