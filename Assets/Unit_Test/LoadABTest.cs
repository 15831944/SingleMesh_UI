using BS.BL.Two.Loader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadABTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError(Application.streamingAssetsPath);
        StartCoroutine(LoadAssetBundle("http://127.0.0.1:8080/single/overground.assetbundle"));
    }

    // Update is called once per frame
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
                GameObject goI = Instantiate(go);
                foreach (MeshRenderer item in goI.transform.GetComponentsInChildren<MeshRenderer>())
                {
                    Debug.Log(item.material.shader.name);
                    //线框效果shader
                    foreach (Material mat in item.materials)
                    {
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
                request.Dispose();
            }
        }
    }
}
