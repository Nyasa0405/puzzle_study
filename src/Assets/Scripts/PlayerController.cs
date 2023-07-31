using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    const int TRANS_TIME = 3;//�ړ����x�J�ڎ���
    const int ROT_TIME = 3;//��]�J�ڎ���

    // ��������
    const int FALL_COUNT_UNIT = 120; // �Ђƃ}�X��������J�E���g��
    const int FALL_COUNT_SPD = 10; // �������x
    const int FALL_COUNT_FAST_SPD = 20; // �����������̑��x
    const int GROUND_FRAMES = 50; // �ڒn�ړ��\����
    //�萔�Ƃ��Đݒ肵�܂�
    //���s�ړ��Ɖ�]�̈ړ����x��ς�����悤�ɂ��Ă����܂���
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
    LogicalInput _logicalInput = null;

    Vector2Int _position = new Vector2Int(2, 12);// ���Ղ�̈ʒu
    RotState _rotate = RotState.Up;//�p�x�� 0:�� 1:�E 2:�� 3;�� �Ŏ��i�q�Ղ�̈ʒu�j

    AnimationController _animationController=new AnimationController(); //AnimationController �̒ǉ�
    Vector2Int _last_position;
    RotState _last_rotate = RotState.Up;
    //�J�ڑO�̈ʒu�u_last_position�v�A�����u_last_rot�v�̕ۑ�

    // ��������
    int _fallCount = 0;
    int _groundFrame = GROUND_FRAMES;// �ڒn����

    // ���_
    uint _additiveScore = 0;

    // Start is called before the first frame update

    void Start()
    {
        gameObject.SetActive(false);// �Ղ�̎�ނ��ݒ肳���܂Ŗ���
    }

    public void SetLogicalInput(LogicalInput reference)
    {
        _logicalInput = reference;
    }
    public bool Spawn(PuyoType axis, PuyoType child)
    {
        // �����ʒu�ɏo���邩�m�F
        Vector2Int position = new(2, 12);// �����ʒu
        RotState rotate = RotState.Up;// �ŏ��͏����
        if (!CanMove(position, rotate)) return false;

        // �p�����[�^�̏�����
        _position = _last_position = position;
        _rotate = _last_rotate = rotate;
        _animationController.Set(1);
        _fallCount = 0;
        _groundFrame = GROUND_FRAMES;

        // �Ղ������
        _puyoControllers[0].SetPuyoType(axis);
        _puyoControllers[1].SetPuyoType(child);

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//�Ղ�Ղ悪�o�ꂷ��W���I�ȏꏊ�̈ʒu
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));//�\������Q�[���I�u�W�F�N�g�̈ʒu��ݒ�

        gameObject.SetActive(true);

        return true;
    }

    private bool CanMove(Vector2Int pos, RotState rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;
        //CanMove�v���\�b�h�ł̎q�Q�[���I�u�W�F�N�g�̈ʒu�̌��؂̏C��

        return true;
    }

    void SetTransition(Vector2Int pos, RotState rot, int time) //�J�ڎ��Ԃ̂�ݒ肷�郁�\�b�h�uSetTransition�v�̒ǉ�
    {
        //��Ԃ̂��ߕۑ����Ă���
        _last_position=_position;
        _last_rotate=_rotate;//�Ă΂��O�̈ʒu�ƌ�����ۑ����Ă���

        //�l�̍X�V
        _position = pos;
        _rotate = rot;//�ʒu��������X�V

        _animationController.Set(time);//AnimationController �Ɏ��Ԃ��w��
    }
    private bool Translate(bool is_right)
    {
        //���z�I�Ɉړ��ł��邩���؂���
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;

        //���ۂɈړ�
        SetTransition(pos, _rotate, TRANS_TIME);//���s�ړ��̖ړI�l�̐ݒ� 

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
        SetTransition(pos, rot, ROT_TIME);//��]�̖ړI�l�̐ݒ�

        return true;
    }

    void Settle()
    {
        // ���ڐڒn
        bool is_set0 = boardController.Settle(_position,
            (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);// �u�����̂͋󂢂Ă����ꏊ�̂͂�

        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate),
            (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);// �u�����̂͋󂢂Ă����ꏊ�̂͂�

        gameObject.SetActive(false);
    }

    //QuickDrop�̏����ł́A���Ղ�Ǝq�Ղ����i�����z�I�ɗ��Ƃ��Ă����āA�ݒu�����Ƃ���Ń{�[�h�̏����X�V���܂�

    //�uQuickDrop�v���\�b�h�̎���
    void QuickDrop()
    {
        // ��������ԉ��܂ŗ�����
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;// ���̏ꏊ�i�Ō�ɒu�����ꏊ�j�ɖ߂�

        _position = pos;

        Settle();
    }





    bool Fall(bool is_fast)
    {
        _fallCount -= is_fast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

        // �u���b�N���щz������A�s����̂��`�F�b�N
        while (_fallCount < 0)// �u���b�N����ԉ\�����Ȃ����Ƃ��Ȃ��C������̂ŕ��������ɑΉ�
        {
            if (!CanMove(_position + Vector2Int.down, _rotate))
            {
                // ������Ȃ��Ȃ�
                _fallCount = 0; // �������~�߂�
                if (0 < --_groundFrame) return true;// ���Ԃ�����Ȃ�A�ړ��E��]�\

                // ���Ԑ؂�ɂȂ�����{���ɌŒ�
                Settle();
                return false;
            }

            // �������Ȃ牺�ɐi��
            _position += Vector2Int.down;
            _last_position += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }

        if (is_fast) _additiveScore++; // ���ɓ���āA�������Ƃ��̓{�[�i�X�ǉ�

        return true;
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

    void Control() //�L�[���͏������uControl�v���\�b�h�Ƃ��Ĕ����o��
    {

        // ���Ƃ�
        if (!Fall(_logicalInput.IsRaw(LogicalInput.Key.Down))) return;// �ڒn������I��

        // �A�j�����̓L�[���͂��󂯕t���Ȃ�
        if (_animationController.Update()) return;

        //���s�ړ��̃L�[���͎擾
        if (_logicalInput.IsRepeat(LogicalInput.Key.Right))//���E�ړ��̓L�[���s�[�g�Ŕ���
        {
            if (Translate(true))return;//�� �E�̓��͔���𖈃t���[�����ׂ�B
        }
        if (_logicalInput.IsRepeat(LogicalInput.Key.Left))
        {
            if(Translate(false))return;//�� ���̓��͔���𖈃t���[�����ׂ�B
        }

        //��]�̃L�[���͎擾
        //��]�͉������u�ԂŔ���
        if (_logicalInput.IsTrigger(LogicalInput.Key.RotR))//�E��]
        {
            if(Rotate(true)) return;//�ux�v�ŉE��]����t��
        }
        if (_logicalInput.IsTrigger(LogicalInput.Key.RotL))//����]
        {
            if(Rotate(false)) return;//�uz�v�ō���]����t��
        }

        //�N�C�b�N�h���b�v�͘b�����u�ԂŔ���
        //�N�C�b�N�h���b�v�̃L�[���͎擾 �������u�Ԃɔ��肷��Ɓu���A���Ƃ��ꏊ������Ă����v�Ƃ��������N���₷���̂ŁA����������␳�ł���悤�ɘb�����u�Ԃ��g�p

        if (_logicalInput.IsRelease(LogicalInput.Key.QuickDrop))
        {
            QuickDrop();//���͂Ƃ��ẮA���������ꂽ�ۂɁA�����uQuickDrop�v���Ăяo���悤�ɂ��܂�
        }
    }

    // rate�� 1 -> 0 �ŁA pos_last -> pos, rot_last->rot�ɑJ�ځB�@rot��RotState.Invalid�Ȃ��]���l�����Ȃ��@�i���Ղ�p�j
    static Vector3 Interpolate(Vector2Int pos,RotState rot,Vector2Int pos_last,RotState rot_last,float rate)
    {
        //���s�ړ�
        Vector3 p = Vector3.Lerp( //���s�ړ��͐��`���
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f), rate);

        if (rot == RotState.Invalid) return p;//���Ղ�͐��`��Ԃ̌��ʂ��g���Ă��炤 

        //��]
        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = theta1 - theta0;

        //�߂������ɉ��
        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;//�߂������v�́A��]�̊p�x�� [-��, ��] �͈̔͂Ɏ����I�Ɋۂ߂�

        theta = theta0 + rate * theta;//��]�͍ŏ��̌�������ړI�̌����ɁA�߂������ŉ�]����
        //����1.0�Ŋp�x0��������ɂȂ�悤�ɕ��s�ړ��̈ʒu���炸�炷


        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }

    // Update is called once per frame
    //�uUpdateInput�v���\�b�h�œ��͏�����������A�uControl�v���\�b�h�œ��͂ɉ������������s���܂��B
    //�uControl�v���\�b�h�̒��ł́A���܂ł̔���ɁuGetKeyDown�v����p���Ă������̂��A�_�����͂ɒu�������Ă����܂�

    // ���_�̎󂯓n��
    public uint popScore()
    {
        uint score = _additiveScore;
        _additiveScore = 0;

        return score;
    }
    void FixedUpdate()
    {
        //�uPlayerController�v�ɗp�ӂ����uUpdateInput�v���\�b�h�́A�uPlayerController�v�̍X�V�����ŌĂяo���܂��B ����A�Œ�t���[�����[�g�ɂ���̂ŁA�uUpdate�v�ł͂Ȃ��uFixedUpdate�v���\�b�h��p�ӂ��܂��B
        //���͂���荞��
     
        //�uUpdate�v���ŃA�j���[�V�������X�V
        // ������󂯂ē�����
        // ������󂯂ē�����
        Control();

        // �\��
        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;

        float anim_rate = _animationController.GetNormalized();//���K�����Ԃ�p���ĂՂ���Ԃ��Ȃ���\��
        //���K������1��_last_pos, _last_rot�̏�ԂɂȂ�A���K������0��_pos, _rot�̏�ԂɂȂ��Ԋ֐��𓱓����āA�ʒu���v�Z���Đݒ�
        _puyoControllers[0].SetPos(Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));//���Ղ��rot�̉e�����󂯂Ȃ��̂ŁARotState��Invalid���w�肷�邱�ƂŎ��Ղ悩����ł���悤�ɂ���

        //���t���[���ʒu��ݒ�
        //�����I�Ɉʒu���w�肷��̂ŁA�m���ł͂��邪�A�I�u�W�F�N�g�w���Ƃ��Ă͂��܂���
    }
}
