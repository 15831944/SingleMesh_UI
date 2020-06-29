using BS.BL.Two.Element;
using BS.BL.Two.Item;
using BS.BL.Two.Loader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two {
    public enum ModeType{
        allView,//综合监控
        inspector,//巡检模式
        wayLine,//路线模式
        single2D,//2D模式
        tunnelDegree,//综合监控下的掘金进度模式
        area,//综合监控下的区域选择模式
        track,//轨迹回放模式
    }
    public class MainManager : MonoBehaviour
    {
        private static MainManager instance;
        public static MainManager GetInstance() {
            if (instance == null)
            {
                instance = new MainManager();
            }
            return instance;
        }
        public ModeType modeType = ModeType.allView;//当前模式
        public Dictionary<ModeType, IMode> dicMode = new Dictionary<ModeType, IMode>();
        public Transform tviewBase;
        public LoadConfig loadConfig;
        public Transform lineLabelParent;//三位标签的父物体
        public Transform arrayLabelParent;//合并标签的父物体

        public GameObject P_ElementArray;//合并标签的实例化物体

        public GameObject editTip;
        public bool editWay = false;//路径编辑
        public bool isCurrent = false;//框选

        public bool isClick = true;

        public Material matLine, matNode;
        public Transform cameraStartTrans;

        public GameObject chuasong;

        public bool arrayActive = true;
        private void Awake()
        {
            instance = this;
            matLine = Resources.Load("Materails/LineMat") as Material;
            matNode = Resources.Load("Materails/NodeMat") as Material;
            chuasong = Resources.Load("Prefabs/Two/arrowU") as GameObject;
            InitMode();
            InitAllListener();
            //TEventSystem.Instance.EventManager.dispatchEvent(new TEvent(TEventType.DominantMode, 0), this);
        }
        private void Update()
        {
            if (!Utility.IsPointerOverUIObject())
            {
                if (RayTools.GetArrayIndex() == 3)
                {

                    if ((dicMode[ModeType.allView] as AllViewManager).level == 2 && !DynamicElement.GetInstance().isPlay && !DynamicElement.GetInstance().inPlay)
                    {
                        (dicMode[ModeType.allView] as AllViewManager).ResetLabel();
                    }
                    if ((dicMode[ModeType.allView] as AllViewManager).level == 1 && !DynamicElement.GetInstance().isPlay && !DynamicElement.GetInstance().inPlay)
                    {
                        (dicMode[ModeType.allView] as AllViewManager).ResetArray();
                    }
                    if ((dicMode[ModeType.area] as AreaManager).nowArea != null)
                    {
                        //(dicMode[ModeType.area] as AreaManager).nowArea.SetUIByLevel(0);
                    }
                }
            }
        }
        #region 初始化方法
        private void InitMode() {
            dicMode.Add(ModeType.allView, gameObject.AddComponent<AllViewManager>());
            dicMode.Add(ModeType.area, gameObject.AddComponent<AreaManager>());
            dicMode.Add(ModeType.inspector, gameObject.AddComponent<InspectorManager>());
            dicMode.Add(ModeType.single2D, gameObject.AddComponent<Single2DManager>());
            dicMode.Add(ModeType.tunnelDegree, gameObject.AddComponent<TunnelDegreeManager>());
            dicMode.Add(ModeType.wayLine, gameObject.AddComponent<WayLineManager>());
            dicMode.Add(ModeType.track, gameObject.AddComponent<TrackManager>());
            tviewBase = GameObject.Find("TViewBase").transform;
            P_ElementArray = Resources.Load("Prefabs/Two/ElementArray") as GameObject;
            loadConfig = new LoadConfig();
        }
        /// <summary>
        /// 添加监听事件
        /// </summary>
        private void InitAllListener() {
            TEventSystem.Instance.EventManager.addEventListener(TEventType.DominantMode, setDominantMode);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.InitData, InitData);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.Current, CurrentRes);
        }
        private void ReStartNormal(ModeType _modeType) {
            listType.Clear();
            foreach (Transform item in lineLabelParent)
            {
                if (!listType.Contains(item.name))
                {
                    listType.Add(item.name);
                }
            }
            foreach (KeyValuePair<string, List<ElementItem>> item in dicElements)
            {
                foreach (ElementItem itemChild in item.Value)
                {
                    itemChild.gameObject.SetActive(true);
                }
            }
            foreach (KeyValuePair<string, List<Transform>> item in dicAreaLines)
            {
                foreach (Transform itemGo in item.Value)
                {
                    itemGo.GetChild(0).GetComponent<MeshRenderer>().material = itemGo.GetComponent<Line>().dataSetMat;
                }
            }
            foreach (KeyValuePair<ModeType,IMode> item in dicMode)
            {
                if (item.Key != _modeType)
                {
                    item.Value.IQuit();
                }
            }
            Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(tviewBase, true);
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="nEvent"></param>
        private void InitData(TEvent nEvent) {
            if (!string.IsNullOrEmpty(nEvent.eventParams[0].ToString()))
            {
                JObject basicData = JObject.Parse(nEvent.eventParams[0].ToString());
                //巷道的线
                foreach (JObject itemLine in basicData["lines"])
                {
                    LineData lineData = JsonUtility.FromJson<LineData>(itemLine.ToString());
                    Transform trLine = tviewBase.transform.Find("Layer_HD/Lines/Line_" + lineData.id);
                    if (trLine != null)
                    {
                        //添加区域的组
                        string _areaName = SetName(lineData.a_eCode, "other");
                        if (dicAreaLines.ContainsKey(_areaName))
                        {
                            dicAreaLines[_areaName].Add(trLine);
                            trLine.SetParent(tviewBase.Find(_areaName));
                        }
                        else
                        {
                            GameObject goArea = null;
                            if (tviewBase.Find(_areaName) == null)
                            {
                                goArea = new GameObject();
                                goArea.name = _areaName;
                                goArea.transform.SetParent(tviewBase);
                                goArea.transform.localPosition = Vector3.zero;
                            }
                            else
                            {
                                goArea = tviewBase.Find(_areaName).gameObject;
                            }
                            List<Transform> listGoTemp = new List<Transform>();
                            listGoTemp.Add(trLine);
                            dicAreaLines.Add(_areaName, listGoTemp);
                            trLine.SetParent(goArea.transform);
                        }
                        //添加巷道名称的组
                        string _lineName = SetName(lineData.l_eCode, "noName");
                        if (dicNameLines.ContainsKey(_lineName))
                        {
                            dicNameLines[_lineName].Add(trLine);
                        }
                        else
                        {
                            List<Transform> listGoTemp = new List<Transform>();
                            listGoTemp.Add(trLine);
                            dicNameLines.Add(_lineName, listGoTemp);
                        }
                        if (dicLines.ContainsKey(lineData.id))
                        {
                            if (string.IsNullOrEmpty(lineData.a_eCode))
                            {
                                lineData.a_eCode = "other";
                            }
                            dicLines[lineData.id] = lineData;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(lineData.a_eCode))
                            {
                                lineData.a_eCode = "other";
                            }
                            dicLines.Add(lineData.id, lineData);
                        }
                    }
                }
                //路径
                foreach (JObject itemWay in basicData["ways"])
                {
                    WayData wayData = JsonUtility.FromJson<WayData>(itemWay.ToString());
                    if (dicWays.ContainsKey(wayData.type))
                    {
                        dicWays[wayData.type].Add(wayData);
                    }
                    else
                    {
                        List<WayData> wayDatas = new List<WayData>();
                        wayDatas.Add(wayData);
                        dicWays.Add(wayData.type, wayDatas);
                    }
                }
                //资源类型
                ConfigData configData = JsonUtility.FromJson<ConfigData>(basicData["config"].ToString());
                foreach (ConfigItem itemArea in configData.area)
                {
                    if (!dicAreaName.ContainsKey(itemArea.eCode))
                    {
                        dicAreaName.Add(itemArea.eCode, itemArea.name);
                    }
                }
                foreach (ConfigItem itemLine in configData.line)
                {
                    if (!dicLineName.ContainsKey(itemLine.eCode))
                    {
                        dicLineName.Add(itemLine.eCode, itemLine.name);
                    }
                }
                foreach (ConfigType itemType in configData.eType)
                {
                    StartCoroutine(loadConfig.LoadPhoto(diceTypeSprites, itemType));
                    if (!diceType.ContainsKey(itemType.eCode))
                    {
                        diceType.Add(itemType.eCode, itemType);
                    }
                }
                //OverGround overGround = JsonUtility.FromJson<OverGround>(basicData["overground"].ToString());
                //if (!string.IsNullOrEmpty(overGround.intoWell))
                //{
                //    if (GameObject.Find("Line_" + overGround.intoWell))
                //    {
                //        Line lineTemp = GameObject.Find("Line_" + overGround.intoWell).GetComponent<Line>();
                //        startPos = tviewBase.transform.TransformPoint(lineTemp.startPoint.pos);
                //    }
                //}
                //if (overGround.resData.Count > 0)
                //{
                //    foreach (Data item in overGround.resData)
                //    {
                //        if (!dicOverGroundRes.ContainsKey(item.resId))
                //        {
                //            dicOverGroundRes.Add(item.resId, item.resType);
                //        }
                //    }
                //}
                //if (BL.Manager.MainManager.GetInstance().overground != null)
                //{
                //    if (startPos != Vector3.zero)
                //    {
                //        BL.Manager.MainManager.GetInstance().overground.transform.position = startPos;
                //    }
                //    foreach (Transform item in BL.Manager.MainManager.GetInstance().overground.GetComponentsInChildren<Transform>(false))
                //    {
                //        //Debug.LogError(item.name);
                //        if (dicOverGroundRes.ContainsKey(item.name))
                //        {
                //            if (dicOverGroundRes[item.name] == 2) //传送带
                //            {
                //                GameObject goWay = Instantiate(chuasong) as GameObject;
                //                goWay.transform.parent = item;
                //                goWay.transform.localPosition = Vector3.zero;
                //                goWay.transform.eulerAngles = item.eulerAngles + new Vector3(0, 90, 0);
                //                //goWay.transform.localScale = new Vector3(Vector3.Distance(vec2, vec1) / 10, 1, 1);
                //                //goWay.GetComponent<MeshRenderer>().material = new Material(goWay.GetComponent<MeshRenderer>().material);
                //                //goWay.GetComponent<MeshRenderer>().material.SetTextureScale("_MainTex", new Vector2(Vector3.Distance(vec2, vec1) / 10, 1));
                //                //DOTween.To(() => goWay.GetComponent<MeshRenderer>().material.mainTextureOffset, x =>
                //                //goWay.GetComponent<MeshRenderer>().material.mainTextureOffset = x, new Vector2(-1, 0), 1).SetLoops(-1).SetEase(Ease.Linear);
                //                //goWay.GetComponent<MeshRenderer>().material.color = newColor;
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    Debug.LogError("请平直地上模型");
                //}
                
            }
        }
        /// <summary>
        /// 通过返回的code设置名称，如果返回为空则自定义命名
        /// </summary>
        /// <param name="code"></param>
        /// <param name="noCode"></param>
        /// <returns></returns>
        private string SetName(string code, string noCode) {
            return !string.IsNullOrEmpty(code) ? code : noCode;
        }
        #endregion
        #region 当前模式设置
        /// <summary>
        /// 设置当前大的模式
        /// </summary>
        /// <param name="_pModeType"></param>
        private void SetMode(ModeType _ModeType) {
            modeType = _ModeType;
        }
        /// <summary>
        /// 获取当前小的模式
        /// </summary>
        /// <returns></returns>
        public ModeType GetMode() {
            return modeType;
        }
        #endregion
        #region 当前功能脚本
        public IMode GetModeManager(ModeType _modeType)
        {
            if (dicMode.ContainsKey(_modeType))
            {
                return dicMode[_modeType];
            }
            return null;
        }
        /// <summary>
        /// 更新告警实体数据
        /// </summary>
        /// <param name="entity"></param>
        public void RefreshAlarm(Entity entity) {
            if (entity.state == 0)
            {
                if (dicAlarmEntity.ContainsKey(entity.id))
                {
                    dicAlarmEntity.Remove(entity.id);
                }
            }
            else
            {
                if (dicAlarmEntity.ContainsKey(entity.id))
                {
                    dicAlarmEntity[entity.id] = entity;
                }
                else
                {
                    dicAlarmEntity.Add(entity.id, entity);
                }
            }
        }
        /// <summary>
        /// 刷新现有资源实体
        /// </summary>
        /// <param name="entity"></param>
        public void RefreshEntity(Entity entity) {
            if (!listType.Contains(entity.t_eCode))
            {
                listType.Add(entity.t_eCode);
            }
            if (entity.exist == 1)
            {
                if (dicElements.ContainsKey(entity.t_eCode))
                {
                    if (lineLabelParent.Find(entity.t_eCode + "/" + entity.id) != null)
                    {
                        Transform transElementTemp = lineLabelParent.Find(entity.t_eCode + "/" + entity.id);
                        if (dicElements[entity.t_eCode].Contains(transElementTemp.GetComponent<ElementItem>()))
                        {
                            dicElements[entity.t_eCode].Remove(transElementTemp.GetComponent<ElementItem>());
                        }
                        if (dicLineLabels[entity.position.lineId].Contains(transElementTemp))
                        {
                            dicLineLabels[entity.position.lineId].Remove(transElementTemp);
                        }
                        DestroyImmediate(transElementTemp.gameObject);
                    }
                }
            }
        }
        /// <summary>
        /// 显示告警范围
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string ShowAlarmRange(Entity entity)
        {
            string result = "";
            Transform itemPos = lineLabelParent.Find(entity.t_eCode + "/" + entity.id);
            foreach (ElementItem item in transform.GetComponentsInChildren<ElementItem>())
            {
                if (item.name != entity.id && itemPos != null)
                {
                    if (Vector3.Distance(item.transform.position, itemPos.position) <= entity.s_data.range)
                    {
                        result += "{\"resId\":\"" + item.name + "\",\"t_eCode\":\"" + item.t_eType + "\"},";
                    }
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Remove(result.LastIndexOf(','), 1);
            }
            return result;
        }
        /// <summary>
        /// 通过标签是否合并，显示或者隐藏标签
        /// </summary>
        public void ShowOrHideLabel()
        {
            if (arrayActive)
            {
                foreach (KeyValuePair<string, List<Transform>> item in dicLineLabels)
                {
                    Line line = tviewBase.Find(dicLines[item.Key].a_eCode + "/Line_" + item.Key).GetComponent<Line>();
                    double num = Math.Floor(line.lineLenght / Manager.GetInstance().limitConfig.perDis);

                    if (item.Value.Count > num && item.Value.Count > 1)
                    {
                        foreach (Transform itemChild in item.Value)
                        {
                            if (itemChild != null)
                            {
                                itemChild.gameObject.SetActive(false);
                            }
                        }
                        GameObject goItem = null;
                        if (arrayLabelParent.Find(item.Key) == null)
                        {
                            goItem = Instantiate(P_ElementArray) as GameObject;
                            goItem.name = item.Key;
                            goItem.transform.SetParent(arrayLabelParent);
                            goItem.transform.localPosition = line.transform.localPosition;
                        }
                        else
                        {
                            goItem = arrayLabelParent.Find(item.Key).gameObject;
                        }
                        List<Transform> listActive = new List<Transform>();
                        foreach (Transform itemActive in item.Value)
                        {
                            if (itemActive.parent.gameObject.activeInHierarchy)
                            {
                                listActive.Add(itemActive);
                            }
                        }
                        goItem.GetComponent<ElementArray>().Set(listActive);
                    }
                }
            }
            
        }
        /// <summary>
        /// 刷新区域资源实体
        /// </summary>
        /// <param name="entity"></param>
        public void RefreshAreaElements(Entity entity, ElementItem elementItem) {
            if (dicLines.ContainsKey(entity.position.lineId))
            {
                if (dicAreaElements.ContainsKey(dicLines[entity.position.lineId].a_eCode))
                {
                    dicAreaElements[dicLines[entity.position.lineId].a_eCode].Add(elementItem);
                }
                else
                {
                    List<ElementItem> elementItemsTemp = new List<ElementItem>();
                    elementItemsTemp.Add(elementItem);
                    dicAreaElements.Add(dicLines[entity.position.lineId].a_eCode, elementItemsTemp);
                }
            }
        }
        /// <summary>
        /// 刷新线段资源实体
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="_element"></param>
        public void RefreshLineElements(Entity entity, Transform _element) {
            if (dicEntityLine.ContainsKey(entity.id))
            {
                if (dicEntityLine[entity.id] != entity.position.lineId)
                {
                    if (dicLineLabels.ContainsKey(dicEntityLine[entity.id]))
                    {
                        for (int i = 0; i < dicLineLabels[dicEntityLine[entity.id]].Count; i++)
                        {
                            if (dicLineLabels[dicEntityLine[entity.id]][i] != null)
                            {
                                if (dicLineLabels[dicEntityLine[entity.id]][i].name == entity.id)
                                {
                                    dicLineLabels[dicEntityLine[entity.id]].Remove(dicLineLabels[dicEntityLine[entity.id]][i]);
                                }
                            }
                        }
                    }
                    if (dicLineLabels.ContainsKey(entity.position.lineId))
                    {
                        if (!dicLineLabels[entity.position.lineId].Contains(_element))
                        {
                            dicLineLabels[entity.position.lineId].Add(_element);
                        }
                    }
                    else
                    {
                        List<Transform> listTemp = new List<Transform>();
                        listTemp.Add(_element);
                        dicLineLabels.Add(entity.position.lineId, listTemp);
                    }
                }
            }
            else
            {
                if (!dicEntityLine.ContainsKey(entity.id))
                {
                    dicEntityLine.Add(entity.id, entity.position.lineId);
                }
                if (!dicLineLabels.ContainsKey(entity.position.lineId))
                {
                    List<Transform> listTemp = new List<Transform>();
                    listTemp.Add(_element);
                    dicLineLabels.Add(entity.position.lineId, listTemp);
                }
                else
                {
                    if (!dicLineLabels[entity.position.lineId].Contains(_element))
                    {
                        dicLineLabels[entity.position.lineId].Add(_element);
                    }
                }
            }
        }
        /// <summary>
        /// 显示资源
        /// </summary>
        /// <param name="_active"></param>
        public void SetResActive(bool _active = true) {
            lineLabelParent.gameObject.SetActive(_active);
            arrayLabelParent.gameObject.SetActive(_active);
        }
        #endregion
        #region 主要功能模块
        /// <summary>
        /// 框选
        /// </summary>
        /// <param name="current"></param>
        public void CurrentRes(TEvent tEvent)
        {
            isCurrent = int.Parse(tEvent.eventParams[0].ToString()) == 0 ? true : false;
        }
        /// <summary>
        /// 设置当前模式
        /// </summary>
        /// <param name="nEvent"></param>
        private void setDominantMode(TEvent nEvent)
        {
            ModeType _modeType = (ModeType)nEvent.eventParams[0];
            SetMode(_modeType);
            ReStartNormal(_modeType);
            dicMode[_modeType].IInit();
            switch (_modeType)
            {
                case ModeType.allView://0
                    Debug.Log("综合监控模式");
                    break;
                case ModeType.inspector://1
                    Debug.Log("寻览模式");
                    break;
                case ModeType.wayLine://2
                    Debug.Log("路线模式");
                    break;
                case ModeType.single2D://3
                    Debug.Log("2D模式");
                    break;
                case ModeType.tunnelDegree://4
                    Debug.Log("掘进进度模式");
                    break;
                case ModeType.area://5
                    Debug.Log("区域选择模式");
                    break;
                case ModeType.track://6
                    Debug.Log("轨迹回放模式");
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 需要保存的数据

        public Dictionary<string, List<Transform>> dicNameLines = new Dictionary<string, List<Transform>>();//线段按名称划分
        public Dictionary<string, List<Transform>> dicAreaLines = new Dictionary<string, List<Transform>>();//线段按区域划分

        public Dictionary<string, LineData> dicLines = new Dictionary<string, LineData>();//线段的数据
        public Dictionary<int, List<WayData>> dicWays = new Dictionary<int, List<WayData>>();//路径

        public Dictionary<string, Entity> dicAlarmEntity = new Dictionary<string, Entity>();//告警

        public Dictionary<string, string> dicAreaName = new Dictionary<string, string>();//区域配置
        public Dictionary<string, string> dicLineName = new Dictionary<string, string>();//巷道名称配置

        public Dictionary<string, ConfigType> diceType = new Dictionary<string, ConfigType>();//资源类型配置
        public Dictionary<string, Sprite> diceTypeSprites = new Dictionary<string, Sprite>();

        public Dictionary<string, List<ElementItem>> dicElements = new Dictionary<string, List<ElementItem>>();//所有资源实体

        public Dictionary<string, List<Transform>> dicLineLabels = new Dictionary<string, List<Transform>>();//以线段为单位的资源实体

        public Dictionary<string, List<ElementItem>> dicAreaElements = new Dictionary<string, List<ElementItem>>();//以区域为单位的资源实体

        public Dictionary<string, string> dicEntityLine = new Dictionary<string, string>();//资源实体所在线段


        public List<string> listType = new List<string>();//资源实体的类型

        public Dictionary<GameObject, bool> dicTrack = new Dictionary<GameObject, bool>();//轨迹回放

        public List<ElementItem> listSelectItems = new List<ElementItem>();

        public Vector3 startPos = Vector3.zero;
        public Dictionary<string, int> dicOverGroundRes = new Dictionary<string, int>();
        #endregion
    }
    #region 地上模型配置解析
    [Serializable]
    public class OverGround
    {
        public string intoWell;
        public List<Data> resData;
    }
    [Serializable]
    public class Data
    {
        public string resId;
        public int resType;//资源类型，用于显示标签及点击功能还有运煤带 0 - 装饰建筑；1 - 可选中建筑；2 - 运煤带
    }
    #endregion
    #region 初始化线段解析
    [System.Serializable]
    public class LineData
    {
        public string id;
        public string l_eCode;
        public float length;
        public string a_eCode;
    }
    #endregion
    #region  路径解析
    [System.Serializable]
    public class WayData
    {
        public string id;
        public string name;
        public int type;
        public string color;
        public List<Points> points;
    }
    [System.Serializable]
    public class Points
    {
        public string lineId;
        public float distance;
    }
    #endregion
    #region config 配置
    [System.Serializable]
    public class ConfigData
    {
        public List<ConfigItem> area;
        public List<ConfigItem> line;
        public List<ConfigType> eType;
    }
    [System.Serializable]
    public class ConfigItem
    {
        public string eCode;
        public string name;
    }
    [System.Serializable]
    public class ConfigType
    {
        public string eCode;
        public string name;
        public TypeData data;
    }
    [System.Serializable]
    public class TypeData
    {
        public string icon;
        public string color;
    }
    #endregion
    #region 资源实体解析
    [Serializable]
    public class Entity
    {
        public string id;//资源的id
        public string t_eCode;//资源类型编号
        public string name;//资源名称
        public int a_type;//资源的位置0-地下，1-地上
        public int state;//资源状态 0-正常，1,2,3,4...不正常
        public StateData s_data;
        public int exist;//资源操作 0-刷新，1-删除
        public int display;//资源是否显示 0-显示，1-不显示
        public Position position;//资源位置
        public List<IndexData> data;
    }
    [System.Serializable]
    public class StateData
    {
        public string icon;
        public string color;
        public float range;
        public float rate;
    }
    [System.Serializable]
    public class PositionContent
    {
        public string lineId;
        public float distance;
    }
    [System.Serializable]
    public class Position
    {
        public string lineId;
        public List<PositionContent> content;
        public float distance;
    }
    [Serializable]
    public class IndexData
    {
        public string key;
        public float keyfontsize;
        public string keyfontcolor;
        public string value;
        public float valuefontsize;
        public string valuefontcolor;
    }
    #endregion
    #region 掘进
    [Serializable]
    public class TunnelList
    {
        public List<Tunnel> list;
    }
    [Serializable]
    public class Tunnel
    {
        public string lineId;
        public float tunnelDegree;
    }
    #endregion
    #region 巡检
    [Serializable]
    public class Inspection
    {
        public List<Points> insp;
    }
    #endregion
    #region 轨迹回放
    [Serializable]
    public class Track
    {
        public string id;
        public List<IndexData> data;//指标
        public string t_eCode;
        public List<TrackPoints> track;
    }
    [Serializable]
    public class TrackPoints
    {
        public string lineId;
        public float distance;
        public List<IndexData> data;
    }
    #endregion

    #region 发送cad信息
    [Serializable]
    public class cadMessage
    {
        public string lineId;
        public float length;
    }
    #endregion
    #region 巷道图标限制
    [Serializable]
    public class LimitConfig
    {
        public string icon;
        public string color;
        public int perDis;
    }
    #endregion
    #region cad等的初始化配置
    [Serializable]
    public class InitConfig
    {
        public string cadURL;
        public string cadColor;
        public int arrayActive;//集合图表的开启和关闭，0-开启，1-关闭
        public LimitConfig limit;
        public string background;
    }
    #endregion
    #region 资源类型
    [Serializable]
    public class ResTypes
    {
        public List<string> list;
    }
    #endregion
}

