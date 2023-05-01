using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public enum PuyoType
    {

        Blank=0,

        Green=1,
        Red=2,
        Yellow=3,
        Blue=4,
        Purple=5,
        Cyan=6,

        Invalid =7
    };

[RequireComponent(typeof(Renderer))]//Renderer�R���|�[�l���g�̕t���Y���h��
public class PuyoController : MonoBehaviour
{
    static readonly Color[] color_table = new Color[]
        {
            Color.black,

            Color.green,
            Color.red,
            Color.yellow,
            Color.blue,
            Color.magenta,
            Color.cyan,

            Color.gray,
    };

    [SerializeField] Renderer My_Renderer = default!;//��k���w��AGetComponent��h��
    PuyoType _Type = PuyoType.Invalid;

    public void SetPuyoType(PuyoType type)//enum����
    {
        _Type = type;

        My_Renderer.material.color=color_table[(int)_Type];
    }

    public PuyoType GetPuyoType()//enum�^�ŕϐ��쐬
    {
        return _Type;
    }

    public void SetPos(Vector3 pos)//�O���Œ���transform�͖��
    {
        this.transform.localPosition = pos;
    }

};