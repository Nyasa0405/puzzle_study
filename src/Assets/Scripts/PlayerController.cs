using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };//ぷよのスクリプトを配列で持つ。　Default!を初期値として設定し設定もれを防ぐ
    [SerializeField] BoardController boardController = default!;//「BoardController」はあらかじめ設定しておくように「[SerializeField]」をつけてメンバー変数として用意しておきます
    // Start is called before the first frame update

    Vector2Int _position;//軸ぷよの位置　_positionを導入

    void Start()
    {
        //ひとまず決め打ちで色を決定
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);

        _position = new Vector2Int(2, 12);

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//ぷよぷよが登場する標準的な場所の位置
        _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y +1.0f,0.0f));//表示するゲームオブジェクトの位置を設定
    }

    private bool CanMove(Vector2Int pos)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(pos+Vector2Int.up)) return false;//「BoardController」スクリプトに、移動した先の軸ぷよと子ぷよの状態が、空きになっているか問い合わせます
        
        //移動できないなら、処理を終了
        return true;
    }

    private bool Translate(bool is_right)
    {
        //仮想的に移動できるか検証する
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);//動いた先の場所を計算　If Right +=1 If Left-=1
        if(!CanMove(pos)) return false;//移動できるか検証

        //  実際に移動
        _position = pos;//「_position」プロパティを更新

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//
        _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));//ぷよのゲームオブジェクトの位置を更新
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


    }
}
