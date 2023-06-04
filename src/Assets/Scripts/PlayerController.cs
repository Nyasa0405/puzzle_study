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
    //�u��ԁv�𓱓����ĊǗ�����̂���ԁB����́A�uRotState�v�Ƃ�����]�̏�Ԃ𓱓�

    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };//�Ղ�̃X�N���v�g��z��Ŏ��B�@Default!�������l�Ƃ��Đݒ肵�ݒ�����h��
    [SerializeField] BoardController boardController = default!;//�uBoardController�v�͂��炩���ߐݒ肵�Ă����悤�Ɂu[SerializeField]�v�����ă����o�[�ϐ��Ƃ��ėp�ӂ��Ă����܂�

    Vector2Int? position;//���Ղ�̈ʒu
    RotState _rotate = RotState.Up;//�p�x�� 0:�� 1:�E 2:�� 3;�� �Ŏ��i�q�Ղ�̈ʒu�j


    Vector2Int _position;//���Ղ�̈ʒu�@_position�𓱓�

    // Start is called before the first frame update

    void Start()
    {
       // Start�v���\�b�h�ł̎q�Q�[���I�u�W�F�N�g�̈ʒu�̐ݒ�
        //�ЂƂ܂����ߑł��ŐF������
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);

        _position = new Vector2Int(2, 12);
        _rotate = RotState.Up;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//�Ղ�Ղ悪�o�ꂷ��W���I�ȏꏊ�̈ʒu
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y,0.0f));//�\������Q�[���I�u�W�F�N�g�̈ʒu��ݒ�
    }

    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;
        //CanMove�v���\�b�h�ł̎q�Q�[���I�u�W�F�N�g�̈ʒu�̌��؂̏C��

        return true;
    }

    private bool Translate(bool is_right)
    {
        //���z�I�Ɉړ��ł��邩���؂���
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;

        //���ۂɈړ�
        _position = pos;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));
        //�uTranslate�v���\�b�h�ł̎q�Q�[���I�u�W�F�N�g�̈ʒu�̌��؂̏C��

        return true;
    }

    bool Rotate (bool is_right)
    {
        RotState rot = (RotState)(((int)_rotate + (is_right ? +1 : +3)) & 3);

        //���z�I�Ɉړ��ł��邩���؂���i�㉺���E�ɂ��炵�������m�F�j
        Vector2Int pos = _position;

        switch (rot)
        {
            case RotState.Down:
                //�E�i��)���牺 :�����̉����E�i���j���Ƀu���b�N������΂Ђ�������
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                    !boardController.CanSettle(pos + new Vector2Int(is_right ? 1 : -1, 0)))
                {
                    pos += Vector2Int.up;
                } break;//�E�i���j���牺�ɉ�]�F�����̉����E�i���j�������܂��Ă���΁A�ʒu�������グ�� 
            case RotState.Right:
                //�E�F�E�����܂��Ă���΍��Ɉړ�
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;//��i���j����E�ɉ�]�F�E�����܂��Ă���΁A���Ɉړ�
            case RotState.Left:
                //���F�������܂��Ă���ΉE�Ɉړ�
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;//��i���j���獶�ɉ�]�F�������܂��Ă���΁A�E�Ɉړ� 
            case RotState.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }

        if (!CanMove(pos, rot)) return false;

        //���ۂɈړ�
        _position = pos;
        _rotate = rot;
        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));

        return true;
    }

    //QuickDrop�̏����ł́A���Ղ�Ǝq�Ղ����i�����z�I�ɗ��Ƃ��Ă����āA�ݒu�����Ƃ���Ń{�[�h�̏����X�V���܂�

    //�uQuickDrop�v���\�b�h�̎���
    void QuickDrop()
    {
        //������ԉ��܂ŗ�����
        Vector2Int pos = _position;//��i�����Ƃ��邩�m�F���āA���Ƃ��Ȃ��Ȃ钼�O�̏ꏊ���擾���܂� 
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;//���̏ꏊ�i�Ō�ɒu�����ꏊ)�ɖ߂�

        _position = pos; //�ڒn����ꏊ������������A�{�[�h�̏����X�V���܂�

        //���ڒ��n
        bool is_Set0 = boardController.Settle(_position,
            (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_Set0);//�u�����̂͋󂢂Ă����ꏊ�̂͂�

        bool is_Set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate),
            (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_Set1);//�u�����̂͋󂢂Ă����ꏊ�̂͂�

        //�O�̂��߁A�{���ɒu�������ǂ������A�T�[�V�������g���Ă����킩��悤�ɂ��܂��� 
        gameObject.SetActive(false);
        //�{�[�h�ɂՂ��u������A�i�v���C���[�̕��ŕ\�����Ă���Ղ���������߁j�����̃Q�[���I�u�W�F�N�g���A�N�e�B�u�ɂ��܂� 
    }

    static readonly Vector2Int[] rotate_tbl = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left, 
    };
    //CalcChildPuyoPos�v�𓱓����܂�
    //�uCalcChildPuyoPos�v�̌v�Z�̍������̂��߂ɁA�萔�̔z��urotate_tbl�v��p��

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos,RotState rot)
    {
        return pos + rotate_tbl[(int)rot];//�u_rotate�v�̒l�𐮐��ɕϊ����āA�e�[�u������ 
    }
    private bool CanMove(Vector2Int pos)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(pos+Vector2Int.up)) return false;//�uBoardController�v�X�N���v�g�ɁA�ړ�������̎��Ղ�Ǝq�Ղ�̏�Ԃ��A�󂫂ɂȂ��Ă��邩�₢���킹�܂�
        
        //�ړ��ł��Ȃ��Ȃ�A�������I��
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        //���s�ړ��̃L�[���͎擾
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Translate(true);//�� �E�̓��͔���𖈃t���[�����ׂ�B
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Translate(false);//�� ���̓��͔���𖈃t���[�����ׂ�B
        }

        //��]�̃L�[���͎擾
        if (Input.GetKeyDown(KeyCode.X))//�E��]
        {
            Rotate(true);//�ux�v�ŉE��]����t��
        }
        if (Input.GetKeyDown(KeyCode.Z))//����]
        {
            Rotate(false);//�uz�v�ō���]����t��
        }

        //�N�C�b�N�h���b�v�̃L�[���͎擾
        if(Input.GetKey(KeyCode.UpArrow))
        {
            QuickDrop();//���͂Ƃ��ẮA���������ꂽ�ۂɁA�����uQuickDrop�v���Ăяo���悤�ɂ��܂�
        }


    }
}
