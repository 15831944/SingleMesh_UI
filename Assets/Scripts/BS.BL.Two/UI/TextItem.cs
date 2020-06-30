using BS.BL.Two;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BL.BS.Two.Icon.Text
{
    [ExecuteInEditMode]
    public class TextItem : MonoBehaviour
    {
        public float nowLength = 0;
        public IndexData indexData;
        public float textParentHeight;
        public float kuangHeight;
        private void Awake()
        {
            
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }
        // Update is called once per frame
        void Update()
        {
            SetText(indexData);
        }
        /// <summary>
        /// 通过返回数据设置标签
        /// </summary>
        /// <param name="indexData"></param>
        public void SetText(IndexData indexData) {
            string _textValue = "";
            if (!string.IsNullOrEmpty(indexData.keyfontcolor) && !string.IsNullOrEmpty(indexData.valuefontcolor))
            {
                _textValue = "<color=" + indexData.keyfontcolor + ">" + indexData.key +
                    "<color=" + indexData.valuefontcolor + ">" + indexData.value;
            }
            GetTextLength(transform.Find("TextValue").GetComponent<TextMeshProUGUI>(), _textValue);
           
        }
        private void GetTextLength(TextMeshProUGUI _textPro, string _text) {
            _textPro.text = _text;
            string textNoHtml = striphtml(_text);
            int leng = Encoding.Default.GetBytes(textNoHtml.ToCharArray()).Length;
            nowLength = leng * 4.0f;
            if (nowLength < 206)
            {
                //_textPro.GetComponent<RectTransform>().localPosition = new Vector3(-103f + nowLength / 2f, 0, 0);
            }
            else
            {
                transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(nowLength, textParentHeight);
                transform.parent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(nowLength, 29f);
                transform.parent.parent.Find("bg").GetComponent<RectTransform>().sizeDelta = new Vector2(nowLength + 26f, textParentHeight);
                transform.parent.parent.Find("kuang").GetComponent<RectTransform>().sizeDelta = new Vector2(nowLength + 40f, kuangHeight);
                transform.Find("zs").localPosition = new Vector3(-nowLength / 2 - 1, 0, 0);
                //transform.Find("line").GetComponent<RectTransform>().sizeDelta = new Vector2(nowLength, 1);
            }
        }
        /// <summary>
        /// 去除富文本格式
        /// </summary>
        /// <param name="strhtml"></param>
        /// <returns></returns>
        public string striphtml(string strhtml)
        {
            string stroutput = strhtml;
            Regex regex = new Regex(@"<[^>]+>|</[^>]+>");
            stroutput = regex.Replace(stroutput, "");
            stroutput = new Regex(@"( )+").Replace(stroutput, " ");
            return stroutput;
        }
    }
}
