﻿/*
 * PIDI_PlanarReflections v 1.7
 * Programmed  by : Jorge Pinal Negrete.
 * This code is part of the PIDI Framework made by Jorge Pinal Negrete. for Irreverent Software.
 * Copyright(c) 2015-2018, Jorge Pinal Negrete.  All Rights Reserved. 
 * 
 *  
*/


Shader "PIDI Shaders Collection/Planar Reflection/Generic/Simple (Mobile)" {
	Properties {
		[Space(12)]
		[Header(PBR Properties)]
		_Color( "Color", Color ) = (1,1,1,1) //The color tint applied to the surface
		_SpecColor( "Specular Color", Color ) = ( 0.1,0.1,0.1,1 ) //Specular Color
		_MainTex( "Main Tex", 2D ) = "white" {} //The main texture used
		_BumpMap( "Normal map", 2D ) = "bump"{} //Used for bump mapping and reflection distortion
		_GSOHMap( "Gloss(R) Specular (G) Occlusion (B) Heightmap (A)", 2D ) = "white" {} //Main texture that handles gloss, metallic, occlusion and height maps 
		
		[Space(8)]
		_Parallax( "Parallax Height", Range( 0, 0.1 ) ) = 0.0 //The distortion applied by the heightmap
		_Glossiness( "Smoothness", Range( 0, 1 ) ) = 0	//PBR smoothness value	

		[Space(12)]
		[Header(Reflection Properties)]
		_ReflectionTint("Reflection Tint", Color) = (1,1,1,1) //The color tint to be applied to the reflection		
		_RefDistortion( "Bump Reflection Distortion", Range( 0, 0.1 ) ) = 0.25 //The distortion applied to the reflection
		_NormalDist( "Surface Distortion", Range(0,1)) = 0 //Surface derived distortion
		_ReflectionTex ("Reflection Texture", 2D) = "white" {} //The render texture containing the real-time reflection
		
		[Space(12)]
		[Header(Material Emission)]
		[Enum(Additive,0,Masked,1)]_EmissionMode( "Emission Mode", Float ) = 0 //Blend mode for the emission and reflection channels
		[Enum(Disabled,2,Enabled,50,Overbright,100)]_HDROverride( "HDR Mode", Float ) = 2
		[Space(8)]
		_EmissionColor( "Emission Color (RGB) Intensity (2*Alpha)", Color ) = (0,0,0,0.5)
		_EmissionMap( "Emission Map (RGB) Mask (A)", 2D ) = "white"{}//Emissive map

		[Space(12)]
		[Header(VR Specific)]
		_EyeOffset("Eye's Offset (XY=Left, ZW=Right)", vector) = (0,0,0.058,-0.004)

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular noshadow
		#pragma target 2.0

		sampler2D _MainTex;
		sampler2D _ReflectionTex;
		sampler2D _BumpMap;
		sampler2D _GSOHMap;
		sampler2D _EmissionMap;

		half _NormalDist;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_HeightMap;
			float2 uv_GSOHMap;
			float2 uv_EmissionMap;
			float4 screenPos;
			float3 viewDir;
		};

		fixed4 _ReflectionTint;
		fixed4 _Color;
		fixed4 _EmissionColor;
		half _Glossiness;
		half _RefDistortion;
		half _Parallax;
		half _EmissionMode;
		half _HDROverride;

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
		
			half4 gsoh = tex2D( _GSOHMap, IN.uv_GSOHMap ); //We project the GSOH map ( Gloss Metallic Occlusion Height )
			
			//float2 offsetHeight = ParallaxOffset( gsoh.a, _Parallax, IN.viewDir ); //And calculate the parallax offset based on the height map (alpha  channel of our gsoh texture )

			o.Normal = float3(0,0,1);

			half dist = 2*sign(dot(o.Normal, IN.viewDir)-0.5)*(dot(o.Normal,IN.viewDir)-0.5)*_NormalDist; //Normal based distortion factor

			o.Normal = UnpackNormal( tex2D( _BumpMap, IN.uv_BumpMap ) ); //We unpack the normals from our normalmap applying the distortion to them
			
			half4 mColor = tex2D( _MainTex, IN.uv_MainTex ); //and our diffuse texture is projected as well.
			o.Albedo =  mColor*_Color; //The albedo output will be the main texture color multiplied by the tint color _Color
			
			//offsetHeight = o.Normal*_RefDistortion; //We get the reflection distortion by multiplying the _RefDistortion factor by the normal.
		
			//Calculate the screen UV coordinates
			float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w,0.001);
			
			screenUV -= dist;

			//VR Single pass fix version 1.0
			#if UNITY_SINGLE_PASS_STEREO 
				//If Unity is rendering the scene in a single pass stereoscopic rendering mode, we need to repeat the image horizontally. This is of course not 100% phisycally accurate
				//produces the desired effect in almost any situation (except those where the stereoscopic separation between the eyes is abnormally large)

				screenUV.x = screenUV.x*2-floor(screenUV.x*2)+unity_StereoEyeIndex*_EyeOffset.z+(1-unity_StereoEyeIndex)*_EyeOffset.x;
				screenUV.y += unity_StereoEyeIndex*_EyeOffset.w+(1-unity_StereoEyeIndex)*_EyeOffset.y;
				//TODO : Future versions of this tool will have full support for physically accurate single pass stereoscopic rendering by merging two virtual eyes into a single accurate reflection texture.
			#endif


			half4 c = tex2D ( _ReflectionTex, screenUV );// We render the reflection and distort it
			half4 e = tex2D( _EmissionMap, IN.uv_EmissionMap);

			e.rgb *= e.a*_EmissionColor.rgb*(_EmissionColor.a*_HDROverride);
							
			half fresnelValue = saturate( dot ( o.Normal, IN.viewDir ) ); //We calculate a very simple fresnel - like value based on the view to surface angle.
			//And use it for the reflection, since we want it to be stronger in sharp view angles and get affected by the diffuse color of the surface when viewed directly.
			o.Emission = e.rgb+1*c.rgb*_ReflectionTint;
			o.Specular = _SpecColor*gsoh.g; //Specular value is multiplied by the green channel of our GSOH texture
			o.Smoothness = _Glossiness*gsoh.g; //Glossiness value is multiplied by the green channel of our GSOH texture
			o.Occlusion = gsoh.b; //Occlusion is read from our GSOH texture.
			o.Alpha = 1;
		}
		
		
		ENDCG
		
		
	

		
	} 
	FallBack "Diffuse"
}
