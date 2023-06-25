using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    
    int _time = 0;//���ԊǗ����邽�߂̕ϐ���p�ӂ��܂��B �c�莞�Ԃ̃����o�[�u_time�v�𐮐���
    float _inv_time_max = 1.0f;//���̐��K�����Ԃ�ۑ����Ă����܂��B�������ACPU�͊���Z���|���Z�̕��������X���ɂ���̂ŁA�|���Z�ŏ����ł���悤�ɁA�ŏ��ɋt���ɂ��ĕۑ����܂��B

    public void Set(int max_time) //�A�j���[�V�������鎞�Ԃ�ݒ肷��uSet�v���\�b�h��ǉ� 
    {
        Debug.Assert(0 < max_time);//���̑J�ڎ��Ԃ͕s�� �J�ڎ��Ԃ�ݒ肷��uSet�v���\�b�h�̈����𐮐��� 
        //�O�̂��߁A���̐��������Ă��Ȃ����`�F�b�N����

        _time = max_time;
        _inv_time_max = 1.0f / max_time;//�J�ڎ��Ԑݒ莞�Ɂu_inv_time_max�v�����o�[�������� ���������_���Ƃ��ď�������ꏊ�ł̓L���X�g��ǉ�
    }

    //�A�j���[�V�������Ȃ�true��Ԃ�
    public bool Update() //���Ԃ��X�V����uUpdate�v���\�b�h��ǉ� �uUpdate�v���\�b�h�̈����͂Ȃ���
    {
        _time = Math.Max(--_time, 0);//�����ł́A�u_time�v��������炵��0�����ɂȂ�Ȃ��悤��Max���\�b�h�ŃN�����v����
        //������炷�̂ŁA���炷�l�̏���̊m�F�͍폜
        return (0 < _time);
    }

    public float GetNormalized() //���K�����Ԃ��擾����uGetNormalized�v���\�b�h��ǉ�
    {
        return _time * _inv_time_max; //�c�莞�ԂƁA�J�ڎ��Ԃ̋t���������Đ��K�����ԂƂ��܂�
    }
}

