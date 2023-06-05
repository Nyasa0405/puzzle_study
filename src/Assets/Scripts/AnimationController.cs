using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController 
{
    const float DELTA_TIME_MAX = 1.0f;//�X�V���Ԃ̏���̒l�̐ݒ�
    float _time = 0.0f;//���ԊǗ����邽�߂̕ϐ���p�ӂ��܂��B
    float _inv_time_max = 1.0f;//���̐��K�����Ԃ�ۑ����Ă����܂��B�������ACPU�͊���Z���|���Z�̕��������X���ɂ���̂ŁA�|���Z�ŏ����ł���悤�ɁA�ŏ��ɋt���ɂ��ĕۑ����܂��B

    public void Set(float max_time) //�A�j���[�V�������鎞�Ԃ�ݒ肷��uSet�v���\�b�h��ǉ� 
    {
        Debug.Assert(0.0f < max_time);//���̑J�ڎ��Ԃ͕s��
        //�O�̂��߁A���̐��������Ă��Ȃ����`�F�b�N����

        _time = max_time;
        _inv_time_max = 1.0f / max_time;//�J�ڎ��Ԑݒ莞�Ɂu_inv_time_max�v�����o�[��������
    }

    //�A�j���[�V�������Ȃ�true��Ԃ�
    public bool Update(float delta_time) //���Ԃ��X�V����uUpdate�v���\�b�h��ǉ�
    {
        //�O�ɌĂ΂ꂽ������̍X�V���ԁidelta_time�j���󂯎��

        //���܂莞�Ԃ����������ʂ͉������̂ŁA�X�V���Ԃ̏���𓱓�����
        if (DELTA_TIME_MAX<delta_time)delta_time = DELTA_TIME_MAX; //�X�V���Ԃ�����ŗ}���� 

        _time -= delta_time;
        //delta_time �������Ԃ����炷
        //0�ɂȂ�����I��
        if (_time <= 0.0f)
        {
            _time = 0.0f;//���̐��ɂ��Ȃ�
            //����A���x���Ă΂�Ă����v�Ȃ悤�ɕ��ɂȂ�����0�ɂ��Ă���
            return false;
        }
        return true;
    }

    public float GetNormalized() //���K�����Ԃ��擾����uGetNormalized�v���\�b�h��ǉ�
    {
        return _time * _inv_time_max; //�c�莞�ԂƁA�J�ڎ��Ԃ̋t���������Đ��K�����ԂƂ��܂�
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
