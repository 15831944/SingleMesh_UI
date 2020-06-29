using BS.BL.Interface;
using BS.BL.Two;
using BS.BL.Two.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 屏幕线框绘制以及物体选择
/// </summary>

public class DrawRectangle_ZH : MonoBehaviour
{
    //线框颜色
    [Header("线框颜色")]
    public Color _RectColor = Color.green;
    [Header("选中材质")]
    public Material _SelectingMaterial;
    [Header("取消材质")]
    public Material _CancelMaterial;

    //单例
    public static DrawRectangle_ZH _DrawRectangle;

    //记下鼠标按下位置
    private Vector3 _MouseStart = Vector3.zero;

    //画线的材质 不设定系统会用当前材质画线 结果不可控
    private Material _RectMat;

    //是否开始画线标志
    private bool _IsDrawRectangle = false;

    //场景角色数组
    public List<GameObject> _Characters;

    //当前选择数组
    private List<GameObject> _NewListGam = new List<GameObject>();

    private void Awake()
    {
        _DrawRectangle = this;
        //场景特定物体数组存入
        GameobjectAddList();
    }

    void Start()
    {
        //生成画线的材质
        _RectMat = new Material(Shader.Find("UI/Default"));
        //GameObject(游戏对象)没有显示在层次结构中，没有保存到场景中，也没有被Resources.UnloadUnusedAssets卸载。
        _RectMat.hideFlags = HideFlags.HideAndDontSave;
        //Gameobject(游戏对象)没有显示在层次结构中，没有保存到场景中，也没有被Resources.UnloadUnusedAssets卸载
        _RectMat.shader.hideFlags = HideFlags.HideAndDontSave;
    }


