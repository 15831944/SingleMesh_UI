using DG.Tweening;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two.Item
{
    public class WayLineManager : MonoBehaviour,IMode
    {
        private MainManager mainManager;
        private GameObject wayGo;
        private Transform wayPa;
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
            wayPa = mainManager.tviewBase.Find("WayPa");
            wayGo = Resources.Load("Prefabs/Two/arrow") as GameObject;

            TEventSystem.Instance.EventManager.addEventListener(TEventType.SetWay, SetWays);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.DeleteWayPoint, DeletePoint);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void SetWays(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.wayLine)
            {
                if (!string.IsNullOrEmpty(tEvent.eventParams[0].ToString()))
                {
                    JObject waysTemp = JObject.Parse(tEvent.eventParams[0].ToString());
                    WayData wayData = JsonUtility.FromJson<WayData>(waysTemp["ways"].ToString());
                    if (!string.IsNullOrEmpty(wayData.id))
                    {
                        if (wayPa.childCount > 0)
                        {
                            foreach (Transform item in wayPa)
                            {
                                DestroyImmediate(item.gameObject);
                            }
                        }
                        mainManager.editWay = false;
                        mainManager.editTip.SetActive(false);
                        Transform wayTypePa = null;
                        if (wayPa.Find(wayData.type.ToString()) == null)
                        {
                            wayTypePa = (new GameObject()).transform;
                            wayTypePa.parent = wayPa;
                            wayTypePa.name = wayData.type.ToString();
                            wayTypePa.localPosition = Vector3.zero;
                        }
                        else
                        {
                            wayTypePa = wayPa.Find(wayData.type.ToString());
                        }
                        Color newColor = new Color(0, 1, 0, 0.3f);
                        if (!string.IsNullOrEmpty(wayData.color))
                        {
                            Color nowColor = ColorToRGBA.GetColor(wayData.color);
                            newColor = new Color(nowColor.r, nowColor.g, nowColor.b, 0.3f);
                        }
                        for (int i = 0; i < wayData.points.Count - 1; i++)
                        {
                            Line line1 = mainManager.tviewBase.Find(mainManager.dicLines[wayData.points[i].lineId].a_eCode).Find("Line_" + wayData.points[i].lineId).GetComponent<Line>();
                            Vector3 vFor1 = (line1.endPoint.pos - line1.startPoint.pos).normalized;
                            Vector3 vec1 = line1.endPoint.pos - vFor1 * wayData.points[i].distance;
                            Line line2 = mainManager.tviewBase.Find(mainManager.dicLines[wayData.points[i + 1].lineId].a_eCode).Find("Line_" + wayData.points[i + 1].lineId).GetComponent<Line>();
                            Vector3 vFor2 = (line2.endPoint.pos - line2.startPoint.pos).normalized;
                            Vector3 vec2 = line2.endPoint.pos - vFor2 * wayData.points[i + 1].distance;
                            Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.forward, vec2 - vec1).eulerAngles;
                            GameObject goWay = Instantiate(wayGo) as GameObject;
                            goWay.transform.parent = wayTypePa;
                            goWay.transform.localPosition = (vec1 + vec2) / 2;
                            goWay.transform.localEulerAngles = eulerAngles + new Vector3(0, 90, 0);
                            goWay.transform.localScale = new Vector3(Vector3.Distance(vec2, vec1) / 10, 1, 1);
                            goWay.GetComponent<MeshRenderer>().material = new Material(goWay.GetComponent<MeshRenderer>().material);
                            goWay.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(vec2, vec1) / 10, 1));
                            DOTween.To(() => goWay.GetComponent<MeshRenderer>().material.mainTextureOffset, x =>
                            goWay.GetComponent<MeshRenderer>().material.mainTextureOffset = x, new Vector2(-1, 0), 1).SetLoops(-1).SetEase(Ease.Linear);
                            goWay.GetComponent<MeshRenderer>().material.color = newColor;
                        }
                        if (wayTypePa.childCount > 0)
                        {
                            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(wayTypePa);
                        }
                        else
                        {
                            Debug.Log("直接进入编辑模式，此编辑模式不分路线类型");
                            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(mainManager.tviewBase);
                            mainManager.editTip.SetActive(true);
                            mainManager.editWay = true;
                        }
                    }
                    else
                    {
                        Debug.Log("直接进入编辑模式，此编辑模式不分路线类型");
                        Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(mainManager.tviewBase);
                        mainManager.editTip.SetActive(true);
                        mainManager.editWay = true;
                    }
                }
                else
                {
                    Debug.Log("直接进入编辑模式，此编辑模式不分路线类型");
                    Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(mainManager.tviewBase);
                    mainManager.editTip.SetActive(true);
                    mainManager.editWay = true;
                }
            }
            else
            {
                Debug.Log("请先进入路线模式");
            }

        }
        /// <summary>
        /// 通过数据删除点位
        /// </summary>
        /// <param name="pointMsg"></param>
        public void DeletePoint(TEvent tEvent)
        {
            if (mainManager.editWay)
            {
                if (!string.IsNullOrEmpty(tEvent.eventParams[0].ToString()))
                {
                    Points points = JsonUtility.FromJson<Points>(tEvent.eventParams[0].ToString());
                    if (wayPa.Find("point_" + points.lineId + "_" + points.distance) != null)
                    {
                        wayPa.GetComponent<WayPoints>().dicPoints.Remove("point_" + points.lineId + "_" + points.distance);
                        DestroyImmediate(wayPa.Find("point_" + points.lineId + "_" + points.distance).gameObject);
                    }
                    else
                    {
                        Debug.Log("当前点位不存在");
                    }
                }
            }
            else
            {
                Debug.Log("请进入编辑模式");
            }
        }
    }
}
