using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicalInput
{
    const int KEY_REPEAT_START_TIME = 12;//�������ςȂ��ŃL�[���s�[�g�ɓ���t���[����
    const int KEY_REPEAT_ITERATION_TIME = 1;//�L�[���s�[�g�ɓ�������̍X�V�t���[����
    [Flags]
    public enum Key
    {
        Right = 1 << 0,
        Left = 1 << 1,
        RotR = 1 << 2,
        RotL = 1 << 3,
        QuickDrop = 1 << 4,
        Down = 1 << 5,//�����I�ɉ��ɑf�����ړ��ł���悤�Ɂu���{�^���v����`

        MAX = 6, //��
        //�u[flags]�v�����āAenum ��錾
        //�e�X�g����R�[�h�������₷���Ȃ�悤�ɁA�ő�l�uMAX�v����`
    }

    Key inputRaw;//���݂̒l
    Key inputTrg;//���͂����������̒l
    Key inputRel;//���͂��������Ƃ��̒l
    Key inputRep;//�A������
    int[] _trgWaitingTime = new int[(int)Key.MAX];//�܂��A�L�[���s�[�g���𐶐����邽�߂ɁA�c�薳�����Ԃ�ێ����邽�߂̐������{�^�����Ƃɕێ����܂�

   public void Clear()//Clear: ������ �֌W����S�Ă̒l��0�N���A����
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

    public void Update(Key inputDev) //Update: �f�o�C�X����̓��͂��󂯂Ă̓�����Ԃ̍X�V
    {
        //���͂�������/������
        inputTrg = (inputDev ^ inputRaw) & inputDev; //�����ꂽ�u�ԁA�����ꂽ�u�Ԃ̌��o  xor ���Z�őO�̃{�^���̏�Ԃƌ��݂̃{�^���̏�Ԃ��r���ĈقȂ�l�ɂȂ��Ă���{�^�������o������A�{�^���́u���݁v�̏�Ԃ�or������āA�u�������u�ԁv�𔻒肷��
        inputRel = (inputDev ^ inputRaw) & inputRaw;

        //���f�[�^�̐���
        inputRaw = inputDev; //���݂̃f�o�C�X�̏�Ԃ��X�V �����ꂽ�u�Ԃ̓��o�ɕK�v�Ȃ̂ŁA�����ꂽ�u�Ԃ��v�Z������ŏ㏑������

        //�L�[���s�[�g�̐���
        inputRep = 0;
        for (int i = 0; i < (int)Key.MAX; i++)
        {
            if(inputTrg.HasFlag((Key)(1<<i)))
            {
                inputRep |= (Key)(1 << i);
                _trgWaitingTime[i] = KEY_REPEAT_START_TIME; //�����ꂽ�u�Ԃ̌�͉������ςȂ��ł�ON�ɂȂ�Ȃ����Ԃ�ݒ� 
            }
            //�����ꂽ�u�Ԃ�ON
            else if (inputTrg.HasFlag((Key)(1 << i))) //�����ꂽ�u�ԂłȂ��ĉ������ςȂ��Ȃ�A�{�^���ɉ������J�E���^�����炷 
            {
                if (--_trgWaitingTime[i] <= 0)//�J�E���^��0�ɂȂ�����A���s�[�g��ON�ɂ��� 
                {
                    inputRep |= (Key)(1 << i);
                    _trgWaitingTime[i] = KEY_REPEAT_START_TIME; //2�x�ڈȍ~��ON�ɂȂ�܂ł̎��Ԃ͒Z������
                }
            }
        } //����́A���E�ړ��ׂ̈̃L�[���s�[�g���ԂȂ̂łQ�x�ڈȍ~�͂P�t����ON�ɂ��Ă��܂����A���j���[�ł̃J�[�\���ړ����͂����ƒ������Ԃ̕������R�ȓ����ɂȂ邩������܂���
    }
    //�����o�[�ϐ��Ƃ��ẮA���͂̒l��ێ�����ϐ���p�ӂ��܂��B�p�ӂ���ϐ��͎��̒ʂ�ł�
    public bool IsRaw(Key k)
    {
        return inputRaw.HasFlag(k);//inputRaw: �Q�[�����W�b�N�I��ON/OFF��ێ�����
    }

    public bool IsTrigger(Key k)
    {
        return inputTrg.HasFlag(k);//inputTrg: �e�{�^���������ꂽ�u�Ԃ���ON
    }

    public bool IsRelease(Key k)
    {
        return inputRel.HasFlag(k);//inputRel: �e�{�^���������ꂽ�u�Ԃ���ON
    }

    public bool IsRepeat(Key k)
    {
        return inputRep.HasFlag(k);//inputRep: �L�[���s�[�g�I��ON/OFF��ێ�����
    }

    //�Ȃ��A�O�����炱���̕ϐ��ɃA�N�Z�X����ɂ́Apublic���J���ꂽ�uIs***�v���\�b�h��ʂ��āA�e�t���O�������Ă��邩�ǂ������m�F���܂��B


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
