using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };//�Ղ�̃X�N���v�g��z��Ŏ��B�@Default!�������l�Ƃ��Đݒ肵�ݒ�����h��
    [SerializeField] BoardController boardController = default!;//�uBoardController�v�͂��炩���ߐݒ肵�Ă����悤�Ɂu[SerializeField]�v�����ă����o�[�ϐ��Ƃ��ėp�ӂ��Ă����܂�
    // Start is called before the first frame update

    Vector2Int _position;//���Ղ�̈ʒu�@_position�𓱓�

    void Start()
    {
        //�ЂƂ܂����ߑł��ŐF������
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);

        _position = new Vector2Int(2, 12);

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//�Ղ�Ղ悪�o�ꂷ��W���I�ȏꏊ�̈ʒu
        _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y +1.0f,0.0f));//�\������Q�[���I�u�W�F�N�g�̈ʒu��ݒ�
    }

    private bool CanMove(Vector2Int pos)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(pos+Vector2Int.up)) return false;//�uBoardController�v�X�N���v�g�ɁA�ړ�������̎��Ղ�Ǝq�Ղ�̏�Ԃ��A�󂫂ɂȂ��Ă��邩�₢���킹�܂�
        
        //�ړ��ł��Ȃ��Ȃ�A�������I��
        return true;
    }

    private bool Translate(bool is_right)
    {
        //���z�I�Ɉړ��ł��邩���؂���
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);//��������̏ꏊ���v�Z�@If Right +=1 If Left-=1
        if(!CanMove(pos)) return false;//�ړ��ł��邩����

        //  ���ۂɈړ�
        _position = pos;//�u_position�v�v���p�e�B���X�V

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));//
        _puyoControllers[1].SetPos(new Vector3((float)_position.x, (float)_position.y + 1.0f, 0.0f));//�Ղ�̃Q�[���I�u�W�F�N�g�̈ʒu���X�V
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


    }
}
