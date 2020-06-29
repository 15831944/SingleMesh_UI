using BS.BL.Two;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (RayTools.GetAarryTrans(transform).Count > 0)
        {
            Debug.LogError("检测到物体" + RayTools.GetAarryTrans(transform).Count);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, 5);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
