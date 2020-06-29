using BS.BL.Two.Element;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two.Item {
    public class AreaManager : MonoBehaviour,IMode
    {
        public Transform areaLabelParent;
        private Transform areaPa;
        private MainManager mainManager;
        public GameObject P_ElementArea;
        public GameObject areaItem;
        public Material matArea;
        public Dictionary<string, List<Transform>> dicArea = new Dictionary<string, List<Transform>>();
        private AllArea allArea;
        public ElementArea nowArea;
        void Start() {
            areaPa = GameObject.Find("AreaParent").transform;
            areaLabelParent = GameObject.Find("AreaLabelParent").transform;
            P_ElementArea = Resources.Load("Prefabs/Two/ElementArea") as GameObject;
            matArea = Resources.Load("Materails/LineMat3") as Material;
            areaItem = Resources.Load("Prefabs/Two/AreaItem") as GameObject;
            mainManager = MainManager.GetInstance();
            TEventSystem.Instance.EventManager.addEventListener(TEventType.DominantArea, SetDominatArea);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.SetAllArea, SetAllArea);
        }

        public void IInit()
        {
            
        }

        public void SetAllArea(TEvent tEvent) {
            allArea = JsonUtility.FromJson<AllArea>("{\"areas\":" + tEvent.eventParams[0].ToString() + "}");
            areaPa.position = mainManager.tviewBase.position;
            foreach (Area item in allArea.areas)
            {
                GameObject goArea = Instantiate(areaItem) as GameObject;
                goArea.name = item.id;
                goArea.transform.parent = areaPa;
                goArea.transform.localPosition = Vector3.zero;
                List<Transform> listTransTemp = new List<Transform>();
                foreach (string itemLineName in item.lineIds)
                {
                    foreach (Transform itemLine in mainManager.tviewBase.GetComponentsInChildren<Transform>())
                    {
                        if (itemLine.name == "Line_" + itemLineName)
                        {
                            Line lineItemTemp = itemLine.GetComponent<Line>();
                            GameObject goStart = new GameObject();
                            GameObject goEnd = new GameObject();
                            goStart.transform.parent = goArea.transform;
                            goEnd.transform.parent = goArea.transform;
                            goStart.transform.localPosition = lineItemTemp.startPoint.pos;
                            goEnd.transform.localPosition = lineItemTemp.endPoint.pos;
                            listTransTemp.Add(goStart.transform);
                            listTransTemp.Add(goEnd.transform);
                            goArea.GetComponent<PolygonDrawer>().listLines.Add(lineItemTemp.transform);
                        }
                    }
                }
                for (int i = 0; i < listTransTemp.Count; i++)
                {
                    if (i + 1 < listTransTemp.Count)
                    {
                        if (listTransTemp[i].position == listTransTemp[i + 1].position)
                        {
                            listTransTemp.RemoveAt(i);
                        }
                    }
                }
                if (listTransTemp.Count >= 2)
                {
                    Transform ts1 = listTransTemp[0];
                    Transform ts2 = listTransTemp[1];
                    List<Transform> listTransTemp1 = new List<Transform>();
                    listTransTemp.RemoveAt(0);
                    listTransTemp.RemoveAt(0);
                    if (listTransTemp.Count > 1)
                    {
                        listTransTemp.Sort(delegate (Transform x, Transform y)
                        {
                            return -MathTools.GetAxis(ts2.position, ts1.position, x.position).
                            CompareTo(MathTools.GetAxis(ts2.position, ts1.position, y.position));
                        });
                    }
                    listTransTemp1.Add(ts1);
                    listTransTemp1.Add(ts2);
                    foreach (Transform item2 in listTransTemp)
                    {
                        listTransTemp1.Add(item2);
                    }
                    Color nowColor = ColorToRGBA.GetColor(item.color);
                    Color newColor = new Color(nowColor.r, nowColor.g, nowColor.b, 0.9f);
                    if (item.areaType == "area")
                    {
                        goArea.GetComponent<PolygonDrawer>().Init(listTransTemp1, nowColor, item.id,item.areaType);
                        GameObject go = Instantiate(P_ElementArea) as GameObject;
                        go.transform.parent = areaLabelParent;
                        go.transform.position = NormalCenter.GetCenter(goArea.transform);
                        go.GetComponent<ElementArea>().Set(item.id);
                        go.GetComponent<ElementArea>().SetUIByLevel(1);
                        go.name = "Area_" + item.id;
                    }
                    else
                    {
                        GameObject go = Instantiate(P_ElementArea) as GameObject;
                        go.transform.parent = areaLabelParent;
                        go.transform.position = NormalCenter.GetCenter(goArea.GetComponent<PolygonDrawer>().listLines);
                        go.GetComponent<ElementArea>().Set(item.id);
                        go.GetComponent<ElementArea>().SetUIByLevel(1);
                        go.name = "Line_" + item.id;
                    }
                    foreach (Transform item3 in goArea.GetComponent<PolygonDrawer>().listLines)
                    {
                        Material matTemp = new Material(matArea);
                        if (item.areaType == "area")
                        {
                            matTemp.color = newColor;
                            goArea.GetComponent<MeshRenderer>().material = matTemp;
                        }
                        else
                        {
                            matTemp.color = nowColor;
                            item3.GetChild(0).GetComponent<MeshRenderer>().material = matTemp;
                        }
                    }
                }
            }
        }
        public void IQuit()
        {
            Debug.Log("区域退出");
            //foreach (Transform item in areaPa)
            //{
            //    Destroy(item.gameObject);
            //}
            //foreach (Transform itemLabel in areaLabelParent)
            //{
            //    Destroy(itemLabel.gameObject);
            //}
            //if (areaPa.GetComponent<PolygonDrawer>().listLines.Count > 0)
            //{
            //    foreach (Transform item in areaPa.GetComponent<PolygonDrawer>().listLines)
            //    {
            //        if (item.GetComponent<Line>() != null)
            //        {
            //            item.GetChild(0).GetComponent<MeshRenderer>().material = item.GetComponent<Line>().dataSetMat;
            //        }
            //    }
            //}
            //if (areaPa.GetComponent<MeshRenderer>() != null)
            //{
            //    Destroy(areaPa.GetComponent<MeshRenderer>());
            //    Destroy(areaPa.GetComponent<MeshCollider>());
            //    Destroy(areaPa.GetComponent<MeshFilter>());
            //}
            //areaPa.GetComponent<PolygonDrawer>().listLines.Clear();
        }

        public void IRun()
        {
        }
        public void SetDominatArea(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.allView)
            {
                if (!string.IsNullOrEmpty(tEvent.eventParams[0].ToString()))
                {
                    if (areaPa.Find(tEvent.eventParams[0].ToString()) != null)
                    {
                        if (areaPa.Find(tEvent.eventParams[0].ToString()).GetComponent<PolygonDrawer>().areaType == "area")
                        {
                            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis2(areaPa.Find(tEvent.eventParams[0].ToString()));
                        }
                        else
                        {
                            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis3(areaPa.Find(tEvent.eventParams[0].ToString()).GetComponent<PolygonDrawer>().listLines);
                        }
                    }
                    else
                    {
                        Debug.LogError("当前区域不存在");
                    }
                }
                //JObject area = JObject.Parse(tEvent.eventParams[0].ToString());
                //if (mainManager.dicAreaName.ContainsKey(area["eCode"].ToString()))
                //{
                //    IQuit();
                //    List<Transform> listTrans = new List<Transform>();
                //    PolygonDrawer polygonDrawer = areaPa.GetComponent<PolygonDrawer>();
                //    areaPa.position = mainManager.tviewBase.position;
                //    if (mainManager.dicAreaLines.ContainsKey(area["eCode"].ToString()))
                //    {
                //        foreach (Line item in mainManager.tviewBase.Find(area["eCode"].ToString()).GetComponentsInChildren<Line>())
                //        {
                //            GameObject goStart = new GameObject();
                //            GameObject goEnd = new GameObject();
                //            goStart.transform.parent = areaPa;
                //            goEnd.transform.parent = areaPa;
                //            goStart.transform.localPosition = item.startPoint.pos;
                //            goEnd.transform.localPosition = item.endPoint.pos;
                //            listTrans.Add(goStart.transform);
                //            listTrans.Add(goEnd.transform);
                //            polygonDrawer.listLines.Add(item.transform);
                //        }
                //    }
                //    if (listTrans.Count > 2)
                //    {
                //        Transform ts1 = listTrans[0];
                //        Transform ts2 = listTrans[1];
                //        List<Transform> listTransTemp = new List<Transform>();
                //        listTrans.RemoveAt(0);
                //        listTrans.RemoveAt(0);
                //        if (listTrans.Count > 1)
                //        {
                //            listTrans.Sort(delegate (Transform x, Transform y) {
                //                Debug.Log("x" + MathTools.GetAxis(ts2.position, ts1.position, x.position) + "y" + MathTools.GetAxis(ts2.position, ts1.position, y.position));
                //                return -MathTools.GetAxis(ts2.position, ts1.position, x.position).
                //                CompareTo(MathTools.GetAxis(ts2.position, ts1.position, y.position));
                //            });
                //        }
                //        listTransTemp.Add(ts1);
                //        listTransTemp.Add(ts2);
                //        foreach (Transform item in listTrans)
                //        {
                //            listTransTemp.Add(item);
                //        }
                //        Color nowColor = ColorToRGBA.GetColor(area["color"].ToString());
                //        Color newColor = new Color(nowColor.r, nowColor.g, nowColor.b, 0.3f);
                //        if (area["areaType"].ToString() == "area")
                //        {
                //            polygonDrawer.Init(listTransTemp, nowColor, area["eCode"].ToString());
                //            if (areaPa.GetComponent<MeshCollider>() == null)
                //            {
                //                areaPa.gameObject.AddComponent<MeshCollider>();
                //            }
                //            if (areaPa.childCount == 0)
                //            {
                //                return;
                //            }
                //            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis2(areaPa);
                //            GameObject go = Instantiate(P_ElementArea) as GameObject;
                //            go.transform.parent = areaLabelParent;
                //            go.transform.position = NormalCenter.GetCenter(areaPa);
                //            go.GetComponent<ElementArea>().Set(mainManager.dicAreaName[area["eCode"].ToString()]);
                //            go.name = area["eCode"].ToString();
                //        }
                //        else
                //        {
                //            GameObject go = Instantiate(P_ElementArea) as GameObject;
                //            go.transform.parent = areaLabelParent;
                //            go.transform.position = NormalCenter.GetCenter(polygonDrawer.listLines);
                //            go.GetComponent<ElementArea>().Set(mainManager.dicAreaName[area["eCode"].ToString()]);
                //            go.name = area["eCode"].ToString();
                //            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis3(polygonDrawer.listLines);
                //        }
                //        foreach (Transform item in polygonDrawer.listLines)
                //        {
                //            Material matTemp = new Material(matArea);
                //            if (area["areaType"].ToString() == "area")
                //            {
                //                matTemp.color = newColor;
                //            }
                //            else
                //            {
                //                matTemp.color = nowColor;
                //            }
                //            item.GetChild(0).GetComponent<MeshRenderer>().material = matTemp;
                //        }


                //        foreach (KeyValuePair<string, List<ElementItem>> itemElement in mainManager.dicAreaElements)
                //        {
                //            if (itemElement.Key != area["eCode"].ToString())
                //            {
                //                foreach (ElementItem itemChild in itemElement.Value)
                //                {
                //                    itemChild.gameObject.SetActive(false);
                //                }
                //            }
                //            else
                //            {
                //                foreach (ElementItem itemChild in itemElement.Value)
                //                {
                //                    itemChild.gameObject.SetActive(true);
                //                }
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    Debug.Log("当前区域不存在");
                //}

            }
            else
            {
                Debug.Log("请先设置综合监控模式");
            }

        }
    }
    #region 区域解析
    [Serializable]
    public class AllArea
    {
        public List<Area> areas;
    }
    [Serializable]
    public class Area {
        public string id;
        public string color;
        public string areaType;
        public List<string> lineIds;
    }
    #endregion
}

