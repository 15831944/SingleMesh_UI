using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.AnimatedValues; 
using System.Collections.Generic;
using System;

[CustomEditor( typeof( PIDI_PlanarReflection ) ) ]
public class PIDI_PlanarReflectionsEditor : Editor {

	public GUISkin pidiSkin;
	public AnimBool[] b_folds = new AnimBool[3];
	private PIDI_PlanarReflection v_target;
	public enum LandscapeSimp{ VeryLow = 2, Low = 4, Moderate = 8, High = 16, Extreme = 32};

	public void OnEnable( ){
		b_folds = new AnimBool[6];
		for ( int i = 0; i < 6; i++ ){
			b_folds[i] = new AnimBool(  );
			b_folds[i].valueChanged.AddListener( Repaint ); 
		}

		v_target = (PIDI_PlanarReflection)target;

	}
 

	public override void OnInspectorGUI(){

		Undo.RecordObject( v_target, "PD_PlanarReflectionInstance"+v_target.GetInstanceID() );
		SceneView.RepaintAll();

		var tSkin = GUI.skin;
		GUI.skin = pidiSkin;

		GUILayout.BeginHorizontal();GUILayout.BeginVertical(pidiSkin.box);
        GUILayout.Space(8);
		
		if ( !v_target.v_staticTexture && ( !v_target.GetComponent<MeshFilter>() || !v_target.GetComponent<MeshFilter>().sharedMesh ) ){
			EditorGUILayout.HelpBox( "To dynamically preview and use a planar reflection object without a mesh, you need to assign a static output render texture to it", MessageType.Info );
			EditorGUILayout.Separator();
		}


		if ( PDEditor_BeginFold("General Settings", ref b_folds[0] ) ){
			EditorGUI.indentLevel++;
			PDEditor_Toggle( new GUIContent("Update in Edit Mode","Updates the reflections for the Scene View while inside the Unity Editor"), ref v_target.b_updateInEditMode );
			PDEditor_Toggle( new GUIContent("Use Global Settings","If enabled, the global settings for downsampling and max. resolution will be used"), ref v_target.b_useGlobalSettings );
			PDEditor_Toggle( new GUIContent("Display Gizmos"), ref v_target.b_displayGizmos );
			
			GUILayout.Space(4);
			
			PDEditor_Toggle( new GUIContent("Use Explicit Normal", "Use an explicitly defined global direction as the surface normal to generate the reflection"), ref v_target.b_useExplicitNormal);
			
			if ( v_target.b_useExplicitNormal){
				GUILayout.BeginHorizontal();GUILayout.Space(12);GUILayout.BeginVertical();
				PDEditor_Toggle(new GUIContent("Local Normal?","Defines whether the explicit normal should be calculated based on this objects local coordinates"), ref v_target.b_localNormal );
				PDEditor_Vector3( new GUIContent("Normal Direction"), ref v_target.v_explicitNormal);
				GUILayout.EndVertical();GUILayout.EndHorizontal();
			}	

			var showDepth = false;
			var showChroma = false;

			if ( v_target.GetComponent<Renderer>() && v_target.GetComponent<Renderer>().sharedMaterials !=null ){
				
				if ( v_target.GetComponent<Renderer>().sharedMaterials!=null && v_target.GetComponent<Renderer>().sharedMaterials.Length > 0 )
					foreach ( Material m in v_target.GetComponent<Renderer>().sharedMaterials ){
						if ( m!=null && m.HasProperty("_ReflectionDepth") ){
							showDepth = true;
						}
						if ( m!=null && m.HasProperty("_ChromaTolerance") ){
							showChroma = true;
						}
					}
			}

			


			if ( showDepth ){
				GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox("You are using a depth-enabled reflection shader. Please make sure that a depth shader is assigned in the slot below", MessageType.Info );
				GUILayout.Space(8);GUILayout.EndHorizontal(); GUILayout.Space(4);

				PDEditor_ObjectField( new GUIContent("Depth Shader", "The shader to be used for the depth effects of this reflection"), ref v_target.depthShader, false );
				GUILayout.Space(4);
			}

			


			if ( showChroma ){
				GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox("You are using a Chroma Key enabled shader. Make sure to enable Chroma Key Effects for this reflection", MessageType.Info );
				GUILayout.Space(8);GUILayout.EndHorizontal(); GUILayout.Space(4);

				PDEditor_Toggle( new GUIContent("Use Chroma Key Effects", "Modifies the way this reflections will be rendered to allow the Chroma Key based shaders to work as intended"), ref v_target.b_ignoreSkybox );
				if ( v_target.b_ignoreSkybox ){
				PDEditor_Color( new GUIContent("Background Color (For Chroma Key)", "The background color that will be erased with the Chroma Key effect and replaced with a static background"), ref v_target.v_backdropColor );
				}
				
			}

			GUILayout.Space(8);
		}
		PDEditor_EndFold();


		if ( PDEditor_BeginFold( "Rendering Settings", ref b_folds[1] ) ){

			PDEditor_LayerMaskField( new GUIContent( "Reflect Layers", "The layers this reflection will reflect"), ref v_target.v_reflectLayers );

			
			//Oblique projection matrices force forward rendering and other projections with deferred shading break the main directional light.
			if ( v_target.v_renderingPath == RenderingPath.DeferredShading && (v_target.b_safeClipping||v_target.b_realOblique) ){
				GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox( "Due to limits in the screen space rendering, Deferred mode works only with Safe Clipping disabled and may not recognize directional lights in Unity versions prior to Unity 5.6", MessageType.Warning );
				GUILayout.Space(8);GUILayout.EndHorizontal();GUILayout.Space(4);
			}
			

			v_target.v_renderingPath = (RenderingPath) PDEditor_EnumPopup( new GUIContent("Rendering Path", "Rendering path to be used by this reflection"), v_target.v_renderingPath );

			


			PDEditor_ObjectField( new GUIContent("Explicit Output", "If you assign a Render Texture here, the planar reflection will be written to this texture making it accesible to, for example, be used across multiple materials" ), ref v_target.v_staticTexture, false );

			GUILayout.Space(8);
			
			PDEditor_Toggle ( new GUIContent( "Safe Clipping", "Enable Safe Clipping for a physically accurate mirror with a reflection cropped to the edges of the plane" ), ref v_target.b_safeClipping );
			
			if ( v_target.b_safeClipping ){
#if !UNITY_5_5_OR_NEWER
				if ( v_target.b_realOblique ){
					GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
					EditorGUILayout.HelpBox( "In older Unity versions, the use of real oblique projections breaks the shadows system", MessageType.Info );
					GUILayout.Space(8);GUILayout.EndHorizontal();GUILayout.Space(4);
				}

				PDEditor_Toggle ( new GUIContent( "Oblique Projection" ), ref v_target.b_realOblique );

				if ( !v_target.b_realOblique ){
					v_target.v_shadowDistance = v_target.v_shadowDistance == 0?1:v_target.v_shadowDistance;
					GUILayout.BeginHorizontal();GUILayout.Space(12);
					PDEditor_Vector2( new GUIContent( "Safe Area", "The area used to crop the reflection. This is an approximation, so you must experiment with different values" ), ref v_target.v_mirrorSize );
					GUILayout.EndHorizontal();GUILayout.Space(8);
				}
#endif		
			}

			PDEditor_FloatField( new GUIContent( "Clipping Planes Offset" ), ref v_target.v_clippingOffset );
			PDEditor_FloatField( new GUIContent( "Near Clip" ), ref v_target.v_nearClipModifier );
			PDEditor_FloatField( new GUIContent( "Far Clip Distance Modifier" ), ref v_target.v_farClipModifier );

			v_target.v_farClipModifier = Mathf.Clamp( v_target.v_farClipModifier, v_target.v_nearClipModifier+0.01f, Mathf.Infinity );
			v_target.v_shadowDistance = Mathf.Clamp ( v_target.v_shadowDistance, 0.0f, v_target.v_farClipModifier );

			PDEditor_FloatField( new GUIContent("Shadows Distance"), ref v_target.v_shadowDistance );


			if ( v_target.v_shadowDistance == 0 && !v_target.b_realOblique){
				v_target.b_realOblique = v_target.b_safeClipping = true;
			}

			GUILayout.Space(8);

			PDEditor_CenteredLabel("RESOLUTION SETTINGS");

			GUILayout.Space(8);

			if ( !v_target.v_staticTexture ){
				PDEditor_Toggle( new GUIContent("Force Power of 2 textures", "Force the reflection texture to be a power of 2 texture"), ref v_target.b_forcePower2 );
				PDEditor_Toggle( new GUIContent("Use Screen resolution", "Uses the screen resolution as the resolution for all reflections, which offers (in most cases) the best balance between quality and performance"), ref v_target.b_useScreenResolution );

				if ( !v_target.b_useScreenResolution ){
					v_target.v_resolution.x = v_target.v_resolution.y = v_target.b_forcePower2?Mathf.ClosestPowerOfTwo( PDEditor_IntField( new GUIContent("Reflection Resolution"), Mathf.Clamp ( (int)v_target.v_resolution.x, 16, 4096 ) ) ):EditorGUILayout.IntField( new GUIContent("Reflection Resolution"), Mathf.Clamp ( (int)v_target.v_resolution.x, 16, 4096 ) );
				}

				if ( !v_target.b_useDynamicResolution ){
					PDEditor_Popup( new GUIContent("Resolution Downscale", "The level of downscaling to be applied to the reflection"), ref v_target.v_dynRes, new GUIContent[]{new GUIContent("Full Resolution"),new GUIContent("Half Resolution"), new GUIContent("Quarter Resolution")} );
				}
				
				EditorGUILayout.Space();

				PDEditor_Slider( new GUIContent("Final Resolution Multiplier"), ref v_target.v_resMultiplier, 0.1f, 4 );

				EditorGUILayout.Space();
			}
			EditorGUILayout.Separator();
		}
		PDEditor_EndFold();

		if ( PDEditor_BeginFold("Optimization Settings", ref b_folds[2]) ){
			
			if ( v_target.b_useReflectionsPool ){
				PDEditor_Toggle( new GUIContent("Share Reflection Cameras", "Should the reflection cameras be shared across reflections? (Improves performance)"), ref v_target.b_useReflectionsPool );
				v_target.v_poolSize = PDEditor_IntField( new GUIContent("Max. reflection cameras", "When sharing reflection cameras, the maximum amount of cameras that can be created"), (int)Mathf.Clamp ( v_target.v_poolSize, 1, Mathf.Infinity ) );

				if ( v_target.v_poolSize > 32 ){
					GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
					EditorGUILayout.HelpBox( "Creating too many reflection cameras may produce unwanted rendering / garbage collection overhead", MessageType.Warning );
					GUILayout.Space(8);GUILayout.EndHorizontal();GUILayout.Space(4);
				}
			}
			else{	
				GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox( "Each reflection plane will generate its own reflection cameras.", MessageType.Warning );
				GUILayout.Space(8);GUILayout.EndHorizontal();GUILayout.Space(4);
				PDEditor_Toggle( new GUIContent("Share Reflection Cameras", "Should the reflection cameras be shared across reflections? (Improves performance)"), ref v_target.b_useReflectionsPool );
			}

			
			EditorGUILayout.Space();

		
			PDEditor_Toggle( new GUIContent("Update on Fixed Timestep","Updates this reflecetion at a fixed framerate independent of the game's framerate"), ref v_target.b_useFixedUptade );

			if ( v_target.b_useFixedUptade ){
				v_target.v_timesPerSecond = PDEditor_IntField( new GUIContent("Fixed Framerate"), (int)Mathf.Clamp ( v_target.v_timesPerSecond, 1, Mathf.Infinity ) );

				if ( v_target.v_timesPerSecond < 28 ){
					GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
					EditorGUILayout.HelpBox( "Using a very low update frequency may produce evident stuttering in the reflections", MessageType.Warning );
					GUILayout.Space(8);GUILayout.EndHorizontal();GUILayout.Space(4);
				}
			}

			if ( PDEditor_Popup( new GUIContent("Pixel Lights"), v_target.v_pixelLights<0?0:1, new GUIContent[]{ new GUIContent("User Settings"), new GUIContent("Custom") } ) == 1 ){
				if ( v_target.v_pixelLights < 0 ) {
					v_target.v_pixelLights = 0;
				}
				v_target.v_pixelLights = PDEditor_IntField( new GUIContent("Amount of Pixel Lights"), Mathf.Clamp ( v_target.v_pixelLights, 0,8 ) );
			}
			else{
				v_target.v_pixelLights = -1;
			}
			
			var disableOnDistance = v_target.v_disableOnDistance>-1;
			PDEditor_Toggle( new GUIContent("Disable when far away", "Stop updating this reflection when it is far away from the camera"), ref disableOnDistance );
			if ( disableOnDistance ){
				if ( v_target.v_disableOnDistance < 0 ) {
					v_target.v_disableOnDistance = 0;
				}
				v_target.v_disableOnDistance = PDEditor_FloatField( new GUIContent("Disable at Distance"), Mathf.Clamp ( v_target.v_disableOnDistance, 0, Mathf.Infinity ) );
			}
			else{
				v_target.v_disableOnDistance = -1;
			}

			EditorGUILayout.Separator();

			PDEditor_Toggle( new GUIContent("Simplify Reflected Terrains"), ref v_target.b_simplifyLandscapes );
			
			if ( v_target.b_simplifyLandscapes )
				v_target.v_agressiveness =  (int)((LandscapeSimp)PDEditor_EnumPopup( new GUIContent("Simplification Strength"), (LandscapeSimp)v_target.v_agressiveness ));

			EditorGUILayout.Separator();

			PDEditor_CenteredLabel("DYNAMIC RESOLUTION");

			EditorGUILayout.Space();

			if ( !v_target.v_staticTexture ){
				PDEditor_Toggle(new GUIContent("Use Dynamic Resolution", "Dynamically adjust the reflection's resolution based on its distance to the camera"), ref v_target.b_useDynamicResolution );

				if ( v_target.b_useDynamicResolution ){
					v_target.v_minMaxDistance.x = PDEditor_FloatField(new GUIContent("Min. Distance", "When the camera is closer than min. distance the reflection will be rendered at full quality. When it is further away than min. distance but closer than max. distance, it will be rendered at half resolution"), v_target.v_minMaxDistance.x );
					v_target.v_minMaxDistance.y = PDEditor_FloatField(new GUIContent("Max. Distance", "When the camera is closer than max. distance the reflection will be rendered at half quality. When it is further away than max. distance it will be rendered at quarter resolution"), v_target.v_minMaxDistance.y );
				}
			}
			else{
				GUILayout.Space(4);GUILayout.BeginHorizontal();GUILayout.Space(8);
				EditorGUILayout.HelpBox("Dynamic resolution cannot be used with static/shared reflection textures. It is only compatible with dynamically generated reflections. To use dynamic resolution, please set the static/shared reflection texture to null.", MessageType.Warning);
				GUILayout.Space(8);GUILayout.EndHorizontal();GUILayout.Space(4);
			}

			EditorGUILayout.Space();
			

			EditorGUI.indentLevel--;
		}
		PDEditor_EndFold();


		if ( PDEditor_BeginFold( "Legacy VR & Compatibility", ref b_folds[3] ) ){
			PDEditor_Toggle( new GUIContent("Explicit Cams Only", "Will render the reflections only from cameras called 'ExplicitCamera'. Read the manual for more information"), ref v_target.b_useExplicitCameras );
			GUILayout.Space(8);
		}
		PDEditor_EndFold();

		GUILayout.Space(2);

        var tempStyle = new GUIStyle();
        tempStyle.normal.textColor = new Color(0.75f,0.75f,0.75f);
        tempStyle.fontSize = 9;
        tempStyle.fontStyle = FontStyle.Italic;
        GUILayout.BeginHorizontal();GUILayout.FlexibleSpace();
        GUILayout.Label("PIDI - Planar Reflections©. Version 1.8", tempStyle );
        GUILayout.FlexibleSpace();GUILayout.EndHorizontal();


		GUILayout.Space(8);
		GUILayout.EndVertical(); GUILayout.EndHorizontal();
		GUI.skin = tSkin;
	}


