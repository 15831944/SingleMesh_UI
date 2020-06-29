using BS.BL.Two;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point {
    public Vector3 pos;
    public string id;

    public Point(string id,Vector3 pos) {
        this.id = id;
        this.pos = pos;
    }
}

public class Node : MonoBehaviour {

    public string id;
    private int forkNum;

    private Vector3 pos;

    public Material dataSetMat, selectMat, unselectMat;

    private bool isDateSet = true;
    private bool isSelect = false;

    public List<Point> pointList = new List<Point>();

    // Use this for initialization
    void Start () {
        Invoke("RayTemp", 2f);
    }
    void RayTemp() {
        if (RayTools.GetAarryTrans(transform).Count == 2)
        {
            foreach (Transform item in RayTools.GetAarryTrans(transform))
            {
                if (item.name != transform.name)
                {
                    transform.localEulerAngles = item.parent.localEulerAngles;
                }
            }
        }
        if (RayTools.GetAarryTrans(transform).Count > 1)
        {
            foreach (Transform item in RayTools.GetAarryTrans(transform))
            {
                if (item.name != transform.name)
                {
                    unselectMat = new Material(unselectMat);
                    unselectMat.color = item.GetComponent<MeshRenderer>().material.color;
                    SetData();
                }
            }
        }
    }
   

    // Update is called once per frame
    void Update () {
        CheckMat();
       
    }

    public void Set(int forkNum,int id) {
        this.id = id.ToString();
        pos = transform.position;
        this.forkNum = forkNum;
    }

    public void SetPointList(List<Point> pointList) {
        this.pointList = pointList;
    }

    public void Select(bool isSelect)
    {
        transform.GetComponent<MeshRenderer>().material = isSelect ? selectMat : unselectMat;
        this.isSelect = isSelect;
        if (isSelect) {
        }
    }

    public void UpdatePos() {
        pos = transform.position;
        foreach (Point point in pointList) {
            point.pos = pos;
        }
    }

    public void SetData()
    {
        isDateSet = false;
    }

    public void CheckMat()
    {
        if (isSelect)
        {
            transform.GetComponent<MeshRenderer>().material = selectMat;
        }
        else
        {
            transform.GetComponent<MeshRenderer>().material = isDateSet ? dataSetMat : unselectMat;
            //transform.GetComponent<MeshRenderer>().material = dataSetMat;
        }
    }
}
