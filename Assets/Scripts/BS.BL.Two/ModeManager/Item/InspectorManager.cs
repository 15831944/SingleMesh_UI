using System.Collections;
using System.Collections.Generic;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two.Item
{
    public class InspectorManager : MonoBehaviour, IMode
    {
        private MainManager mainManager;
        private Transform inspectionPa;
        private bool startInspection = false;
        public void IInit()
        {
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
            mainManager = MainManager.GetInstance();
            inspectionPa = mainManager.tviewBase.Find("InspectionPa");
            TEventSystem.Instance.EventManager.addEventListener(TEventType.Inspector, SetInspection);
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// 设置巡检
        /// </summary>
        /// <param name="tEvent"></param>
        public void SetInspection(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.inspector && !string.IsNullOrEmpty(tEvent.eventParams[0].ToString()))
            {
                tEvent.eventParams[0] = "{\"insp\":" + tEvent.eventParams[0].ToString() + "}";
                Inspection _inspaction = JsonUtility.FromJson<Inspection>(tEvent.eventParams[0].ToString());
                if (_inspaction.insp.Count > 0)
                {
                    foreach (Transform item in inspectionPa)
                    {
                        DestroyImmediate(item.gameObject);
                    }
                    for (int i = 0; i < _inspaction.insp.Count; i++)
                    {
                        GameObject goInspaction = new GameObject();
                        goInspaction.transform.parent = inspectionPa;
                        Line line = mainManager.tviewBase.Find(mainManager.dicLines[_inspaction.insp[i].lineId].a_eCode).Find("Line_" + _inspaction.insp[i].lineId).GetComponent<Line>();
                        Vector3 vFor = (line.endPoint.pos - line.startPoint.pos).normalized;
                        Vector3 vec = line.startPoint.pos + vFor * _inspaction.insp[i].distance;
                        goInspaction.transform.localPosition = vec;
                        goInspaction.name = "Inspection_" + i.ToString();
                    }
                    startInspection = true;
                    InvokeRepeating("InspectionUpdate", 1, 2);
                }
                else
                {
                    InspactionEdit();
                }
            }
            else
            {
                Debug.Log("请设置巡检模式再进行巡检");
            }
        }
        private void InspactionEdit()
        {
            Debug.Log("直接进入巡览编辑模式");
            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(mainManager.tviewBase.transform);
            mainManager.editTip.SetActive(true);
            mainManager.editWay = true;
        }
    }
}