	public static LayerMask LayerMaskField (string label, LayerMask selected) {
		
		List<string> layers = null;
		string[] layerNames = null;
		
		if (layers == null) {
			layers = new List<string>();
			layerNames = new string[4];
		} else {
			layers.Clear ();
		}
		
		int emptyLayers = 0;
		for (int i=0;i<32;i++) {
			string layerName = LayerMask.LayerToName (i);
			
			if (layerName != "") {
				
				for (;emptyLayers>0;emptyLayers--) layers.Add ("Layer "+(i-emptyLayers));
				layers.Add (layerName);
			} else {
				emptyLayers++;
			}
		}
		
		if (layerNames.Length != layers.Count) {
			layerNames = new string[layers.Count];
		}
		for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
		
		selected.value =  EditorGUILayout.MaskField (label,selected.value,layerNames);
		
		return selected;
	}


	public Camera[] ResizeArray(Camera[] originalArray, int newSize){
		var newArray = new Camera[newSize];

		if ( newSize > originalArray.Length ){
			for ( int i = 0; i < originalArray.Length; i++){
				newArray[i] = originalArray[i];
			}
			return newArray;
		}
		else{
		for ( int i = 0; i < newSize; i++){
				newArray[i] = originalArray[i];
			}
			return newArray;
		}
	}



