using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手势识别
/// </summary>

public class GestureRecognition_ZH : MonoBehaviour
{
    //鼠标第一次点击位置
    public Vector2 _MousePos;
    //位置枚举
    public GestureState _GestureStateBe;
    //最小动作距离
    private float _MinGestureDistance = 20.0f;
    //手势开启布尔
    private bool _IsInvaild;
    //单例
    public static GestureRecognition_ZH _GestureRecognition;

    void Update()
    {
        //手势方法
        GestureOnClick();
        _GestureRecognition = this;
    }

    //手势方法
    public void GestureOnClick()
    {
        //手势为空
        _GestureStateBe = GestureState.Null;

        if (Input.GetMouseButtonDown(0))
        {
            //第一次鼠标点击位置记录
            _MousePos = Input.mousePosition;
            //开启手势识别
            _IsInvaild = true;

        }
        if (Input.GetMouseButton(0))
        {
            //鼠标轨迹向量
            Vector2 _Dis = (Vector2)Input.mousePosition - _MousePos;
            //画线
            Debug.DrawLine(_MousePos, (Vector2)Input.mousePosition, Color.cyan);
            //判断当前 向量的长度 是否大于 最小动作距离
            if (_Dis.magnitude > _MinGestureDistance)
            {
                //判断在 空间 X轴 还是在 Y轴
                if (Mathf.Abs(_Dis.x) > Mathf.Abs(_Dis.y) && _IsInvaild)
                {
                    if (_Dis.x > 0)
                    {
                        //如果当前向量值 X 大于 0  就是 Right 状态
                        _GestureStateBe = GestureState.Right;
                    }
                    else if (_Dis.x < 0)
                    {
                        //如果当前向量值 X 小于 0  就是 Lift 状态
                        _GestureStateBe = GestureState.Lift;
                    }
                }
                //判断在 空间 X轴 还是在 Y轴
                else if (Mathf.Abs(_Dis.x) < Mathf.Abs(_Dis.y) && _IsInvaild)
                {
                    if (_Dis.y > 0)
                    {
                        //如果当前向量值 Y 大于 0  就是 Up 状态
                        _GestureStateBe = GestureState.Up;
                    }
                    else if (_Dis.y < 0)
                    {
                        //如果当前向量值 Y 小于 0  就是 Down 状态
                        _GestureStateBe = GestureState.Down;
                    }
                }

                CallEvent();
                //关闭手势识别
                _IsInvaild = false;
            }
        }
    }

    //呼叫事件
    public void CallEvent()
    {
        switch (_GestureStateBe)
        {
            case GestureState.Null:

                print("Null");

                // Null 方法调用(自己写)

                break;

            case GestureState.Up:

                print("Up");

                // Up 方法调用(自己写)

                break;

            case GestureState.Down:

                print("Down");
                // Down 方法调用(自己写)

                break;

            case GestureState.Lift:

                print("Lift");

                // Lift 方法调用(自己写)

                break;

            case GestureState.Right:

                print("Right");

                // Right 方法调用(自己写)

                break;

            default:
                break;
        }
    }

    //状态枚举
    public enum GestureState
    {
        Null,
        Up,
        Down,
        Lift,
        Right
    }
}