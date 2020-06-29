using BS.BL.Interface;
using BS.BL.Two.Item;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BS.BL.Two.Element
{
    public class DynamicElement : MonoBehaviour
    {
        private static DynamicElement instance;
        public static DynamicElement GetInstance() {
            if (instance == null)
            {
                instance = new DynamicElement();
            }
            return instance;
        }
        private void Awake()
        {
            instance = this;
        }
        public bool isPlay = false;
        public bool inPlay = false;
        //private List<Vector3> listPos = new List<Vector3>();
        private Dictionary<int, Vector3> dicPos = new Dictionary<int, Vector3>();
        private Dictionary<Vector3, string> dicPosLine = new Dictionary<Vector3, string>();
        public Transform target;
        public float moveSpeed = 1f;
        public Two.Track nowTrack1;
        public bool exist = true;
        private Vector3 initPos;
        public ElementItem targetItem;
        private float d_Length = 0f;
        private float z_Degree = 0f;
        private float waitTime1 = 0f;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (isPlay)
            {
                if (!target.gameObject.activeInHierarchy)
                {
                    target.gameObject.SetActive(true);
                }
            }
        }
        private void FixedUpdate()
        {
            SetTargetAnimation();
            DisAbleEntity();
        }
        float waitTime = 0f;
        private void DisAbleEntity() {
            if (inPlay)
            {
                waitTime += Time.deltaTime;
                if (waitTime > 1f)
                {
                    foreach (KeyValuePair<string, List<ElementItem>> item in MainManager.GetInstance().dicElements)
                    {
                        foreach (ElementItem itemElement in item.Value)
                        {
                            if (itemElement != (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).nowElement)
                            {
                                itemElement.gameObject.SetActive(false);
                            }
                        }
                    }
                    foreach (Transform itemArray in MainManager.GetInstance().arrayLabelParent)
                    {
                        itemArray.gameObject.SetActive(false);
                    }
                    waitTime = 0f;
                }
                
            }
        }
        /// <summary>
        /// 添加路径数组
        /// </summary>
        /// <param name="pos"></param>
        int dicNum = 0;
        public void SetPointPosition(Vector3 pos,string lineId) {
            //if (listPos.Contains(pos))
            //{
            //    return;
            //}
            //listPos.Add(pos);
            if (!dicPosLine.ContainsKey(pos))
            {
                dicPosLine.Add(pos, lineId);
            }
            dicPos.Add(dicNum, pos);
            dicNum++;
            inPlay = true;
        }

        public void SetTrackLine() {
            foreach (Transform item in MainManager.GetInstance().tviewBase.Find("TrackLine"))
            {
                DestroyImmediate(item.gameObject);
            }
            //for (int i = 0; i < listPos.Count - 1; i++)
            //{
            //    if (dicPosLine[listPos[i]] == dicPosLine[listPos[i + 1]])
            //    {
            //        Line line1 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
            //            dicLines[dicPosLine[listPos[i]]].a_eCode).Find("Line_" + dicPosLine[listPos[i]]).GetComponent<Line>();
            //        GameObject lineGa = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
            //        lineGa.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
            //        lineGa.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa.GetComponent<TunnelLine>().tunnelMat;
            //        lineGa.name = "tack_" + dicPosLine[listPos[i]];
            //        lineGa.transform.eulerAngles = line1.transform.eulerAngles;
            //        lineGa.transform.localPosition = (listPos[i] + listPos[i + 1]) / 2;
            //        lineGa.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(listPos[i], listPos[i + 1]) / 2);
            //    }
            //}
        }
        /// <summary>
        /// 重置
        /// </summary>
        public void ResetStart()
        {
            if (MainManager.GetInstance().modeType != ModeType.single2D)
            {
                //listPos.Clear();
                dicPos.Clear();
                dicNum = 0;
                dicPosLine.Clear();
                isPlay = false;
                inPlay = false;
                MainManager.GetInstance().modeType = ModeType.allView;
                foreach (Transform item in MainManager.GetInstance().tviewBase.Find("Layer_HD/Nodes"))
                {
                    if (item.GetComponent<Node>() != null)
                    {
                        item.GetComponent<Node>().Select(false);
                    }
                }
                foreach (Transform item in MainManager.GetInstance().tviewBase.Find("TrackLine"))
                {
                    Destroy(item.gameObject);
                }
                if (nowTrack1 != null)
                {
                    foreach (KeyValuePair<GameObject, bool> item in MainManager.GetInstance().dicTrack)
                    {
                        if (item.Key != null)
                        {
                            item.Key.SetActive(item.Value);
                        }
                    }
                    if (exist)
                    {
                        target.position = initPos;
                        foreach (KeyValuePair<string, List<Transform>> item in MainManager.GetInstance().dicLineLabels)
                        {
                            if (item.Value.Contains(target))
                            {
                                if (MainManager.GetInstance().arrayLabelParent.Find(item.Key) != null)
                                {
                                    (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).arrayGo = MainManager.GetInstance().arrayLabelParent.Find(item.Key).gameObject;
                                    MainManager.GetInstance().arrayLabelParent.Find(item.Key).gameObject.SetActive(false);
                                }
                                //ElementContainer.GetInstance().level = 1;
                            }
                        }
                        //target.GetComponent<ElementItem>().circle.GetComponent<TrailRenderer>().time = 0f;
                        target.GetComponent<ElementItem>().SetUIByLevel(1);
                        target.GetComponent<ElementItem>().Set(target.GetComponent<ElementItem>().resEntityTemp, false, true);
                    }
                    else
                    {
                        Destroy(target.gameObject);
                        (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).SceneToLevel(1);
                        exist = true;
                    }
                    (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).nowElement = target.GetComponent<ElementItem>();
                    if ((MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).arrayGo != null)
                    {
                        if (MainManager.GetInstance().dicTrack.ContainsKey((MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).arrayGo))
                        {
                            if (MainManager.GetInstance().dicTrack[(MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).arrayGo] == true)
                            {
                                (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).ResetLabel(true);
                            }
                        }
                    }
                    nowTrack1 = null;
                }
                StartCoroutine(WaitTime());
            }
        }
        IEnumerator WaitTime() {
            yield return new WaitForSeconds(0.1f);
            if ((MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).nowElement != null)
            {
                (MainManager.GetInstance().dicMode[ModeType.allView] as AllViewManager).nowElement.SetUIByLevel(1);
            }
            target = null;
        }
        /// <summary>
        /// 设置目标物体
        /// </summary>
        /// <param name="_target"></param>
        public void SetTarget(Transform _target,Vector3 _initPos, bool _exist = true) {
            initPos = _initPos;
            posNum = 0;
            exist = _exist;
            target = _target;
            d_Length = 0f;
            z_Degree = 0f;
            //target.localPosition = listPos[0];
            //for (int i = 0; i < listPos.Count; i++)
            //{
            //    if (i + 1 < listPos.Count)
            //    {
            //        if (dicPosLine[listPos[i]] == dicPosLine[listPos[i + 1]])
            //        {
            //            d_Length += Vector3.Distance(listPos[i], listPos[i + 1]);
            //        }
            //    }
            //}
            target.localPosition = dicPos[0];
            for (int i = 0; i < dicPos.Count; i++)
            {
                if (i + 1 < dicPos.Count)
                {
                    if (dicPosLine[dicPos[i]] == dicPosLine[dicPos[i + 1]])
                    {
                        d_Length += Vector3.Distance(dicPos[i], dicPos[i + 1]);
                    }
                }
            }
        }
        private int posNum = 0;
        private void SetTargetAnimation() {
            if (isPlay)
            {
                Camera.main.GetComponent<BLCameraControl>().LookAtPosition3(target.position, true);
                if (dicPos.Count > 0)
                {
                    if (posNum < dicPos.Count - 1)
                    {
                        if (posNum == 0)
                        {
                            z_Degree = Vector3.Distance(target.localPosition, dicPos[0]) / d_Length;
                        }
                        else
                        {
                            float _distance = 0f;
                            for (int i = 0; i < posNum; i++)
                            {
                                if (dicPosLine[dicPos[i]] == dicPosLine[dicPos[i + 1]])
                                {
                                    _distance += Vector3.Distance(dicPos[i], dicPos[i + 1]);
                                }
                            }
                            if (dicPosLine[dicPos[posNum]] == dicPosLine[dicPos[posNum + 1]])
                            {
                                _distance += Vector3.Distance(target.localPosition, dicPos[posNum]);
                            }
                            z_Degree = _distance / d_Length;
                        }
                        GameObject.Find("JSInterface").GetComponent<JSInterface>().sendTrackDegree(z_Degree.ToString("N10"));
                        if (dicPosLine[dicPos[posNum]] == dicPosLine[dicPos[posNum + 1]])
                        {
                            target.localPosition = Vector3.MoveTowards(target.localPosition, dicPos[posNum + 1], moveSpeed * Time.deltaTime);
                            if (target.localPosition == dicPos[posNum + 1])
                            {
                               CreateLine(posNum);
                            }
                        }
                        else
                        {
                            target.localPosition = dicPos[posNum + 1];
                            try
                            {
                                Line lineTemp1 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                               dicLines[dicPosLine[dicPos[posNum]]].a_eCode).Find("Line_" + dicPosLine[dicPos[posNum]]).GetComponent<Line>();
                                Line lineTemp2 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                                        dicLines[dicPosLine[dicPos[posNum + 1]]].a_eCode).Find("Line_" + dicPosLine[dicPos[posNum + 1]]).GetComponent<Line>();
                                CreateLineJie(lineTemp1, lineTemp2, posNum);
                            }
                            catch (System.Exception e)
                            {

                                Debug.Log("区域" + MainManager.GetInstance().
                                        dicLines[dicPosLine[dicPos[posNum + 1]]].a_eCode + "线段" + dicPosLine[dicPos[posNum + 1]]);
                            }
                            
                        }
                        target.GetComponent<ElementItem>().SetTrackData(null, true, nowTrack1.track[posNum].data);
                        if (target.localPosition == dicPos[posNum + 1])
                        {
                            posNum += 1;
                        }
                    }
                    else
                    {
                        posNum = 0;
                        target.localPosition = dicPos[posNum];
                        GameObject.Find("JSInterface").GetComponent<JSInterface>().sendTrackDegree("0");
                        target.GetComponent<ElementItem>().SetTrackData(null, true, nowTrack1.track[posNum].data);
                    }
                }
            }
        }
        public void SetTrackDegree(float degree) {
            isPlay = false;
            if (dicPos.Count > 0)
            {
                float _degreeLength = degree * d_Length;
                float _distance2 = 0f;
                if (degree != 0)
                {
                    for (int i = 0; i < dicPos.Count; i++)
                    {
                        if (i + 1 < dicPos.Count)
                        {
                            if (dicPosLine[dicPos[i]] == dicPosLine[dicPos[i + 1]])
                            {
                                if (_degreeLength > _distance2)
                                {
                                    _distance2 += Vector3.Distance(dicPos[i], dicPos[i + 1]);
                                }
                                if (_degreeLength == _distance2)
                                {
                                    target.localPosition = dicPos[i + 1];
                                    target.GetComponent<ElementItem>().SetTrackData(null, true, nowTrack1.track[i + 1].data);
                                    Camera.main.GetComponent<BLCameraControl>().LookAtPosition3(target.position, true);
                                    posNum = i + 1;
                                    CreateLine(posNum);
                                    return;
                                }
                                if (_degreeLength < _distance2)
                                {
                                    target.localPosition = dicPos[i + 1] - (_distance2 - _degreeLength) * (dicPos[i + 1] - dicPos[i]).normalized;
                                    target.GetComponent<ElementItem>().SetTrackData(null, true, nowTrack1.track[i].data);
                                    Camera.main.GetComponent<BLCameraControl>().LookAtPosition3(target.position, true);
                                    Create2(posNum);
                                    posNum = i;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    target.localPosition = dicPos[0];
                    posNum = 0;
                }
            }
        }
        private Dictionary<string, int> dicAreaStart = new Dictionary<string, int>();
        private void CreateLine(int num) {
            if (target.localPosition == dicPos[num + 1])
            {
                if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[num]] + num.ToString()) == null)
                {
                    Line line1 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                        dicLines[dicPosLine[dicPos[num]]].a_eCode).Find("Line_" + dicPosLine[dicPos[num]]).GetComponent<Line>();
                    GameObject lineGa = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                    lineGa.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                    lineGa.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa.GetComponent<TunnelLine>().tunnelMat;
                    lineGa.name = "tack_" + dicPosLine[dicPos[num]] + num.ToString();
                    lineGa.transform.eulerAngles = line1.transform.eulerAngles;
                    lineGa.transform.localPosition = (dicPos[num] + dicPos[num + 1]) / 2;
                    lineGa.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[num], dicPos[num + 1]) / 2);
                }
            }
        }
        private void Create2(int num) {
            if (posNum == num - 1)
            {
                if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[num]] + num.ToString()) == null)
                {
                    Line line1 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                        dicLines[dicPosLine[dicPos[num]]].a_eCode).Find("Line_" + dicPosLine[dicPos[num]]).GetComponent<Line>();
                    GameObject lineGa = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                    lineGa.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                    lineGa.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa.GetComponent<TunnelLine>().tunnelMat;
                    lineGa.name = "tack_" + dicPosLine[dicPos[num]] + num.ToString();
                    lineGa.transform.eulerAngles = line1.transform.eulerAngles;
                    lineGa.transform.localPosition = (dicPos[num] + dicPos[num + 1]) / 2;
                    lineGa.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[num], dicPos[num + 1]) / 2);
                }
            }
            else
            {
                for (int i = 0; i < num - 1; i++)
                {
                    if (dicPosLine[dicPos[i]] == dicPosLine[dicPos[i + 1]])
                    {
                        if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[i]] + i.ToString()) == null)
                        {
                            Line line1 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                                dicLines[dicPosLine[dicPos[i]]].a_eCode).Find("Line_" + dicPosLine[dicPos[i]]).GetComponent<Line>();
                            GameObject lineGa = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                            lineGa.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                            lineGa.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa.GetComponent<TunnelLine>().tunnelMat;
                            lineGa.name = "tack_" + dicPosLine[dicPos[i]] + i.ToString();
                            lineGa.transform.eulerAngles = line1.transform.eulerAngles;
                            lineGa.transform.localPosition = (dicPos[i] + dicPos[i + 1]) / 2;
                            lineGa.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[i], dicPos[i + 1]) / 2);
                        }
                    }
                    else
                    {
                        Line lineTemp1 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                                dicLines[dicPosLine[dicPos[i]]].a_eCode).Find("Line_" + dicPosLine[dicPos[i]]).GetComponent<Line>();
                        Line lineTemp2 = MainManager.GetInstance().tviewBase.Find(MainManager.GetInstance().
                                dicLines[dicPosLine[dicPos[i + 1]]].a_eCode).Find("Line_" + dicPosLine[dicPos[i + 1]]).GetComponent<Line>();
                        CreateLineJie(lineTemp1, lineTemp2, i);
                    }
                }
            }
        }
        private void CreateLineJie(Line line1, Line line2, int numTemp) {
            if (line1.startPoint.pos == line2.endPoint.pos)
            {
                if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[numTemp]] + numTemp.ToString() + "_1") == null)
                {
                    GameObject lineGa1 = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                    lineGa1.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                    lineGa1.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa1.GetComponent<TunnelLine>().tunnelMat;
                    lineGa1.name = "tack_" + dicPosLine[dicPos[numTemp]] + numTemp.ToString() + "_1";
                    lineGa1.transform.eulerAngles = line1.transform.eulerAngles;
                    lineGa1.transform.localPosition = (dicPos[numTemp] + line1.startPoint.pos) / 2;
                    lineGa1.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[numTemp], line1.startPoint.pos) / 2);
                    foreach (Transform item in MainManager.GetInstance().tviewBase.Find("Layer_HD/Nodes"))
                    {
                        if (Vector3.Distance(item.position, MainManager.GetInstance().tviewBase.transform.TransformPoint(line1.startPoint.pos)) < 0.01f)
                        {
                            item.GetComponent<Node>().Select(true);
                        }
                    }
                }
                if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[numTemp + 1]] + (numTemp + 1).ToString() + "_2") == null)
                {
                    GameObject lineGa2 = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                    lineGa2.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                    lineGa2.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa2.GetComponent<TunnelLine>().tunnelMat;
                    lineGa2.name = "tack_" + dicPosLine[dicPos[numTemp + 1]] + (numTemp + 1).ToString() + "_2";
                    lineGa2.transform.eulerAngles = line2.transform.eulerAngles;
                    lineGa2.transform.localPosition = (dicPos[numTemp + 1] + line1.startPoint.pos) / 2;
                    lineGa2.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[numTemp + 1], line1.startPoint.pos) / 2);
                    foreach (Transform item in MainManager.GetInstance().tviewBase.Find("Layer_HD/Nodes"))
                    {
                        if (Vector3.Distance(item.position, MainManager.GetInstance().tviewBase.transform.TransformPoint(line1.startPoint.pos)) < 0.01f)
                        {
                            item.GetComponent<Node>().Select(true);
                        }
                    }
                }
            } else if (line1.endPoint.pos == line2.startPoint.pos) {
                if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[numTemp]] + numTemp.ToString() + "_1") == null)
                {
                    GameObject lineGa1 = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                    lineGa1.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                    lineGa1.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa1.GetComponent<TunnelLine>().tunnelMat;
                    lineGa1.name = "tack_" + dicPosLine[dicPos[numTemp]] + numTemp.ToString() + "_1";
                    lineGa1.transform.eulerAngles = line1.transform.eulerAngles;
                    lineGa1.transform.localPosition = (dicPos[numTemp] + line1.endPoint.pos) / 2;
                    lineGa1.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[numTemp], line1.endPoint.pos) / 2);
                    foreach (Transform item in MainManager.GetInstance().tviewBase.Find("Layer_HD/Nodes"))
                    {
                        if (Vector3.Distance(item.position, MainManager.GetInstance().tviewBase.transform.TransformPoint(line1.endPoint.pos)) < 0.01f)
                        {
                            item.GetComponent<Node>().Select(true);
                        }
                    }
                }
                if (MainManager.GetInstance().tviewBase.Find("TrackLine" + "/tack_" + dicPosLine[dicPos[numTemp + 1]] + (numTemp + 1).ToString() + "_2") == null)
                {
                    GameObject lineGa2 = Instantiate((MainManager.GetInstance().dicMode[ModeType.track] as TrackManager).lineGo) as GameObject;
                    lineGa2.transform.parent = MainManager.GetInstance().tviewBase.Find("TrackLine");
                    lineGa2.transform.GetChild(0).GetComponent<MeshRenderer>().material = lineGa2.GetComponent<TunnelLine>().tunnelMat;
                    lineGa2.name = "tack_" + dicPosLine[dicPos[numTemp + 1]] + (numTemp + 1).ToString() + "_2";
                    lineGa2.transform.eulerAngles = line2.transform.eulerAngles;
                    lineGa2.transform.localPosition = (dicPos[numTemp + 1] + line1.endPoint.pos) / 2;
                    lineGa2.transform.localScale = new Vector3(4.1f, 4.1f, Vector3.Distance(dicPos[numTemp + 1], line1.endPoint.pos) / 2);
                    foreach (Transform item in MainManager.GetInstance().tviewBase.Find("Layer_HD/Nodes"))
                    {
                        if (Vector3.Distance(item.position, MainManager.GetInstance().tviewBase.transform.TransformPoint(line1.endPoint.pos)) < 0.01f)
                        {
                            item.GetComponent<Node>().Select(true);
                        }
                    }
                }
            }
        }
    }
}
