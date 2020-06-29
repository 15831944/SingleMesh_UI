using System.Collections;
using System.Collections.Generic;
using TFramework.EventSystem;
using UnityEngine;
namespace BS.BL.Two.Item {
    public class NormalManager : MonoBehaviour, IMode
    {
        private MainManager mainManager;
        void Start() {
            mainManager = MainManager.GetInstance();
        }
        public void IInit()
        {
        }

        public void IQuit()
        {
        }

        public void IRun()
        {
        }
    }
}

