using BS.BL.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BS.BL.Two.Item
{
    public class Single2DManager : MonoBehaviour,IMode
    {
        public void IInit()
        {
            MainManager.GetInstance().ShowOrHideLabel();
            foreach (KeyValuePair<string, List<Two.Element.ElementItem>> item in MainManager.GetInstance().dicElements)
            {
                foreach (Two.Element.ElementItem itemChild in item.Value)
                {
                    //itemChild.gameObject.SetActive(true);
                    itemChild.first.transform.forward = -Vector3.up;
                    itemChild.transform.localScale = new Vector3(-Math.Abs(itemChild.transform.localScale.x), itemChild.transform.localScale.y, itemChild.transform.localScale.z);
                }
            }
            foreach (Transform item in MainManager.GetInstance().arrayLabelParent)
            {
                item.transform.forward = -Vector3.up;
                item.transform.localScale = new Vector3(-item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.z);
            }
            foreach (Transform item in (MainManager.GetInstance().dicMode[ModeType.area]as AreaManager).areaLabelParent)
            {
                item.transform.forward = -Vector3.up;
                //item.transform.localScale = new Vector3(-item.transform.localScale.x, item.transform.localScale.y, item.transform.localScale.z);
            }
            foreach (KeyValuePair<string, List<Transform>> item in MainManager.GetInstance().dicAreaLines)
            {
                foreach (Transform itemGo in item.Value)
                {
                    itemGo.GetChild(0).GetComponent<MeshRenderer>().material = itemGo.GetComponent<Line>().dataSetMat;
                }
            }
            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDisS(MainManager.GetInstance().tviewBase);
        }

        public void IQuit()
        {
        }

        public void IRun()
        {
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
}
