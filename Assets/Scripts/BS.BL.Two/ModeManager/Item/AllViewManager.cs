using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BS.BL.Two.Element;
using DG.Tweening;
using BS.BL.Interface;
using TFramework.EventSystem;
using TFramework.ApplicationLevel;
using System;

namespace BS.BL.Two.Item
{
    public class AllViewManager : MonoBehaviour,IMode
    {
        public GameObject P_ElementItem;
        private ModeType _modeType;
        private Transform lineLabelParent;
        private MainManager mainManager;
        private List<string> listCodes = new List<string>();//资源实体编号
        public List<Tweener> moveTr = new List<Tweener>(), moveTr2 = new List<Tweener>();

        public ElementItem nowElement;
        public GameObject arrayGo;
        public int level = 0;
        void Start() {
            mainManager = MainManager.GetInstance();
            lineLabelParent = mainManager.lineLabelParent;
            P_ElementItem = Resources.Load("Prefabs/Two/ElementItem") as GameObject;
            TEventSystem.Instance.EventManager.addEventListener(TEventType.RefreshRes, RefreshRes);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.DominantRes, SetDominantRes);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.DominantResType, SetDominantResByType);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.DominantTunnel, SetDominatTunnel);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.ShowGround, ShowGround);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.AdjustRange, AdjustRange);
        }
        public void IInit()
        {
            foreach (Transform item in mainManager.lineLabelParent)
            {
                item.gameObject.SetActive(true);
                foreach (Transform itemChild in item)
                {
                    itemChild.transform.localScale = new Vector3(Mathf.Abs( itemChild.transform.localScale.x), itemChild.transform.localScale.y, itemChild.transform.localScale.z);
                }
            }
            foreach (Transform itemArray in mainManager.arrayLabelParent)
            {
                itemArray.gameObject.SetActive(true);
                itemArray.transform.localScale = new Vector3(Mathf.Abs(itemArray.transform.localScale.x), itemArray.transform.localScale.y, itemArray.transform.localScale.z);
            }
            foreach (Transform item in (MainManager.GetInstance().dicMode[ModeType.area] as AreaManager).areaLabelParent)
            {
                //item.transform.forward = -Vector3.up;
                item.transform.localScale = new Vector3(Mathf.Abs(item.transform.localScale.x), item.transform.localScale.y, item.transform.localScale.z);
            }
            foreach (Transform item in mainManager.tviewBase.Find("Layer_HD/Nodes"))
            {
                if (item.GetComponent<Node>() != null)
                {
                    item.GetComponent<Node>().Select(false);
                }
            }
            MainManager.GetInstance().ShowOrHideLabel();
        }
        /// <summary>
        /// 初始化资源实体
        /// </summary>
        /// <param name="result"></param>
        public void RefreshRes(TEvent tEvent)
        {
            _modeType = MainManager.GetInstance().modeType;
            if (_modeType == ModeType.allView || _modeType == ModeType.tunnelDegree || _modeType == ModeType.area)
            {
                transform.position = MainManager.GetInstance().tviewBase.position;
                JObject resEntityList = JObject.Parse(tEvent.eventParams[0].ToString());
                //listType.Clear();
                foreach (JObject item in resEntityList["entity"])
                {
                    Entity entity = JsonUtility.FromJson<Entity>(item.ToString());
                    if (entity.a_type == 0)//地下资源
                    {
                        mainManager.RefreshAlarm(entity);//刷新报警
                        mainManager.RefreshEntity(entity);//刷新资源实体
                        if (entity.exist == 0)//添加
                        {
                            if (!string.IsNullOrEmpty(entity.position.lineId))
                            {
                                if (mainManager.dicLines.ContainsKey(entity.position.lineId))
                                {
                                    Line line = mainManager.tviewBase.Find(mainManager.dicLines[entity.position.lineId].a_eCode + "/Line_"
                                        + entity.position.lineId).GetComponent<Line>();
                                    Vector3 vFor = (line.endPoint.pos - line.startPoint.pos).normalized;
                                    if (entity.position.distance <= line.lineLenght)
                                    {
                                        //AddEntity(entity, line, vFor);
                                        GameObject goItem = null;
                                        if (listCodes.Contains(entity.id))
                                        {
                                            goItem = lineLabelParent.Find(entity.t_eCode + "/" + entity.id).gameObject;
                                        }
                                        else
                                        {
                                            goItem = Instantiate(P_ElementItem) as GameObject;
                                        }
                                        Transform goItemPa = null;
                                        if (mainManager.dicElements.ContainsKey(entity.t_eCode))
                                        {
                                            if (!mainManager.dicElements[entity.t_eCode].Contains(goItem.GetComponent<ElementItem>()))
                                            {
                                                goItemPa = lineLabelParent.Find(entity.t_eCode);
                                                goItem.transform.parent = goItemPa;
                                                goItem.name = entity.id;
                                                //if (entity.position.content.Count == 0)
                                                //{
                                                    goItem.transform.localPosition = line.startPoint.pos + vFor * entity.position.distance;
                                                //}
                                                mainManager.dicElements[entity.t_eCode].Add(goItem.GetComponent<ElementItem>());
                                            }
                                        }
                                        else
                                        {
                                            GameObject goPa = new GameObject();
                                            goPa.transform.parent = lineLabelParent;
                                            goPa.name = entity.t_eCode;
                                            goPa.transform.localPosition = Vector3.zero;
                                            goItemPa = goPa.transform;
                                            goItem.transform.parent = goItemPa;
                                            goItem.name = entity.id;
                                            goItem.transform.localPosition = line.startPoint.pos + vFor * entity.position.distance;
                                            List<ElementItem> elementItems = new List<ElementItem>();
                                            elementItems.Add(goItem.GetComponent<ElementItem>());
                                            mainManager.dicElements.Add(entity.t_eCode, elementItems);

                                        }
                                        //刷新
                                        if (entity.position.content.Count == 0)//跳点移动
                                        {
                                            if (!string.IsNullOrEmpty(goItem.GetComponent<ElementItem>().lineId))
                                            {
                                                if (entity.position.lineId == goItem.GetComponent<ElementItem>().lineId)
                                                {
                                                    moveTr2.Add(goItem.transform.DOLocalMove(line.startPoint.pos + vFor * entity.position.distance, 1).SetEase(Ease.Linear).SetAutoKill(true));
                                                }
                                                else
                                                {
                                                    goItem.transform.localPosition = line.startPoint.pos + vFor * entity.position.distance;
                                                }
                                            }
                                        }
                                        else//按路径移动
                                        {
                                            List<Vector3> listVectorTmep = new List<Vector3>();
                                            foreach (PositionContent pos in entity.position.content)
                                            {
                                                Line lineTemp = GameObject.Find("Line_" + pos.lineId).GetComponent<Line>();
                                                Vector3 vForTemp = (lineTemp.endPoint.pos - lineTemp.startPoint.pos).normalized;
                                                if (pos.distance < lineTemp.lineLenght)
                                                {
                                                    listVectorTmep.Add(lineTemp.endPoint.pos - vForTemp * pos.distance);
                                                }
                                                else
                                                {
                                                    Debug.Log("刷新资源中间点，" + entity.id + "资源超出" + lineTemp.id + "巷道的长度范围");
                                                }
                                            }
                                            Line lineOver = GameObject.Find("Line_" + entity.position.lineId).GetComponent<Line>();
                                            Vector3 vForOver = (lineOver.endPoint.pos - lineOver.startPoint.pos).normalized;
                                            Vector3 posOver = lineOver.startPoint.pos + vForOver * entity.position.distance;
                                            DynamicResMove(goItem.transform, listVectorTmep, posOver, 1f / entity.position.content.Count);
                                        }
                                        goItem.GetComponent<ElementItem>().Set(entity, true);
                                        mainManager.RefreshAreaElements(entity, goItem.GetComponent<ElementItem>());//刷新区域资源实体
                                        mainManager.RefreshLineElements(entity, goItem.transform);//刷新线段资源实体
                                        //框选功能
                                        if (!Camera.main.GetComponent<DrawRectangle_ZH>()._Characters.Contains(goItem))
                                        {
                                            Camera.main.GetComponent<DrawRectangle_ZH>()._Characters.Add(goItem);
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("添加/刷新资源时，" + entity.id + "资源超出" + line.id + "巷道的长度范围");
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError("请配置巷道");
                            }
                        }
                    }
                    else if (int.Parse(item["a_type"].ToString()) == 1)//地上资源
                    {
                        GameObject goItem = Instantiate(P_ElementItem) as GameObject;
                    }
                    else
                    {
                        Debug.Log(item["id"] + "资源所属地上地下划分错误");
                    }
                }
                mainManager.ShowOrHideLabel();
                listCodes.Clear();
                foreach (Transform item in lineLabelParent.GetComponentsInChildren<Transform>(true))
                {
                    if (!mainManager.dicElements.ContainsKey(item.name) && item.name != "LineLabelParent")
                    {
                        listCodes.Add(item.name);
                    }
                }
            }
            else
            {
                Debug.Log("请进入综合监控");
            }

            Resources.UnloadUnusedAssets();
        }
        
        /// <summary>
        /// 资源实体移动动画
        /// </summary>
        /// <param name="entityTrans"></param>
        /// <param name="vector3s"></param>
        /// <param name="overPos"></param>
        /// <param name="timeTemp"></param>
        /// <param name="indexTemp"></param>
        private void DynamicResMove(Transform entityTrans, List<Vector3> vector3s, Vector3 overPos, float timeTemp, int indexTemp = 0)
        {
            if (vector3s.Count > 0)
            {
                if (entityTrans != null)
                {
                    moveTr.Add(entityTrans.DOLocalMove(vector3s[indexTemp], timeTemp).SetEase(Ease.Linear).OnComplete(delegate ()
                    {
                        if (indexTemp < vector3s.Count - 1)
                        {
                            indexTemp++;
                            DynamicResMove(entityTrans, vector3s, overPos, timeTemp, indexTemp);
                        }
                        else
                        {
                            moveTr.Add(entityTrans.DOLocalMove(overPos, timeTemp).SetAutoKill(true));
                        }
                    }).SetAutoKill(true));
                }
            }
        }
        /// <summary>
        /// 通过资源ID设置当前资源
        /// </summary>
        /// <param name="resId"></param>
        public void SetDominantRes(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.allView || mainManager.modeType == ModeType.tunnelDegree || mainManager.modeType == ModeType.area)
            {
                JObject res = JObject.Parse(tEvent.eventParams[0].ToString());
                if (lineLabelParent.Find(res["t_eCode"].ToString() + "/" + res["resId"].ToString()) != null)
                {
                    if (mainManager.arrayLabelParent.Find(lineLabelParent.Find(res["t_eCode"].ToString() + "/" + res["resId"].ToString()).
                        GetComponent<ElementItem>().lineId) != null)
                    {
                        ElementArray _elementArray = mainManager.arrayLabelParent.Find(lineLabelParent.Find(res["t_eCode"].ToString() + "/" + res["resId"].ToString()).
                        GetComponent<ElementItem>().lineId).GetComponent<ElementArray>();
                        if (_elementArray != null)
                        {
                            _elementArray.gameObject.SetActive(false);
                        }
                    }
                    Transform tsTemp = lineLabelParent.Find(res["t_eCode"].ToString() + "/" + res["resId"].ToString());
                    Camera.main.GetComponent<BLCameraControl>().LookAtPosition(tsTemp.position);
                    tsTemp.GetComponent<ElementItem>().SetUIByLevel(1);
                }
            }
            else
            {
                Debug.Log("非综合监控模式下选择资源无效");
            }

        }
       
        public List<string> listType = new List<string>();
        /// <summary>
        /// 通过资源类型显示资源，支持多选
        /// </summary>
        /// <param name="tEvent"></param>
        public void SetDominantResByType(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.allView || mainManager.modeType == ModeType.tunnelDegree || mainManager.modeType == ModeType.area)
            {
                tEvent.eventParams[0] = "{ \"list\": " + tEvent.eventParams[0].ToString() + "}";
                ResTypes resTypesTemp = JsonUtility.FromJson<ResTypes>(tEvent.eventParams[0].ToString());
                listType.Clear();
                try
                {
                    foreach (Transform item in lineLabelParent)
                    {
                        item.gameObject.SetActive(false);
                    }
                    foreach (string itemName in resTypesTemp.list)
                    {
                        if (lineLabelParent.Find(itemName) != null)
                        {
                            if (!listType.Contains(itemName))
                            {
                                listType.Add(itemName);
                            }
                            lineLabelParent.Find(itemName).gameObject.SetActive(true);
                        }
                    }
                    mainManager.ShowOrHideLabel();
                }
                catch (Exception E)
                {
                    Debug.Log("按类型显示错误" + E);
                }
            }
            else
            {
                Debug.Log("非综合监控模式下选择资源类型无效");
            }
        }
        /// <summary>
        /// 设置主航道
        /// </summary>
        /// <param name="tEvent"></param>
        public void SetDominatTunnel(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.allView)
            {
                try
                {
                    foreach (Transform item in mainManager.tviewBase.GetComponentsInChildren<Transform>())
                    {
                        if (item.name == "Line_" + tEvent.eventParams[0].ToString())
                        {
                            Camera.main.GetComponent<BLCameraControl>().LookAtPosition(item.position);
                        }
                    }
                }
                catch
                {
                    Debug.Log("不存在此ID" + tEvent.eventParams[0].ToString());
                }
            }
            else
            {
                Debug.Log("非综合监控模式下选择巷道无效");
            }
        }
        /// <summary>
        /// 显示地上
        /// </summary>
        /// <param name="tEvent"></param>
        public void ShowGround(TEvent tEvent)
        {
            //BL.Manager.MainManager.GetInstance().overground.transform.position = mainManager.tviewBase.position;
            string[] ground = tEvent.eventParams[0].ToString().Split('+');
            if (ground[0] == "1")
            {
                BL.Manager.MainManager.GetInstance().overground.SetActive(true);
                mainManager.tviewBase.gameObject.SetActive(false);
                mainManager.SetResActive(false);
                Camera.main.GetComponent<BLCameraControl>().LookAtUpGround3(BL.Manager.MainManager.GetInstance().overground.transform);
            }
            if (ground[0] == "0")
            {
                foreach (KeyValuePair<string, List<ElementItem>> item in mainManager.dicElements)
                {
                    foreach (ElementItem itemChild in item.Value)
                    {
                        itemChild.gameObject.SetActive(true);
                    }
                }
                foreach (KeyValuePair<string, List<Transform>> item in mainManager.dicAreaLines)
                {
                    foreach (Transform itemGo in item.Value)
                    {
                        itemGo.GetChild(0).GetComponent<MeshRenderer>().material = itemGo.GetComponent<Line>().dataSetMat;
                    }
                }
                mainManager.SetResActive(true);
                mainManager.tviewBase.gameObject.SetActive(true);
                BL.Manager.MainManager.GetInstance().overground.SetActive(false);
                Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(mainManager.tviewBase);
            }
            if (ground.Length > 1)
            {
                if (ground[1] == "1")
                {
                    BL.Manager.MainManager.GetInstance().overground.SetActive(true);
                }
                if (ground[1] == "0")
                {
                    mainManager.tviewBase.gameObject.SetActive(true);
                }
            }
        }

        public void SceneToLevel(int _level)
        {
            StartCoroutine(WaitSomeTime(_level));
        }
        private IEnumerator WaitSomeTime(int _level)
        {
            yield return new WaitForSeconds(0.5f);
            level = _level;
            if (_level == 1)
            {
                nowElement = null;
            }
            if (_level == 0)
            {
                arrayGo = null;
            }
        }
        /// <summary>
        /// 重置
        /// </summary>
        /// <param name="zhankai"></param>
        public void ResetLabel(bool zhankai = false)
        {
            if (nowElement != null)
            {
                foreach (KeyValuePair<string, List<ElementItem>> item in mainManager.dicElements)
                {
                    foreach (ElementItem itemValue in item.Value)
                    {
                        if (itemValue.nowLevel == 1)
                        {
                            if (itemValue != nowElement)
                            {
                                itemValue.SetUIByLevel(0);
                            }
                        }
                    }
                }
                if (DynamicElement.GetInstance().exist)
                {
                    nowElement.SetUIByLevel(0, zhankai);
                }
                else
                {
                    Destroy(nowElement.gameObject);
                }
                SceneToLevel(1);
            }
        }
        public void ResetArray()
        {
            if (arrayGo != null && nowElement == null)
            {
                arrayGo.SetActive(true);
                SceneToLevel(0);
            }
        }
        /// <summary>
        /// 调节告警范围
        /// </summary>
        /// <param name="rate"></param>
        public void AdjustRange(TEvent tEvent)
        {
            if (nowElement != null)
            {
                if (nowElement.pian.activeInHierarchy)
                {
                    nowElement.ChangePianRadius(int.Parse(tEvent.eventParams[0].ToString()));
                }
            }
        }
        public void IQuit()
        {

        }

        public void IRun()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
