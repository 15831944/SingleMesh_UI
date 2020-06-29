using BS.BL.Two.Element;
using BS.BL.Two.Item;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BS.BL.Two
{
    public class PointClick : MonoBehaviour
    {
        private Transform array;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public void OnMouseUp()
        {
            Debug.Log("鼠标按下");
            foreach (KeyValuePair<string, List<ElementItem>> item in MainManager.GetInstance().dicElements)
            {
                foreach (ElementItem itemValue in item.Value)
                {
                    if (itemValue.nowLevel == 1)
                    {
                        if (itemValue != (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).nowElement)
                        {
                            itemValue.SetUIByLevel(0);
                        }
                    }
                }
            }
            //if ((MainManager.GetInstance().dicMode[ModeType.area] as AreaManager).nowArea != null)
            //{
            //    (MainManager.GetInstance().dicMode[ModeType.area] as AreaManager).nowArea.SetUIByLevel(0);
            //}
            foreach (Transform item in MainManager.GetInstance().arrayLabelParent)
            {
                if (!item.gameObject.active 
                && !DynamicElement.GetInstance().isPlay
                && !DynamicElement.GetInstance().inPlay)
                {
                    array = item;
                }
            }
            if (transform.parent.parent.GetComponent<ElementItem>() != null 
                && MainManager.GetInstance().isClick
                && !DynamicElement.GetInstance().isPlay
                && !DynamicElement.GetInstance().inPlay)
            {
                transform.parent.parent.GetComponent<ElementItem>().SetUIByLevel(1);
                if (array != null)
                {
                    if (transform.parent.parent.GetComponent<ElementItem>().lineId != array.name)
                    {
                        array.gameObject.SetActive(true);
                    }
                }
            }
            if (transform.parent.parent.GetComponent<ElementArray>() != null
                && MainManager.GetInstance().isClick 
                && !DynamicElement.GetInstance().isPlay 
                && !DynamicElement.GetInstance().inPlay)
            {
                if (array != null)
                {
                    array.gameObject.SetActive(true);
                }
                transform.parent.parent.gameObject.SetActive(false);
                if (Vector3.Distance(transform.position, Camera.main.transform.position) > 1000 && MainManager.GetInstance().modeType != ModeType.single2D)
                {
                    Camera.main.GetComponent<BLCameraControl>().LookAtPosition3(transform.parent.parent.position);
                }
                transform.parent.parent.parent.GetComponent<ArrayParent>().WaitTime(transform.parent.parent.gameObject);
                (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).SceneToLevel(1);
            }
            if (transform.parent.parent.GetComponent<ElementArea>() != null
                && MainManager.GetInstance().isClick
                && !DynamicElement.GetInstance().isPlay
                && !DynamicElement.GetInstance().inPlay)
            {
                //transform.parent.parent.GetComponent<ElementArea>().SetUIByLevel(1);
            }
        }
    }
}