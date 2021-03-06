﻿/*
 * PIDI_PlanarReflections v 1.7
 * Programmed  by : Jorge Pinal Negrete.
 * This code is part of the PIDI Framework made by Jorge Pinal Negrete. for Irreverent Software.
 * Copyright(c) 2015-2018, Jorge Pinal Negrete.  All Rights Reserved. 
 * 
 *  
*/


Shader "PIDI Shaders Collection/Planar Reflection/Mirror/Blur Only" {
	Properties {
		[Space(12)]
		[Header(Basic Properties)]
		_Glossiness( "Smoothness (for overlay)", Range( 0, 1 ) ) = 0//The smoothness of the overlay
		_Metallic( "Metallic (for overlay )", Range( 0, 1 ) ) = 0//The metallic value of the overlay
		_OverlayTex( "Overlay Tex", 2D ) = "black" {} //The overlay texture is rendered on top of the reflection and everything else. Useful for stains for example.
		_BumpMap( "Normal map", 2D ) = "bump"{} //This normal map distorts the reflection and works as a default normalmap as well.
		
		[Space(12)]
		[Header(Reflection Properties)]
		_RefDistortion( "Reflection Distortion", Range( 0, 0.08 ) ) = 0.03 //The distortion strength applied to the mirror surface
		_NormalStrength( "Bump+Distortion/Just Distortion", Range(0,1) ) = 1 //With this slider, you can change between applying normalmaps to the full rendering or only as distortion for the reflections
		_ReflectionTint("Reflection Tint", Color) = (1,1,1,1) //The tint that will be applied to the reflection
		_ReflectionTex ("ReflectionTex", 2D) = "white" {} //The texture that holds our real time reflection
		_BlurLevel( "Blur Level", Range( 0, 1 ) ) = 0 //The blur level over this mirror.
		_NormalDist( "Surface Distortion", Range(0,1)) = 0 //Surface derived distortion	
	
		[Space(12)]
		[Header(VR Specific)]
		_EyeOffset("Eye's Offset (XY=Left, ZW=Right)", vector) = (0,0,0.058,-0.004)
	
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 600
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _FogTex;
		sampler2D _OverlayTex;
		sampler2D _ReflectionTex;
		sampler2D _BumpMap;
		half _NormalDist;

		struct Input {
			float2 uv_MainTex;
			float2 uv_FogTex;
			float2 uv_OverlayTex;
			float2 uv_BumpMap;
			float4 screenPos;
			float3 viewDir;
		};

		fixed4 _ReflectionTint;
		fixed4 _Color;
		fixed4 _FogColor;
		float4 _EyeOffset;
		half _Glossiness;
		half _Metallic;
		half _RefDistortion;
		half _BlurLevel;
		half _NormalStrength;
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			o.Normal = float3(0,0,1);

			half dist = 2*sign(dot(o.Normal, IN.viewDir)-0.5)*(dot(o.Normal,IN.viewDir)-0.5)*_NormalDist; //Normal based distortion factor
			
			o.Normal = UnpackNormal( tex2D( _BumpMap, IN.uv_BumpMap ) ); //We calculate the normals by unpacking the normalmap
			float2 nOffset = o.Normal*_RefDistortion; //We get the distortion by multiplying the normals by the disortion factor
			
			half4 ovTex = tex2D( _OverlayTex, IN.uv_OverlayTex ); //We get the colors from our overlay texture
		
			//The albedo of the shader is the overlay texture multiplied by the overlay alpha 
			o.Albedo = ovTex*ovTex.a;
			
			//We calculate the screen coordinates for the reflection.
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
			
			half4 c = half4(0.0h,0.0h,0.0h,0.0h);   
			float blurFactor = _BlurLevel*0.0015*saturate(1-ovTex.a*3); //We calculate our blur factor
			
			
			//We lerp the normals from our normals with bump mapping applied to a default positive plane normal with no bump map applied, to make the transition between bumpmap/distortion only
			o.Normal = normalize( lerp( o.Normal, float3(0,0,1) , _NormalStrength ) ); 
			
			if ( _BlurLevel > 0 ){ //If Blur level is bigger than one, we apply the blur effect. This makes the shader cheap when no blur is being applied since all this part will be skipped
		
				//The blur method is fairly simple and cheap, though it doesn't produce the best results as it uses very few samples. 
				//We unpack the texture multiple times and pan it to the sides at different distances and making them more / less visible depending
				//on their distance to the center. Then, you add all of the different textures and combine them, to generate teh basic blur.
				//Distortion is applied to each texture sampling.
    
			   	c += tex2D( _ReflectionTex, float2(screenUV.x-4.0 * blurFactor, screenUV.y+4.0 * blurFactor)+nOffset) * 0.05;
			    c += tex2D( _ReflectionTex, float2(screenUV.x+4.0 * blurFactor, screenUV.y-4.0 * blurFactor)) * 0.05;

			    
			    c += tex2D( _ReflectionTex, float2(screenUV.x-3.0 * blurFactor, screenUV.y+3.0 * blurFactor)+nOffset) * 0.09;
			    c += tex2D( _ReflectionTex, float2(screenUV.x+3.0 * blurFactor, screenUV.y-3.0 * blurFactor)+nOffset) * 0.09;
			    
			    c += tex2D( _ReflectionTex, float2(screenUV.x-2.0 * blurFactor, screenUV.y+2.0 * blurFactor)+nOffset) * 0.12;
			    c += tex2D( _ReflectionTex, float2(screenUV.x+2.0 * blurFactor, screenUV.y-2.0 * blurFactor)+nOffset) * 0.12;
			    
			    c += tex2D( _ReflectionTex, float2(screenUV.x-1.0 * blurFactor, screenUV.y+1.0 * blurFactor)+nOffset) *  0.15;
			    c += tex2D( _ReflectionTex, float2(screenUV.x+1.0 * blurFactor, screenUV.y-1.0 * blurFactor)+nOffset) *  0.15;
			    
				
  
			    c += tex2D( _ReflectionTex, screenUV-4.0 * blurFactor+nOffset) * 0.05;
			    c += tex2D( _ReflectionTex, screenUV-3.0 * blurFactor+nOffset) * 0.09;
			    c += tex2D( _ReflectionTex, screenUV-2.0 * blurFactor+nOffset) * 0.12;
			    c += tex2D( _ReflectionTex, screenUV-1.0 * blurFactor+nOffset) * 0.15;    
			    c += tex2D( _ReflectionTex, screenUV+nOffset) * 0.16; 
			    c += tex2D( _ReflectionTex, screenUV+5.0 * blurFactor+nOffset) * 0.15;
			    c += tex2D( _ReflectionTex, screenUV+4.0 * blurFactor+nOffset) * 0.12;
			    c += tex2D( _ReflectionTex, screenUV+3.0 * blurFactor+nOffset) * 0.09;
			    c += tex2D( _ReflectionTex, screenUV+2.0 * blurFactor+nOffset) * 0.05;
				
				c = c/2;
			}
			else{
				c = tex2D ( _ReflectionTex, screenUV+nOffset); //If the blur level is less than 0, we just unpack the reflection once without any blur.
			}
			
			//The reflection is sent to the emission channel, multiplied by the reflection color and multiplied by the inverse of the overlay alpha
			//so that the reflection is only visible where there is no overlay, making the overlay effectively appear over it
			o.Emission = c.rgb*_ReflectionTint*(1-ovTex.a);
			o.Metallic = _Metallic*ovTex.a;//The metallic value multiplied by the overlay alpha to affect only the overlay
			o.Smoothness = _Glossiness*ovTex.a;//The smoothness value multiplied by the overlay alpha so it only affects the overlay
			o.Alpha = 1;
		}
		
		
		ENDCG
		
		
	

		
	} 
	FallBack "Diffuse"
}
