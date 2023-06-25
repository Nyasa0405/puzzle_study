using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    const int TRANS_TIME = 3;//�ړ����x�J�ڎ���
    const int ROT_TIME = 3;//��]�J�ڎ���
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

    Vector2Int? position;//���Ղ�̈ʒu
    RotState _rotate = RotState.Up;//�p�x�� 0:�� 1:�E 2:�� 3;�� �Ŏ��i�q�Ղ�̈ʒu�j

    AnimationController _animationController=new AnimationController(); //AnimationController �̒ǉ�
    Vector2Int _last_position;
    RotState _last_Rotate = RotState.Up;
    //�J�ڑO�̈ʒu�u_last_position�v�A�����u_last_rot�v�̕ۑ�

    Vector2Int _position;//���Ղ�̈ʒu�@_position�𓱓�

    LogicalInput logicalInput = new();//�uLogicalInput�v�����o�[�̒ǉ�

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

    void SetTransition(Vector2Int pos, RotState rot, int time) //�J�ڎ��Ԃ̂�ݒ肷�郁�\�b�h�uSetTransition�v�̒ǉ�
    {
        //��Ԃ̂��ߕۑ����Ă���
        _last_position=_position;
        _last_Rotate=_rotate;//�Ă΂��O�̈ʒu�ƌ�����ۑ����Ă���

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

   
    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)LogicalInput.Key.MAX]{
        KeyCode.RightArrow, // Right
        KeyCode.LeftArrow,  // Left
        KeyCode.X,          // RotR
        KeyCode.Z,          // RotL
        KeyCode.UpArrow,    // QuickDrop
        KeyCode.DownArrow,  // Down

        //���݂̃f�o�C�X�̒l�́A�Y������{�^���̃L�[���e�[�u���Ƃ��ėp�ӂ��Ă�����
        //Update���Ăяo���O�ɂ��ꂼ��̃L�[��������Ă��邩�ǂ����𒲂ׂĂ����܂��B
        //�L�[�R���t�B�O�𓱓�����ꍇ��A�ΐ�v���C�ɂ���ꍇ�ɂ́A���̃e�[�u���� readonly �ɂ��Ȃ��ŁA���������čs���܂��B

    };

    //���͂���荞��

    void UpdateInput()
    {
        LogicalInput.Key inputDev = 0;//�f�o�C�X�l

        //�L�[���͎擾
        for (int i = 0; i < (int)LogicalInput.Key.MAX; i++)
            {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev = (LogicalInput.Key)(1 << i);
            }
        }
        logicalInput.Update(inputDev);
        //�uLogicalInput�v�Ɍ��݂̃f�o�C�X�̒l��^���čX�V 
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
        //���s�ړ��̃L�[���͎擾
        if (logicalInput.IsRepeat(LogicalInput.Key.Right))//���E�ړ��̓L�[���s�[�g�Ŕ���
        {
            if (Translate(true))return;//�� �E�̓��͔���𖈃t���[�����ׂ�B
        }
        if (logicalInput.IsRepeat(LogicalInput.Key.Left))
        {
            if(Translate(false))return;//�� ���̓��͔���𖈃t���[�����ׂ�B
        }

        //��]�̃L�[���͎擾
        //��]�͉������u�ԂŔ���
        if (logicalInput.IsTrigger(LogicalInput.Key.RotR))//�E��]
        {
            if(Rotate(true)) return;//�ux�v�ŉE��]����t��
        }
        if (logicalInput.IsTrigger(LogicalInput.Key.RotL))//����]
        {
            if(Rotate(false)) return;//�uz�v�ō���]����t��
        }

        //�N�C�b�N�h���b�v�͘b�����u�ԂŔ���
        //�N�C�b�N�h���b�v�̃L�[���͎擾 �������u�Ԃɔ��肷��Ɓu���A���Ƃ��ꏊ������Ă����v�Ƃ��������N���₷���̂ŁA����������␳�ł���悤�ɘb�����u�Ԃ��g�p

        if (logicalInput.IsRelease(LogicalInput.Key.QuickDrop))
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
    void FixedUpdate()
    {
        //�uPlayerController�v�ɗp�ӂ����uUpdateInput�v���\�b�h�́A�uPlayerController�v�̍X�V�����ŌĂяo���܂��B ����A�Œ�t���[�����[�g�ɂ���̂ŁA�uUpdate�v�ł͂Ȃ��uFixedUpdate�v���\�b�h��p�ӂ��܂��B
        //���͂���荞��
        UpdateInput();
        //�uUpdate�v���ŃA�j���[�V�������X�V
        // ������󂯂ē�����
        if (!_animationController.Update())//�A�j�����̓L�[���͂��󂯕t���Ȃ�
        {
            Control();//�A�j���[�V�������Ă��Ȃ���΁A�uControl�v���\�b�h���Ăяo��
        }

        float anim_rate = _animationController.GetNormalized();//���K�����Ԃ�p���ĂՂ���Ԃ��Ȃ���\��
        //���K������1��_last_pos, _last_rot�̏�ԂɂȂ�A���K������0��_pos, _rot�̏�ԂɂȂ��Ԋ֐��𓱓����āA�ʒu���v�Z���Đݒ�
        _puyoControllers[0].SetPos(Interpolate(_position, RotState.Invalid, _last_position, RotState.Invalid, anim_rate));
        _puyoControllers[1].SetPos(Interpolate(_position, _rotate, _last_position, _last_Rotate, anim_rate));//���Ղ��rot�̉e�����󂯂Ȃ��̂ŁARotState��Invalid���w�肷�邱�ƂŎ��Ղ悩����ł���悤�ɂ���

        //���t���[���ʒu��ݒ�
        //�����I�Ɉʒu���w�肷��̂ŁA�m���ł͂��邪�A�I�u�W�F�N�g�w���Ƃ��Ă͂��܂���
    }
}
