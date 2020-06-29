/*
 * PIDI_PlanarReflections v 1.7
 * Programmed  by : Jorge Pinal Negrete.
 * This code is part of the PIDI Framework made by Jorge Pinal Negrete. for Irreverent Software.
 * Copyright(c) 2015-2018, Jorge Pinal Negrete.  All Rights Reserved. 
 * 
 *  
*/
using UnityEngine;


public class PIDI_ForceUpdate:MonoBehaviour{

    public bool affectAllReflections;
    public PIDI_PlanarReflection[] reflections = new  PIDI_PlanarReflection[0];

    public void Start(){
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        if ( affectAllReflections ){
            reflections = GameObject.FindObjectsOfType<PIDI_PlanarReflection>() as PIDI_PlanarReflection[];
        }
    }

    public void OnPreRender() {
        for ( int i = 0; i < reflections.Length; i++ ){
            reflections[i].OnWillRenderObject(GetComponent<Camera>());
        }
    }

}