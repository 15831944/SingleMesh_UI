using BS.BL.Two.Item;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BS.BL.Two {
    public interface IMode
    {
        void IInit();
        void IRun();
        void IQuit();
    }
}

