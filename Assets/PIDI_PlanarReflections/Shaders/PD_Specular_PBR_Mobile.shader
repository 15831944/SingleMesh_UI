Shader "PIDI Shaders Collection/Planar Reflection/Mobile/Simple PBR Specular" {
	
	Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}


		[Header(Specularity)]
		[Space(10)]
		[Enum(Values,0,Texture,1)] _SpecSource ("Specularity Source", Float) = 0
		_SpecColor("Specular Color", Color ) = (0.1,0.1,0.1,1)
        _Glossiness("Glossiness", Range(0.0, 1.0)) = 0.5
        _SpecGlossMap("Specular Color Map (RGB) Gloss (A)", 2D) = "black" {}
		


		[Header(Normals)]
		[Space(10)]
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

		[Header(Occlusion. Set to 0 when using Occlusion Maps)]
		[Space(10)]
        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

		[Space(12)]
		[Header(Material Emission)]
		[Enum(Additive,0,Masked,1)]_EmissionMode( "Emission/Reflection Blend Mode", Float ) = 0 //Blend mode for the emission and reflection channels
		_EmissionColor( "Emission Color (RGB) Intensity (16*Alpha)", Color ) = (1,1,1,0.5)
		_EmissionMap( "Emission Map (RGB) Mask (A)", 2D ) = "black"{}//Emissive map

		
        [Enum(UV1_FromAlbedo,0,UV2_FromNormalmap,1)] _UVSec ("UV Set for secondary textures", Float) = 0

		[Space(12)]
		[Header(Reflection Properties)]
		_ReflectionTint("Reflection Tint", Color) = (1,1,1,1) //The color tint to be applied to the reflection
		_RefDistortion( "Bump Reflection Distortion", Range( 0, 0.1 ) ) = 0.25 //The distortion applied to the reflection
		_NormalDist( "Surface Distortion", Range(0,1)) = 0 //Surface derived distortion
		_ReflectionTex ("Reflection Texture", 2D) = "white" {} //The render texture containing the real-time reflection


		[Space(12)]
		[Header(VR Specific)]
		_EyeOffset("Eye's Offset (XY=Left, ZW=Right)", vector) = (0,0,0.058,-0.004)

    }


	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows
		#include "UnityStandardUtils.cginc"

		#pragma target 2.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _SpecGlossMap;
		sampler2D _MetallicGlossMap;
		sampler2D _OcclusionMap;
		sampler2D _EmissionMap;
		sampler2D _ReflectionTex;
		sampler2D _ParallaxMap;
		sampler2D _DetailAlbedoMap;
		sampler2D _DetailNormalMap;
		sampler2D _DetailMask;

		struct Input {
			float2 uv_MainTex;
			float2 uv_DetailAlbedoMap;
			float2 uv2_DetailNormalMap;
			float2 uv_DetailAlbedoMask;
			float3 viewDir;
			float4 screenPos;
		};

		half _Glossiness;
		half _Metallic;
		half _BumpScale;
		half _DetailNormalMapScale;
		half _OcclusionStrength;
		float _UVSec;
		float _SpecSource;
		fixed4 _Color;
		fixed4 _EmissionColor;
		fixed4 _ReflectionTint;
		float4 _EyeOffset;
		half _NormalDist;
		half _BlurSize;
		half _RefDistortion;
		half _Parallax;
		half _EmissionMode;


		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {

			float2 offsetHeight = float2(0,0);

			o.Normal = float3(0,0,1);

			half dist = 2*sign(dot(o.Normal, IN.viewDir)-0.5)*(dot(o.Normal,IN.viewDir)-0.5)*_NormalDist; //Normal based distortion factor

			
			// Albedo comes from a texture tinted by color
			fixed4 col = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
			half3 n1 = UnpackScaleNormal( tex2D(_BumpMap, IN.uv_MainTex), _BumpScale );
			
			o.Albedo = col.rgb;
			o.Normal = normalize(n1);

			offsetHeight = o.Normal*_RefDistortion; //We get the reflection distortion by multiplying the _RefDistortion factor by the normal.
		
			//Calculate the screen UV coordinates
			float2 screenUV = IN.screenPos.xy / max(IN.screenPos.w,0.001);
			
			screenUV += dist;

			//VR Single pass fix version 1.0
			#if UNITY_SINGLE_PASS_STEREO 
				//If Unity is rendering the scene in a single pass stereoscopic rendering mode, we need to repeat the image horizontally. This is of course not 100% phisycally accurate
				//produces the desired effect in almost any situation (except those where the stereoscopic separation between the eyes is abnormally large)

				screenUV.x = screenUV.x*2-floor(screenUV.x*2)+unity_StereoEyeIndex*_EyeOffset.z+(1-unity_StereoEyeIndex)*_EyeOffset.x;
				screenUV.y += unity_StereoEyeIndex*_EyeOffset.w+(1-unity_StereoEyeIndex)*_EyeOffset.y;


				//TODO : Future versions of this tool will have full support for physically accurate single pass stereoscopic rendering by merging two virtual eyes into a single accurate reflection texture.
			#endif

			
			
			fixed4 c = tex2D ( _ReflectionTex, screenUV+offsetHeight );
			

			half4 sGloss = tex2D(_SpecGlossMap, IN.uv_MainTex+offsetHeight);
			o.Smoothness = lerp(_Glossiness, sGloss.r, _SpecSource );
			o.Specular = lerp(_SpecColor,sGloss.rgb, _SpecSource );
			o.Occlusion = tex2D(_OcclusionMap, IN.uv_MainTex+offsetHeight).r*(1-_OcclusionStrength);
			
			half4 e = tex2D(_EmissionMap,IN.uv_MainTex+offsetHeight);
			e.rgb *= _EmissionColor.rgb*(_EmissionColor.a*16);
			half fresnelValue = saturate( dot ( o.Normal, IN.viewDir ) ); //We calculate a very simple fresnel - like value based on the view to surface angle.
			//And use it for the reflection, since we want it to be stronger in sharp view angles and get affected by the diffuse color of the surface when viewed directly.
			o.Emission = e.rgb+lerp(1,1-e.a,_EmissionMode)*c.rgb*_ReflectionTint*lerp( _ReflectionTint.a*0.5, 1, 1-fresnelValue )*lerp( max( o.Albedo, half3( 0.1, 0.1, 0.1 ) ), half3( 1, 1, 1), 1-fresnelValue );
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
