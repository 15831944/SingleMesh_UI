using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BL.BS.Two.UI.Tito
{
    public class TitoItem : MonoBehaviour
    {
        private Tweener c_Tweener;
        public Color titoColor;
        public void SetColorAndAnimation(Color _color) {
            if (c_Tweener != null)
            {
                c_Tweener.Kill();
            }
            this.GetComponent<Image>().color = _color;
            titoColor = _color;
            c_Tweener = GetComponent<Image>().DOColor(new Color(_color.r, _color.g, _color.b, 0), 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }
    }
}
