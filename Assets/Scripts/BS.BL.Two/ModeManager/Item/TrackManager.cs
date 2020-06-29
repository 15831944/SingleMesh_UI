using BS.BL.Two.Element;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two.Item {
    public class TrackManager : MonoBehaviour,IMode
    {
        private MainManager mainManager;
        private Track trackMsg;
        public GameObject lineGo;
        public void IInit()
        {
        }

        public void IQuit()
        {
            foreach (Transform item in MainManager.GetInstance().tviewBase.Find("TrackLine"))
            {
                DestroyImmediate(item.gameObject);
            }
        }

        public void IRun()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            mainManager = MainManager.GetInstance();
            lineGo = Resources.Load("Prefabs/Two/TrackLineFang") as GameObject;
            TEventSystem.Instance.EventManager.addEventListener(TEventType.TrackPlay, SetTrackPlay);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.TrackInPlay, SetTrackInPlay);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.TrackSpeed, SetTrackSpeed);
            TEventSystem.Instance.EventManager.addEventListener(TEventType.TrackDegree, SetTrackDegree);
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// 轨迹回放
        /// </summary>
        /// <param name="tEvent"></param>
        public void SetTrackPlay(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.allView)
            {
                mainManager.modeType = ModeType.track;
                for (int i = 0; i < (mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr.Count; i++)
                {
                    if ((mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr[i] != null)
                    {
                        (mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr[i].Kill(true);
                    }
                }
                for (int i = 0; i < (mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr2.Count; i++)
                {
                    if ((mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr2[i] != null)
                    {
                        (mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr2[i].Kill(true);
                    }
                }
                (mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr2.Clear();
                (mainManager.dicMode[ModeType.allView] as AllViewManager).moveTr.Clear();
                if (mainManager.modeType == ModeType.track)
                {
                    if (!string.IsNullOrEmpty(tEvent.eventParams[0].ToString()))
                    {
                        trackMsg = JsonUtility.FromJson<Track>(tEvent.eventParams[0].ToString());
                        DynamicElement.GetInstance().nowTrack1 = trackMsg;
                        foreach (Transform itemAll in mainManager.lineLabelParent)
                        {
                            if (!mainManager.dicTrack.ContainsKey(itemAll.gameObject))
                            {
                                mainManager.dicTrack.Add(itemAll.gameObject, itemAll.gameObject.activeInHierarchy);
                            }
                            else
                            {
                                mainManager.dicTrack[itemAll.gameObject] = itemAll.gameObject.activeInHierarchy;
                            }
                            foreach (Transform itemChildAll in itemAll)
                            {
                                if (!mainManager.dicTrack.ContainsKey(itemChildAll.gameObject))
                                {
                                    mainManager.dicTrack.Add(itemChildAll.gameObject, itemChildAll.gameObject.activeInHierarchy);
                                }
                                else
                                {
                                    mainManager.dicTrack[itemChildAll.gameObject] = itemChildAll.gameObject.activeInHierarchy;
                                }
                            }
                        }
                        foreach (Transform itemTrans in mainManager.lineLabelParent)
                        {
                            if (itemTrans.name != trackMsg.t_eCode)
                            {
                                itemTrans.gameObject.SetActive(false);
                            }
                            else
                            {
                                foreach (Transform itemChild in itemTrans)
                                {
                                    if (itemChild.name != trackMsg.id)
                                    {
                                        itemChild.gameObject.SetActive(false);
                                    }
                                }
                            }
                        }
                        foreach (Transform itemArray in mainManager.arrayLabelParent)
                        {
                            if (!mainManager.dicTrack.ContainsKey(itemArray.gameObject))
                            {
                                mainManager.dicTrack.Add(itemArray.gameObject, itemArray.gameObject.activeInHierarchy);
                            }
                            else
                            {
                                mainManager.dicTrack[itemArray.gameObject] = itemArray.gameObject.activeInHierarchy;
                            }
                        }
                        foreach (TrackPoints item in trackMsg.track)
                        {
                            if (GameObject.Find("Line_" + item.lineId))
                            {
                                Line lineTemp = GameObject.Find("Line_" + item.lineId).GetComponent<Line>();
                                Vector3 vForTemp = (lineTemp.endPoint.pos - lineTemp.startPoint.pos).normalized;
                                DynamicElement.GetInstance().SetPointPosition(lineTemp.startPoint.pos + vForTemp * item.distance, item.lineId);
                            }
                            else
                            {
                                Debug.LogError("请在初始化数据及cad中标明" + item.lineId + "线段所在位置");
                            }
                        }
                        DynamicElement.GetInstance().SetTrackLine();
                        if (mainManager.lineLabelParent.Find(trackMsg.t_eCode) == null)
                        {
                            GameObject goPaTemp = new GameObject();
                            goPaTemp.transform.SetParent(mainManager.lineLabelParent);
                            goPaTemp.transform.localPosition = Vector3.zero;
                            goPaTemp.name = trackMsg.t_eCode;
                        }
                        if (mainManager.lineLabelParent.Find(trackMsg.t_eCode + "/" + trackMsg.id) != null)
                        {
                            Transform trackTemp = mainManager.lineLabelParent.Find(trackMsg.t_eCode + "/" + trackMsg.id);
                            foreach (KeyValuePair<string, List<Transform>> item in mainManager.dicLineLabels)
                            {
                                if (item.Value.Contains(trackTemp))
                                {
                                    if (mainManager.arrayLabelParent.Find(item.Key) != null)
                                    {
                                        mainManager.arrayLabelParent.Find(item.Key).gameObject.SetActive(false);
                                    }
                                    foreach (Transform itemChild in item.Value)
                                    {
                                        if (itemChild.name != trackMsg.id)
                                        {
                                            itemChild.gameObject.SetActive(false);
                                        }
                                    }
                                }
                            }
                            DynamicElement.GetInstance().SetTarget(trackTemp, trackTemp.position);
                            trackTemp.GetComponent<ElementItem>().SetUIByLevel(1);
                            trackTemp.gameObject.SetActive(true);
                            trackTemp.GetComponent<ElementItem>().SetTrackData(trackMsg, false, trackMsg.track[0].data);
                        }
                        else
                        {
                            if ((mainManager.dicMode[ModeType.allView] as AllViewManager).nowElement != null)
                            {
                                (mainManager.dicMode[ModeType.allView] as AllViewManager).nowElement.SetUIByLevel(0);
                                (mainManager.dicMode[ModeType.allView] as AllViewManager).SceneToLevel(1);
                            }
                            Transform goItemPa = mainManager.lineLabelParent.Find(trackMsg.t_eCode);
                            GameObject goItem = Instantiate((mainManager.dicMode[ModeType.allView] as AllViewManager).P_ElementItem) as GameObject;
                            Line line = mainManager.tviewBase.Find(mainManager.dicLines[trackMsg.track[0].lineId].a_eCode + "/Line_"
                                            + trackMsg.track[0].lineId).GetComponent<Line>();
                            Vector3 vFor = (line.endPoint.pos - line.startPoint.pos).normalized;
                            goItem.transform.parent = goItemPa;
                            goItem.name = trackMsg.id;
                            goItem.transform.localPosition = line.startPoint.pos + vFor * trackMsg.track[0].distance;
                            goItem.GetComponent<ElementItem>().nowState = 0;
                            DynamicElement.GetInstance().SetTarget(goItem.transform, Vector3.zero, false);
                            goItem.GetComponent<ElementItem>().SetUIByLevel(1);
                            goItem.gameObject.SetActive(true);
                            goItem.GetComponent<ElementItem>().SetTrackData(trackMsg, false, trackMsg.track[0].data);
                            goItem.GetComponent<ElementItem>().circle.SetActive(true);
                            foreach (Transform goTemp in goItemPa)
                            {
                                if (goTemp != goItem.transform)
                                {
                                    goTemp.gameObject.SetActive(false);
                                }
                            }
                        }
                        foreach (Transform item in mainManager.arrayLabelParent)
                        {
                            item.gameObject.SetActive(false);
                        }
                    }
                    //Invoke("play", 0.5f);
                    TEventSystem.Instance.EventManager.dispatchEvent(new TEvent(TEventType.TrackInPlay, 0), this);
                }
            }
            
        }
        private void play()
        {
            TEventSystem.Instance.EventManager.dispatchEvent(new TEvent(TEventType.TrackInPlay, 0), this);
        }
        /// <summary>
        /// 设置动画开始和暂停
        /// </summary>
        /// <param name="isPlay"></param>
        public void SetTrackInPlay(TEvent tEvent)
        {
            if (DynamicElement.GetInstance().inPlay)
            {
                DynamicElement.GetInstance().isPlay = int.Parse(tEvent.eventParams[0].ToString()) == 0 ? true : false;
            }
        }
        /// <summary>
        /// 设置轨迹回放速度
        /// </summary>
        /// <param name="speed"></param>
        public void SetTrackSpeed(TEvent tEvent)
        {
            if (int.Parse(tEvent.eventParams[0].ToString()) != 0)
            {
                DynamicElement.GetInstance().moveSpeed = int.Parse(tEvent.eventParams[0].ToString());
            }
        }
        /// <summary>
        /// 设置轨迹回放进度
        /// </summary>
        /// <param name="degree"></param>
        public void SetTrackDegree(TEvent tEvent)
        {
            DynamicElement.GetInstance().SetTrackDegree(float.Parse(tEvent.eventParams[0].ToString()));
        }
    }
}

