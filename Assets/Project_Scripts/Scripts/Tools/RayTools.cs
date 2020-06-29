using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTools
{
    public static Transform GetAixaHD(Transform _target) {
        Physics.queriesHitBackfaces = true;
        Ray newRay = new Ray(_target.position, -Vector3.up);
        RaycastHit hit;
        if (Physics.Raycast(newRay, out hit, 3))
        {
            if (hit.transform.tag == "HD")
            {
                return hit.transform;
            }
        }
        Ray newRay1 = new Ray(_target.position, Vector3.up);
        RaycastHit hit1;
        if (Physics.Raycast(newRay1, out hit1, 3))
        {
            if (hit1.transform.tag == "HD")
            {
                return hit1.transform;
            }
        }
        return null;
    }
    /// <summary>
    /// 球形碰撞检测
    /// </summary>
    /// <param name="_target"></param>
    /// <returns></returns>
    public static List<Transform> GetAarryTrans(Transform _target) {
        List<Transform> listTrans = new List<Transform>();
        Collider[] cols = Physics.OverlapSphere(_target.position, 4);
        if (cols.Length > 0)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                listTrans.Add(cols[i].transform);
            }
        }
        return listTrans;
    }
}
