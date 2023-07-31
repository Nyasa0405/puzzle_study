using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct FallData
{
    public readonly int X { get; }
    public readonly int Y { get; }
    public readonly int Dest { get; } // 落ちる先
    public FallData(int x, int y, int dest)
    {
        X = x;
        Y = y;
        Dest = dest;
    }
}
public class BoardController : MonoBehaviour
{
    public const int FALL_FRAME_PER_CELL = 5;// 単位セル当たりの落下フレーム数
    public const int BOARD_WIDTH = 6;
    public const int BOARD_HEIGHT = 14;

    [SerializeField] GameObject prefabPuyo = default!;//最初にdefault!でかならず設定しないといけないようにする。[SF]でエディターから設定可能に

    int[,] _board = new int[BOARD_HEIGHT, BOARD_WIDTH];//newで配列、新規作成
    GameObject[,]_Puyos =new GameObject[BOARD_HEIGHT, BOARD_WIDTH];//ゲームオブジェクトの配列を保持

    // 落ちる際の一次的変数
    List<FallData> _falls = new();
    int _fallFrames = 0;

    private void ClearAll()
    {
        for (int y=0;y<BOARD_HEIGHT;y++)
        {
            for (int x=0;x<BOARD_WIDTH;x++)
            {
                _board[y,x] = 0;//まとまった処理なのでSTARTには記さない

                if (_Puyos[y,x] != null) Destroy(_Puyos[y,x]);
                _Puyos[y, x] = null;//オブジェクトの破棄の追加
            }
        }
    }
    // Start is called before the first frame update
   public void Start()
    {
        ClearAll();

        //for(int y=0;y<BOARD_HEIGHT;y++)
        //{
        //    for(int x=0;x<BOARD_WIDTH;x++)
        //    {
        //        Settle(new Vector2Int(x, y), Random.Range(1, 7));// 埋める

        //    }
        //}
    }

    public static bool IsValidated(Vector2Int pos)
    {
        return 0 <= pos.x && pos.x < BOARD_WIDTH
            && 0 <= pos.y && pos.y < BOARD_HEIGHT;//0<x<WIDTH　または　0<y<HEIGHT　　はみ出してない？
    }

    public bool CanSettle(Vector2Int pos)
    {
        if (!IsValidated(pos)) return false;//失敗

        return 0 == _board[pos.y, pos.x];//配列の値が埋まっていないか(0になってないか)
    }

    public bool Settle(Vector2Int pos, int val)
    {
        if (!CanSettle(pos)) return false;//置くことはできる？
        _board[pos.y, pos.x] = val;

        Debug.Assert(_Puyos[pos.y, pos.x] == null);//念のため確認する
        Vector3 world_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        _Puyos[pos.y, pos.x] = Instantiate(prefabPuyo, world_position, Quaternion.identity, transform);
        _Puyos[pos.y,pos.x].GetComponent<PuyoController>().SetPuyoType((PuyoType)val);//,毎フレーム呼ばれるわけではないのでGetComponent

        return true;
    }
    // 下が空間となっていて落ちるぷよを検索する
    public bool CheckFall()
    {
        _falls.Clear();
        _fallFrames = 0;

        // 落ちる先の高さの記録用
        int[] dsts = new int[BOARD_WIDTH];
        for (int x = 0; x < BOARD_WIDTH; x++) dsts[x] = 0;

        int max_check_line = BOARD_HEIGHT - 1;// 実はぷよぷよ通では最上段は落ちてこない
        for (int y = 0; y < max_check_line; y++)// 下から上に検索
        {
            for (int x = 0; x < BOARD_WIDTH; x++)
            {
                if (_board[y, x] == 0) continue;

                int dst = dsts[x];
                dsts[x] = y + 1;// 上のぷよが落ちてくるなら自分の上

                if (y == 0) continue;// 一番下なら落ちない

                if (_board[y - 1, x] != 0) continue;// 下があれば対象外

                _falls.Add(new FallData(x, y, dst));

                // データを変更しておく
                _board[dst, x] = _board[y, x];
                _board[y, x] = 0;
                _Puyos[dst, x] = _Puyos[y, x];
                _Puyos[y, x] = null;

                dsts[x] = dst + 1;// 次の物は落ちたさらに上に乗る
            }
        }

        return _falls.Count != 0;
    }

    public bool Fall()
    {
        _fallFrames++;

        float dy = _fallFrames / (float)FALL_FRAME_PER_CELL;
        int di = (int)dy;

        for (int i = _falls.Count - 1; 0 <= i; i--)// ループ中で削除しても安全なように後ろから検索
        {
            FallData f = _falls[i];

            Vector3 pos = _Puyos[f.Dest, f.X].transform.localPosition;
            pos.y = f.Y - dy;

            if (f.Y <= f.Dest + di)
            {
                pos.y = f.Dest;
                _falls.RemoveAt(i);
            }
            _Puyos[f.Dest, f.X].transform.localPosition = pos;// 表示位置の更新
        }

        return _falls.Count != 0;
    }
}
