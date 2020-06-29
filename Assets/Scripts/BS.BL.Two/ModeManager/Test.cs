using System.Collections;
using System.Collections.Generic;
using TFramework.ApplicationLevel;
using TFramework.EventSystem;
using UnityEngine;
using UnityEngine.UI;

namespace BS.BL.Two.Item {
    public class Test : MonoBehaviour
    {
        public Button btnModeSelect;
        // Start is called before the first frame update
        void Start()
        {
            btnModeSelect.onClick.AddListener(delegate ()
            {
                TEventSystem.Instance.EventManager.dispatchEvent(new TEvent(TEventType.DominantMode, 2), this);
            });
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

