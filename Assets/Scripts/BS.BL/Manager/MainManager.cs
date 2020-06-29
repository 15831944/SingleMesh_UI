using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace BS.BL.Manager {
    public enum ManagerType {
        loadManager,
        uiManager,
    }
    public class MainManager : MonoBehaviour
    {
        private static MainManager instance;
        public static MainManager GetInstance() {
            if (instance == null)
            {
                instance = new MainManager();
            }
            return instance;
        }
        public Dictionary<ManagerType, ManagerBase> dicAllManager = new Dictionary<ManagerType, ManagerBase>();
        public GameObject overground;
        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
        }
        void Start()
        {
            Init();
            StartCoroutine(LoadAssetBundle(Application.streamingAssetsPath + "/overground.assetbundle"));
        }
        void Init() {
            dicAllManager.Add(ManagerType.loadManager, gameObject.AddComponent<LoadManager>());
        }
        public IEnumerator LoadAssetBundle(string _url)
        {
            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(_url))
            {
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    // 下载出错
                    Debug.Log(request.error);
                }
                else
                {
                    // 下载完成
                    AssetBundle assetBundle = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                    // 优先释放request 会降低内存峰值
                    GameObject go = assetBundle.LoadAsset<GameObject>("OverGround");
                    overground = Instantiate(go);
                    overground.transform.position = Vector3.zero;
                    foreach (MeshRenderer item in overground.transform.GetComponentsInChildren<MeshRenderer>(false))
                    {
                        //Debug.Log(item.material.shader.name);
                        //线框效果shader
                        foreach (Material mat in item.materials)
                        {
                            if (mat.shader.name == "VacuumShaders/The Amazing Wireframe/Mobile/One Directional Light")
                            {
                                mat.shader = Resources.Load("shader/Diffuse") as Shader;
                            }
                            if (mat.shader.name == "Hidden/VacuumShaders/The Amazing Wireframe/Mobile/One Directional Light/Transparent/2 Sided/Diffuse")
                            {
                                mat.shader = Resources.Load("shader/Transparent 2Sided Diffuse") as Shader;
                            }
                            if (mat.shader.name == "PIDI Shaders Collection/Planar Reflection/Generic/Depth+Blur")
                            {
                                mat.shader = Resources.Load("shader/PD_ReflectiveMaterial_GenericPBR_Depth+Blur") as Shader;
                            }
                            if (mat.shader.name == "PIDI Shaders Collection/Water/Simple/Reflection+Refraction")
                            {
                                mat.shader = Resources.Load("shader/PD_ReflectiveMaterial_Water_Simple_Refraction&Reflection") as Shader;
                            }
                        }
                    }
                    overground.SetActive(false);
                    request.Dispose();
                }
            }
        }
        private void FixedUpdate()
        {
            waitTime += Time.deltaTime;
            if (waitTime > 30f)
            {
                UnLoadResources();
                waitTime = 0f;
            }
        }
        float waitTime = 0f;
        private void UnLoadResources() {
            Resources.UnloadUnusedAssets();
        }
    }
}

