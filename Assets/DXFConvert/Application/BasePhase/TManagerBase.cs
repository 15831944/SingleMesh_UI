using Loader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TManagerBase : MonoBehaviour {

    public TViewBase TView;

    // Use this for initialization
    void Start()
    {
        // this.GetComponent<Loader.ILoader>().Loaded = Loaded;
        LoadCADTemp();
    }

    // Update is called once per frame
    void Update()
    {
        //if (TView.transform.childCount > 0)
        //{
        //    NormalCenter.GetCenter(TView.transform);
        //}
    }

    DiskFile iLoader;

    private void LoadDXF(string path = null)
    {
        //try
        //{
            GetComponent<MeshGenerater.DataContainer>().ResetError();
            if (path == null) {
                iLoader = new DiskFile(content);
            }
            else{
                iLoader = new DiskFile(path);
            }
            
            DXFConvert.DXFStructure dxfStructure = new DXFConvert.DXFStructure(iLoader);
            dxfStructure.Load();
            iLoader.Dispose();
            TView.Set(dxfStructure);
            NormalCenter.GetCenter(TView.transform);
            GetComponent<MeshGenerater.DataContainer>().SerializeData();
        Camera.main.GetComponent<BLCameraControl>().LookAtResAutoDis(GameObject.Find("TViewBase").transform, true);
        //Debug.Log("OK:" + path);
        //}
        //catch (System.Exception ex)
        //{
        //    Debug.Log("Error:" +ex.Message);
        //}

    }

    public string fileName = "";


    public void LoadCAD(string _url) {
        //plication.streamingAssetsPath, "CAD/" + fileName + ".dxf"));
        //LoadDXF(Application.streamingAssetsPath + "/CAD/"+fileName+".dxf");
        MeshGenerater.DataContainer.GetInstance().ClearAllList();
        //StartCoroutine(loadStreamingAsset(_url));
        StartCoroutine(loadStreamingAsset(_url));
        //StartCoroutine(LoadURL(Application.streamingAssetsPath + _url));

    }
    public void LoadCADTemp()
    {
        //StartCoroutine(loadStreamingAsset(Application.streamingAssetsPath + "/BL-sl2-2.dxf"));

    }

    private string[] content;
    IEnumerator loadStreamingAsset(string _url)
    {
        string _resultPath = "http://";
        string _path = Application.streamingAssetsPath;
        foreach (string item in _path.Split('/'))
        {
            if (item.Split(':').Length > 1)
            {
                if (IsNum(item.Split(':')[1]) && !string.IsNullOrEmpty(item.Split(':')[1]))
                {
                    _resultPath += item;
                }
            }
        }
        _url = _resultPath + _url;
        Debug.Log(_url);
        string result;
        if ((_url).Contains("://") || (_url).Contains(":///"))
        {
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(_url);
            yield return unityWebRequest.SendWebRequest();
            result = unityWebRequest.downloadHandler.text;
            content = result.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            LoadDXF();
        }
        else {
            content = File.ReadAllLines(_url);
            LoadDXF();
        }
    }
    bool IsNum(string _str) {
        ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例
        byte[] bytestr = ascii.GetBytes(_str);
        foreach (byte c in bytestr)                   //遍历这个数组里的内容
        {
            if (c < 48 || c > 57)                          //判断是否为数字
            {
                return false;                              //不是，就返回False
            }
        }
        return true;
    }
    public void TestLoadCAD(string name) {
        MeshGenerater.DataContainer.GetInstance().ClearAllList();
        string url = System.IO.Path.Combine(Application.streamingAssetsPath, "CAD/"+name);
        StartCoroutine(loadStreamingAsset(url));
    }
}