    void Update()
    {
        if (MainManager.GetInstance().isCurrent)
        {
            //按下鼠标左键
            if (Input.GetMouseButtonDown(0))
            {
                //如果鼠标左键按下 设置开始画线标志
                _IsDrawRectangle = true;
                //记录按下位置
                _MouseStart = Input.mousePosition;
            }
            //如果鼠标左键放开 结束画线
            else if (Input.GetMouseButtonUp(0))
            {
                _IsDrawRectangle = false;
                checkSelection(_MouseStart, Input.mousePosition);//框选物体
            }

            string resSelectList = "";
            foreach (ElementItem item in MainManager.GetInstance().listSelectItems)
            {
                resSelectList += "{\"resId\":\"" + item.name + "\",\"t_eCode\":\"" + item.t_eType + "\"},";
            }
            if (!string.IsNullOrEmpty(resSelectList))
            {
                resSelectList = "[" + resSelectList + "]";
                resSelectList = resSelectList.Remove(resSelectList.LastIndexOf(","), 1); ; //移除掉最后一个","
            }
            GameObject.Find("JSInterface").GetComponent<JSInterface>().selectCurrentResList(resSelectList);
            if (Input.GetMouseButton(0))
            {
                //手势识别
                switch (GestureRecognition_ZH._GestureRecognition._GestureStateBe)
                {
                    case GestureRecognition_ZH.GestureState.Null:

                        //扩展

                        break;

                    case GestureRecognition_ZH.GestureState.Up:

                        //扩展

                        break;

                    case GestureRecognition_ZH.GestureState.Down:

                        //取消选中方法
                        break;

                    case GestureRecognition_ZH.GestureState.Lift:

                        //扩展

                        break;

                    case GestureRecognition_ZH.GestureState.Right:

                        //扩展

                        break;

                    default:
                        break;
                }
            }
            if (Input.GetMouseButton(1))
            {
                //移动方法  
                MoveController(_NewListGam);

            }
        }
    }
    void checkSelection(Vector3 start, Vector3 end)
    {

        Vector3 p1 = Vector3.zero;

        Vector3 p2 = Vector3.zero;

        if (start.x > end.x)
        {//这些判断是用来确保p1的xy坐标小于p2的xy坐标，因为画的框不见得就是左下到右上这个方向的

            p1.x = end.x;

            p2.x = start.x;

        }

        else
        {

            p1.x = start.x;

            p2.x = end.x;

        }

        if (start.y > end.y)
        {

            p1.y = end.y;

            p2.y = start.y;

        }

        else
        {

            p1.y = start.y;

            p2.y = end.y;

        }

        foreach (GameObject obj in _Characters)
        {//把可选择的对象保存在characters数组里

            Vector3 location = Camera.main.WorldToScreenPoint(obj.transform.position);//把对象的position转换成屏幕坐标

            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y

            || location.z < Camera.main.nearClipPlane || location.z > Camera.main.farClipPlane)//z方向就用摄像机的设定值，看不见的也不需要选择了

            {
                //disselecting(obj);//上面的条件是筛选 不在选择范围内的对象，然后进行取消选择操作，比如把物体放到default层，就不显示轮廓线了
                Disselecting(obj);
            }

            else

            {
                //selecting(obj);//否则就进行选中操作，比如把物体放到画轮廓线的层去
                //print("+++" + obj.name);
                Selecting(obj);
            }

        }

    }
    /// <summary>
    /// 在渲染后执行
    /// </summary>
    void OnPostRender()
    {
        //画线这种操作推荐在 OnPostRender（）里进行 而不是直接放在Update，所以需要标志来开启
        if (_IsDrawRectangle)
        {
            //鼠标当前位置
            Vector3 _MouseEnd = Input.mousePosition;
            //保存摄像机变换矩阵
            GL.PushMatrix();

            if (!_RectMat)
                return;

            _RectMat.SetPass(0);
            //设置用屏幕坐标绘图
            GL.LoadPixelMatrix();
            GL.Begin(GL.QUADS);
            //设置颜色和透明度，方框内部透明
            GL.Color(new Color(_RectColor.r, _RectColor.g, _RectColor.b, 0.1f));
            GL.Vertex3(_MouseStart.x, _MouseStart.y, 0);
            GL.Vertex3(_MouseEnd.x, _MouseStart.y, 0);
            GL.Vertex3(_MouseEnd.x, _MouseEnd.y, 0);
            GL.Vertex3(_MouseStart.x, _MouseEnd.y, 0);
            GL.End();
            GL.Begin(GL.LINES);
            //设置方框的边框颜色 边框不透明
            GL.Color(_RectColor);
            GL.Vertex3(_MouseStart.x, _MouseStart.y, 0);
            GL.Vertex3(_MouseEnd.x, _MouseStart.y, 0);
            GL.Vertex3(_MouseEnd.x, _MouseStart.y, 0);
            GL.Vertex3(_MouseEnd.x, _MouseEnd.y, 0);
            GL.Vertex3(_MouseEnd.x, _MouseEnd.y, 0);
            GL.Vertex3(_MouseStart.x, _MouseEnd.y, 0);
            GL.Vertex3(_MouseStart.x, _MouseEnd.y, 0);
            GL.Vertex3(_MouseStart.x, _MouseStart.y, 0);
            GL.End();
            //恢复摄像机投影矩阵
            GL.PopMatrix();

            //物体判断方法
            CheckSelection(_MouseStart, _MouseEnd);
        }
    }

    /// <summary>
    /// 线框绘制范围物体判断方法
    /// </summary>
    /// <param 开始位置="_StartPosition">  </param>
    /// <param 结束位置="_EndPosition">  </param>
    void CheckSelection(Vector3 _StartPosition, Vector3 _EndPosition)
    {
        Vector3 p1 = Vector3.zero;
        Vector3 p2 = Vector3.zero;
        //这些判断是用来确保p1的xy坐标小于p2的xy坐标，因为画的框不见得就是左下到右上这个方向的
        if (_StartPosition.x > _EndPosition.x)
        {
            p1.x = _EndPosition.x;
            p2.x = _StartPosition.x;
        }
        else
        {
            p1.x = _StartPosition.x;
            p2.x = _EndPosition.x;
        }
        if (_StartPosition.y > _EndPosition.y)
        {
            p1.y = _EndPosition.y;
            p2.y = _StartPosition.y;
        }
        else
        {
            p1.y = _StartPosition.y;
            p2.y = _EndPosition.y;
        }
        foreach (GameObject _Obj in _Characters)
        {
            //把可选择的对象保存在characters数组里
            //把对象的position转换成屏幕坐标
            Vector3 location = GetComponent<Camera>().WorldToScreenPoint(_Obj.transform.position);
            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y
                //z方向就用摄像机的设定值，看不见的也不需要选择了
                || location.z < GetComponent<Camera>().nearClipPlane || location.z > GetComponent<Camera>().farClipPlane)
            {
                //上面的条件是筛选 不在选择范围内的对象，然后进行取消选择操作，比如把物体放到default层，就不显示轮廓线了
                //Disselecting(obj);
            }
            else
            {
                //否则就进行选中操作，比如把物体放到画轮廓线的层去
                Selecting(_Obj);
            }
        }
    }

    void Disselecting(GameObject obj)
    {
        obj.GetComponent<ElementItem>().DisSelect();
    }

    void Selecting(GameObject obj)
    {
        obj.GetComponent<ElementItem>().Select();
    }

    /// <summary>
    /// 场景物体数组存入
    /// </summary>
    private void GameobjectAddList()
    {
        //查找特定数组存入当前控制数组
        _Characters.AddRange(GameObject.FindGameObjectsWithTag("Player"));
    }

    /// <summary>
    /// 物体移动方法
    /// </summary>
    /// <param 框选物体数组="_ObjList"></param>
    private void MoveController(List<GameObject> _ObjList)
    {
        //将鼠标的屏幕坐标转换到世界坐标内 ，深度值设为相机的最大深度，然后从相机发出射线到该点，中途如果有碰撞，将碰撞的物体设为选择
        Vector3 _MouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(_MouseStart.x, _MouseStart.y, Camera.main.farClipPlane));
        Ray ray = new Ray(Camera.main.transform.position, _MouseWorldPosition - Camera.main.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //如果射线点击到的是地面的话
            if (hit.transform.tag == "地面")
            {
                //选择物体的位置就等于射线点击的位置
                for (int i = 0; i < _ObjList.Count; i++)
                {
                    //_ObjList[i].transform.position = hit.point;

                    //移动算法
                    MoveVrithmetic();
                }

            }
        }
    }

    /// <summary>
    /// 移动算法
    /// </summary>
    private void MoveVrithmetic()
    {
        //没写 哈哈哈
    }
}