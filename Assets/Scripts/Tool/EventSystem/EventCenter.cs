using UnityEngine;
using TFramework.EventSystem;
namespace TFramework.ApplicationLevel
{
    public class TEventType
    {
        //demo
        public const string ChangeDim = "ChangeDim";
        public const string InitData = "InitData";//初始化数据
        public const string DominantMode = "DominantMode";//当前模式事件
        public const string RefreshRes = "RefreshRes";//资源刷新

        public const string SetAllArea = "SetAllArea";//所有区域

        public const string DominantArea = "DominantArea";//设置主区域
        public const string DominantRes = "DominantRes";//设置主资源
        public const string DominantResType = "DominantResType";//设置资源类型
        public const string DominantTunnel = "DominantTunnel";//设置主巷道
        public const string TunnelDegree = "TunnelDegree";//掘进进度
        public const string ShowGround = "ShowGround";//显示地上模型
        public const string Inspector = "Inspector";//巡检
        public const string Current = "Current";//框选
        public const string TrackPlay = "TrackPlay";//轨迹回放
        public const string TrackInPlay = "TrackInPlay";//轨迹回放
        public const string TrackDegree = "TrackDegree";//轨迹回放
        public const string TrackSpeed = "TrackSpeed";//轨迹回放
        public const string AdjustRange = "AdjustRange";//调节告警范围
        public const string SetWay = "SetWay";//路线模式
        public const string DeleteWayPoint = "DeleteWayPoint";//路线模式

        public const string ScrollSpeed = "ScrollSpeed";//设置镜头的缩放
    }

    public class EventCenter : MonoBehaviour, IEventCenter
    {
        public void IClearAllListener()
        {
            throw new System.NotImplementedException();
        }

        private void InitView()
        {
            
        }

        public void IInitManagers()
        {

        }

        public void IRegisterAllListener()
        {
            TEventSystem.Instance.EventManager.addEventListener(TEventType.ChangeDim, ChangeDim);
        }

        /***************************逻辑实现方法************************************/

        private void ChangeDim(TEvent nEvent)
        {
            //GameObject.Find("Root").GetComponent<ApplicationSM>().ChargeTransState(SOperate.ChangeDim, nEvent.eventParams);
        }
        void Start()
        {
            InitView();
            IInitManagers();
            IRegisterAllListener();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
