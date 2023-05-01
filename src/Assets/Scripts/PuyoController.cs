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

[RequireComponent(typeof(Renderer))]//Rendererコンポーネントの付け忘れを防ぐ
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

    [SerializeField] Renderer My_Renderer = default!;//非ヌル指定、GetComponentを防ぐ
    PuyoType _Type = PuyoType.Invalid;

    public void SetPuyoType(PuyoType type)//enum引数
    {
        _Type = type;

        My_Renderer.material.color=color_table[(int)_Type];
    }

    public PuyoType GetPuyoType()//enum型で変数作成
    {
        return _Type;
    }

    public void SetPos(Vector3 pos)//外部で直接transformは野蛮
    {
        this.transform.localPosition = pos;
    }

};