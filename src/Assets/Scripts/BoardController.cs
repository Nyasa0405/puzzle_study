using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public const int BOARD_WIDTH = 6;
    public const int BOARD_HEIGHT = 14;//���ƍ����̒�`�Bconst�ŌŒ�

    [SerializeField] GameObject prefabPuyo = default!;//�ŏ���default!�ł��Ȃ炸�ݒ肵�Ȃ��Ƃ����Ȃ��悤�ɂ���B[SF]�ŃG�f�B�^�[����ݒ�\��

    int[,] _board = new int[BOARD_HEIGHT, BOARD_WIDTH];//new�Ŕz��A�V�K�쐬
    GameObject[,]_Puyos =new GameObject[BOARD_HEIGHT, BOARD_WIDTH];//�Q�[���I�u�W�F�N�g�̔z���ێ�


    private void ClearAll()
    {
        for (int y=0;y<BOARD_HEIGHT;y++)
        {
            for (int x=0;x<BOARD_WIDTH;x++)
            {
                _board[y,x] = 0;//�܂Ƃ܂��������Ȃ̂�START�ɂ͋L���Ȃ�

                if (_Puyos[y,x] != null) Destroy(_Puyos[y,x]);
                _Puyos[y, x] = null;//�I�u�W�F�N�g�̔j���̒ǉ�
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
        //        Settle(new Vector2Int(x, y), Random.Range(1, 7));// ���߂�

        //    }
        //}
    }

    public static bool IsValidated(Vector2Int pos)
    {
        return 0 <= pos.x && pos.x < BOARD_WIDTH
            && 0 <= pos.y && pos.y < BOARD_HEIGHT;//0<x<WIDTH�@�܂��́@0<y<HEIGHT�@�@�͂ݏo���ĂȂ��H
    }

    public bool CanSettle(Vector2Int pos)
    {
        if (!IsValidated(pos)) return false;//���s

        return 0 == _board[pos.y, pos.x];//�z��̒l�����܂��Ă��Ȃ���(0�ɂȂ��ĂȂ���)
    }

    public bool Settle(Vector2Int pos, int val)
    {
        if (!CanSettle(pos)) return false;//�u�����Ƃ͂ł���H
        _board[pos.y, pos.x] = val;

        Debug.Assert(_Puyos[pos.y, pos.x] == null);//�O�̂��ߊm�F����
        Vector3 world_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        _Puyos[pos.y, pos.x] = Instantiate(prefabPuyo, world_position, Quaternion.identity, transform);
        _Puyos[pos.y,pos.x].GetComponent<PuyoController>().SetPuyoType((PuyoType)val);//,���t���[���Ă΂��킯�ł͂Ȃ��̂�GetComponent

        return true;
    }
}
