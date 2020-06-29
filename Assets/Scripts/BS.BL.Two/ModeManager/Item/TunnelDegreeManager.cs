using System.Collections;
using System.Collections.Generic;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two.Item
{
    public class TunnelDegreeManager : MonoBehaviour,IMode
    {
        private MainManager mainManager;
        private GameObject tunnelGo;
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
            tunnelGo = Resources.Load("Prefabs/Two/TunnelLine") as GameObject;
            TEventSystem.Instance.EventManager.addEventListener(TEventType.TunnelDegree, SetTunnelDegree);
        }
        /// <summary>
        /// 设置掘进进度
        /// </summary>
        /// <param name="tEvent"></param>
        public void SetTunnelDegree(TEvent tEvent)
        {
            if (mainManager.modeType == ModeType.tunnelDegree)
            {
                tEvent.eventParams[0] = "{ \"list\": " + tEvent.eventParams[0].ToString() + "}";
                TunnelList tunnelList = JsonUtility.FromJson<TunnelList>(tEvent.eventParams[0].ToString());
                foreach (Tunnel item in tunnelList.list)
                {
                    if (mainManager.dicLines.ContainsKey(item.lineId))
                    {
                        Line line = mainManager.tviewBase.Find(mainManager.dicLines[item.lineId].a_eCode).Find("Line_" + item.lineId).GetComponent<Line>();
                        Vector3 vFor = (line.endPoint.pos - line.startPoint.pos).normalized;
                        Vector3 tunnelStart = line.startPoint.pos + vFor * item.tunnelDegree * (Vector3.Distance(line.startPoint.pos, line.endPoint.pos));
                        GameObject tunnelGa = Instantiate(tunnelGo) as GameObject;
                        tunnelGa.transform.parent = mainManager.tviewBase.Find("TunnelPa");
                        tunnelGa.transform.GetChild(0).GetComponent<MeshRenderer>().material = tunnelGa.GetComponent<TunnelLine>().tunnelMat;
                        tunnelGa.name = "Tunnel_" + item.lineId;
                        tunnelGa.transform.eulerAngles = line.transform.eulerAngles;
                        tunnelGa.transform.localPosition = (line.endPoint.pos + tunnelStart) / 2;
                        tunnelGa.transform.localScale = new Vector3(5.1f, 5.1f, Vector3.Distance(line.endPoint.pos, tunnelStart) / 2);

                    }
                }
            }
            else
            {
                Debug.Log("请先设置综合监控模式下的掘进状态");
            }

        }
    }
}