	#region GENERIC PIDI EDITOR FUNCTIONS

	public bool PDEditor_BeginFold( string label, ref AnimBool fold ){
            if ( GUILayout.Button(label, pidiSkin.button ) ){
                fold.target = !fold.target;
            }

            var b = EditorGUILayout.BeginFadeGroup( fold.faded );
            if ( b ){ 
                GUILayout.Space(8);}
            return b;
        }


        public void PDEditor_EndFold( ){
            EditorGUILayout.EndFadeGroup();
        }

	public void PDEditor_Toggle( GUIContent label, ref bool value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = GUILayout.Toggle( value, "", GUILayout.Width(16) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_TextField( GUIContent label, ref string value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = GUILayout.TextField( value, GUILayout.MinWidth(64), GUILayout.MaxWidth(180) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public Enum PDEditor_EnumPopup( GUIContent label, Enum value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            var x = EditorGUILayout.EnumPopup( value, pidiSkin.button, GUILayout.MinWidth(64), GUILayout.MaxWidth(200) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            return x;
        }


        public void PDEditor_ObjectField<T> ( GUIContent label, ref T value, bool fromScene )where T:UnityEngine.Object{
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(180));
            GUILayout.FlexibleSpace();
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = (T)EditorGUILayout.ObjectField( value, typeof(T), fromScene, GUILayout.MinWidth(64), GUILayout.MaxWidth(180) );
            GUI.skin = pidiSkin;
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Slider( GUIContent label, ref float value, float min, float max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Slider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


		public void PDEditor_IntSlider( GUIContent label, ref int value, int min, int max ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntSlider( value, min, max, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Vector2( GUIContent label, ref Vector2 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector2Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Vector3( GUIContent label, ref Vector3 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector3Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Vector4( GUIContent label, ref Vector4 value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            value = EditorGUILayout.Vector4Field( "", value, GUILayout.MinWidth(100) );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }


        public void PDEditor_Color( GUIContent label, ref Color value ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(145));
            GUILayout.FlexibleSpace();
            GUILayout.Space(4);
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            value = EditorGUILayout.ColorField( "", value, GUILayout.MinWidth(100) );
            GUI.skin = pidiSkin;
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }

        public void PDEditor_Popup( GUIContent label, ref int value, params GUIContent[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


		public int PDEditor_Popup( GUIContent label, int value, params GUIContent[] items ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label(label, GUILayout.Width(175));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.Popup( value, items, pidiSkin.button, GUILayout.MaxWidth(256) );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
			return value;
        }


        public void PDEditor_CenteredLabel( string label ){
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(label);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        public void PDEditor_FloatField( GUIContent label, ref float value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.FloatField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }

		public float PDEditor_FloatField( GUIContent label, float value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.FloatField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
			return value;
        }

		public void PDEditor_IntField( GUIContent label, ref int value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
        }


		public int PDEditor_IntField( GUIContent label, int value, bool overZero = true ){
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.Label( label, GUILayout.Width(170));
            GUILayout.FlexibleSpace();
            value = EditorGUILayout.IntField("", value, pidiSkin.textField, GUILayout.MinWidth(25), GUILayout.MaxWidth(64), GUILayout.MinHeight(20) );

            if ( overZero )
                value = Mathf.Max( value, 0 );
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
			return value;
        }


		public void PDEditor_LayerMaskField ( GUIContent label, ref LayerMask selected) {
		
		List<string> layers = null;
		string[] layerNames = null;
		
		if (layers == null) {
			layers = new List<string>();
			layerNames = new string[4];
		} else {
			layers.Clear ();
		}
		
		int emptyLayers = 0;
		for (int i=0;i<32;i++) {
			string layerName = LayerMask.LayerToName (i);
			
			if (layerName != "") {
				
				for (;emptyLayers>0;emptyLayers--) layers.Add ("Layer "+(i-emptyLayers));
				layers.Add (layerName);
			} else {
				emptyLayers++;
			}
		}
		
		if (layerNames.Length != layers.Count) {
			layerNames = new string[layers.Count];
		}
		for (int i=0;i<layerNames.Length;i++) layerNames[i] = layers[i];
		
		GUILayout.BeginHorizontal();
        GUILayout.Space(12);
        GUILayout.Label(label, GUILayout.Width(175));
        GUILayout.FlexibleSpace();

		selected.value =  EditorGUILayout.MaskField (selected.value,layerNames, pidiSkin.button,GUILayout.MaxWidth(200) );
		
		GUILayout.Space(4);
        GUILayout.EndHorizontal();
		
		}


	#endregion


}
