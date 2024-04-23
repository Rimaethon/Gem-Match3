// Made with Amplify Shader Editor
// Available at the Unity Asset Store - https://assetstore.unity.com/packages/tools/visual-scripting/amplify-shader-editor-68570?aid=1011lqxW8 
Shader "CartoonCoffee/Particle Basic"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_NoiseTexture("Noise Texture", 2D) = "white" {}
		[Toggle(_ENABLEADJUSTCOLOR_ON)] _EnableAdjustColor("Enable Adjust Color", Float) = 0
		_AdjustColorFade("Adjust Color: Fade", Range( 0 , 1)) = 1
		_AdjustColorBrightness("Adjust Color: Brightness", Float) = 1
		_AdjustColorContrast("Adjust Color: Contrast", Float) = 1
		_AdjustColorSaturation("Adjust Color: Saturation", Float) = 1
		_AdjustColorHueShift("Adjust Color: Hue Shift", Range( 0 , 1)) = 0
		[Toggle(_ENABLECUSTOMFADE_ON)] _EnableCustomFade("Enable Custom Fade", Float) = 0
		_CustomFadeFadeMask("Custom Fade: Fade Mask", 2D) = "white" {}
		_CustomFadeSmoothness("Custom Fade: Smoothness", Float) = 2
		_CustomFadeNoiseScale("Custom Fade: Noise Scale", Vector) = (1,1,0,0)
		_CustomFadeNoiseFactor("Custom Fade: Noise Factor", Range( 0 , 0.5)) = 0
		_CustomFadeAlpha("Custom Fade: Alpha", Range( 0 , 1)) = 1
		[Toggle(_ENABLESPLITTONING_ON)] _EnableSplitToning("Enable Split Toning", Float) = 0
		_SplitToningFade("Split Toning: Fade", Range( 0 , 1)) = 1
		[HDR]_SplitToningHighlightsColor("Split Toning: Highlights Color", Color) = (1,0.1,0.1,0)
		[HDR]_SplitToningShadowsColor("Split Toning: Shadows Color", Color) = (0.1,0.4000002,1,0)
		_SplitToningContrast("Split Toning: Contrast", Float) = 1
		_SplitToningBalance("Split Toning: Balance", Float) = 1
		_SplitToningShift("Split Toning: Shift", Range( -1 , 1)) = 0
		[Toggle(_ENABLEBLACKTINT_ON)] _EnableBlackTint("Enable Black Tint", Float) = 0
		_BlackTintFade("Black Tint: Fade", Range( 0 , 1)) = 1
		[HDR]_BlackTintColor("Black Tint: Color", Color) = (1,0,0,0)
		_BlackTintPower("Black Tint: Power", Float) = 2
		[Toggle(_ENABLEALPHATINT_ON)] _EnableAlphaTint("Enable Alpha Tint", Float) = 0
		_AlphaTintFade("Alpha Tint: Fade", Range( 0 , 1)) = 1
		[HDR]_AlphaTintColor("Alpha Tint: Color", Color) = (23.96863,1.254902,23.96863,0)
		_AlphaTintPower("Alpha Tint: Power", Float) = 1
		_AlphaTintMinAlpha("Alpha Tint: Min Alpha", Range( 0 , 1)) = 0.05
		[Toggle(_ENABLEUVDISTORT_ON)] _EnableUVDistort("Enable UV Distort", Float) = 0
		_UVDistortFade("UV Distort: Fade", Range( 0 , 1)) = 1
		[NoScaleOffset]_UVDistortShaderMask("UV Distort: Shader Mask", 2D) = "white" {}
		_UVDistortSpace("UV Distort: Space", Int) = 0
		_UVDistortFrom("UV Distort: From", Vector) = (-0.02,-0.02,0,0)
		_UVDistortTo("UV Distort: To", Vector) = (0.02,0.02,0,0)
		_UVDistortSpeed("UV Distort: Speed", Vector) = (2,2,0,0)
		_UVDistortNoiseScale("UV Distort: Noise Scale", Vector) = (0.1,0.1,0,0)
		[Toggle(_ENABLEUVSCROLL_ON)] _EnableUVScroll("Enable UV Scroll", Float) = 0
		_UVScrollSpeed("UV Scroll: Speed", Vector) = (0.2,0,0,0)
		[Toggle(_ENABLETEXTURESPRITE_ON)] _EnableTextureSprite("Enable Texture Sprite", Float) = 0
		_TextureFadeTexture("Texture Fade: Texture", 2D) = "white" {}
		_TextureFadeFrom("Texture Fade: From", Float) = 0
		[ASEEnd]_TextureFadeTo("Texture Fade: To", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			ColorMask RGBA
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _ENABLEADJUSTCOLOR_ON
				#pragma shader_feature_local _ENABLEALPHATINT_ON
				#pragma shader_feature_local _ENABLEBLACKTINT_ON
				#pragma shader_feature_local _ENABLESPLITTONING_ON
				#pragma shader_feature_local _ENABLECUSTOMFADE_ON
				#pragma shader_feature_local _ENABLETEXTURESPRITE_ON
				#pragma shader_feature_local _ENABLEUVSCROLL_ON
				#pragma shader_feature_local _ENABLEUVDISTORT_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float2 _UVDistortFrom;
				uniform float2 _UVDistortTo;
				uniform sampler2D _NoiseTexture;
				uniform int _UVDistortSpace;
				float4 _MainTex_TexelSize;
				uniform float2 _UVDistortSpeed;
				uniform float2 _UVDistortNoiseScale;
				uniform float _UVDistortFade;
				uniform sampler2D _UVDistortShaderMask;
				uniform float4 _UVDistortShaderMask_ST;
				uniform float2 _UVScrollSpeed;
				uniform sampler2D _TextureFadeTexture;
				uniform float _TextureFadeFrom;
				uniform float _TextureFadeTo;
				uniform sampler2D _CustomFadeFadeMask;
				uniform float2 _CustomFadeNoiseScale;
				uniform float _CustomFadeNoiseFactor;
				uniform float _CustomFadeSmoothness;
				uniform float _CustomFadeAlpha;
				uniform float4 _SplitToningShadowsColor;
				uniform float4 _SplitToningHighlightsColor;
				uniform float _SplitToningShift;
				uniform float _SplitToningBalance;
				uniform float _SplitToningContrast;
				uniform float _SplitToningFade;
				uniform float4 _BlackTintColor;
				uniform float _BlackTintPower;
				uniform float _BlackTintFade;
				uniform float4 _AlphaTintColor;
				uniform float _AlphaTintPower;
				uniform float _AlphaTintFade;
				uniform float _AlphaTintMinAlpha;
				uniform float _AdjustColorHueShift;
				uniform float _AdjustColorSaturation;
				uniform float _AdjustColorContrast;
				uniform float _AdjustColorBrightness;
				uniform float _AdjustColorFade;
				float3 HSVToRGB( float3 c )
				{
					float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
					float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
					return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
				}
				
				float3 RGBToHSV(float3 c)
				{
					float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
					float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
					float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
					float d = q.x - min( q.w, q.y );
					float e = 1.0e-10;
					return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
				}


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					
					o.ase_texcoord4 = v.vertex;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 texCoord67 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float2 appendResult2_g105 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
					float2 ifLocalVar2_g104 = 0;
					if( _UVDistortSpace > 1.0 )
					ifLocalVar2_g104 = (ase_worldPos).xy;
					else if( _UVDistortSpace == 1.0 )
					ifLocalVar2_g104 = (i.ase_texcoord4.xyz).xy;
					else if( _UVDistortSpace < 1.0 )
					ifLocalVar2_g104 = ( i.texcoord.xy / ( 100.0 / appendResult2_g105 ) );
					float2 lerpResult21_g103 = lerp( _UVDistortFrom , _UVDistortTo , tex2D( _NoiseTexture, ( ( ifLocalVar2_g104 + ( _UVDistortSpeed * _Time.y ) ) * _UVDistortNoiseScale ) ).r);
					float2 appendResult2_g107 = (float2(_MainTex_TexelSize.z , _MainTex_TexelSize.w));
					float2 uv_UVDistortShaderMask = i.texcoord.xy * _UVDistortShaderMask_ST.xy + _UVDistortShaderMask_ST.zw;
					float4 tex2DNode3_g106 = tex2D( _UVDistortShaderMask, uv_UVDistortShaderMask );
					#ifdef _ENABLEUVDISTORT_ON
					float2 staticSwitch65 = ( texCoord67 + ( lerpResult21_g103 * ( 100.0 / appendResult2_g107 ) * ( _UVDistortFade * ( tex2DNode3_g106.r * tex2DNode3_g106.a ) ) ) );
					#else
					float2 staticSwitch65 = texCoord67;
					#endif
					#ifdef _ENABLEUVSCROLL_ON
					float2 staticSwitch91 = ( ( ( _UVScrollSpeed * _Time.y ) + staticSwitch65 ) % float2( 1,1 ) );
					#else
					float2 staticSwitch91 = staticSwitch65;
					#endif
					float4 tex2DNode17 = tex2D( _MainTex, staticSwitch91 );
					float4 temp_output_1_0_g166 = tex2DNode17;
					float3 appendResult15_g166 = (float3(temp_output_1_0_g166.rgb));
					float lerpResult7_g166 = lerp( _TextureFadeFrom , _TextureFadeTo , i.color.a);
					float4 tex2DNode2_g166 = tex2D( _TextureFadeTexture, ( ( ( i.texcoord.xy - float2( 0.5,0.5 ) ) / max( lerpResult7_g166 , 0.0001 ) ) + float2( 0.5,0.5 ) ) );
					float4 appendResult16_g166 = (float4(appendResult15_g166 , ( temp_output_1_0_g166.a * ( 1.0 - ( tex2DNode2_g166.r * tex2DNode2_g166.a ) ) )));
					#ifdef _ENABLETEXTURESPRITE_ON
					float4 staticSwitch97 = appendResult16_g166;
					#else
					float4 staticSwitch97 = ( tex2DNode17 * i.color );
					#endif
					float4 temp_output_1_0_g168 = tex2DNode17;
					float2 temp_output_57_0_g168 = staticSwitch91;
					float4 tex2DNode3_g168 = tex2D( _CustomFadeFadeMask, temp_output_57_0_g168 );
					float clampResult37_g168 = clamp( ( ( ( i.color.a * 2.0 ) - 1.0 ) + ( tex2DNode3_g168.r + ( tex2D( _NoiseTexture, ( temp_output_57_0_g168 * _CustomFadeNoiseScale ) ).r * _CustomFadeNoiseFactor ) ) ) , 0.0 , 1.0 );
					float4 appendResult13_g168 = (float4(( float4( (i.color).rgb , 0.0 ) * temp_output_1_0_g168 ).rgb , ( temp_output_1_0_g168.a * pow( clampResult37_g168 , ( _CustomFadeSmoothness / max( tex2DNode3_g168.r , 0.05 ) ) ) * _CustomFadeAlpha )));
					#ifdef _ENABLECUSTOMFADE_ON
					float4 staticSwitch64 = appendResult13_g168;
					#else
					float4 staticSwitch64 = staticSwitch97;
					#endif
					float4 temp_output_1_0_g170 = staticSwitch64;
					float4 break2_g171 = temp_output_1_0_g170;
					float temp_output_3_0_g170 = ( ( break2_g171.x + break2_g171.y + break2_g171.z ) / 3.0 );
					float clampResult25_g170 = clamp( ( ( ( ( temp_output_3_0_g170 + _SplitToningShift ) - 0.5 ) * _SplitToningBalance ) + 0.5 ) , 0.0 , 1.0 );
					float3 lerpResult6_g170 = lerp( (_SplitToningShadowsColor).rgb , (_SplitToningHighlightsColor).rgb , clampResult25_g170);
					float temp_output_9_0_g172 = max( _SplitToningContrast , 0.0 );
					float saferPower7_g172 = abs( ( temp_output_3_0_g170 + ( 0.1 * max( ( 1.0 - temp_output_9_0_g172 ) , 0.0 ) ) ) );
					float3 lerpResult11_g170 = lerp( (temp_output_1_0_g170).rgb , ( lerpResult6_g170 * pow( saferPower7_g172 , temp_output_9_0_g172 ) ) , _SplitToningFade);
					float4 appendResult18_g170 = (float4(lerpResult11_g170 , temp_output_1_0_g170.a));
					#ifdef _ENABLESPLITTONING_ON
					float4 staticSwitch93 = appendResult18_g170;
					#else
					float4 staticSwitch93 = staticSwitch64;
					#endif
					float4 temp_output_1_0_g173 = staticSwitch93;
					float3 temp_output_4_0_g173 = (temp_output_1_0_g173).rgb;
					float4 break12_g173 = temp_output_1_0_g173;
					float3 lerpResult7_g173 = lerp( temp_output_4_0_g173 , ( temp_output_4_0_g173 + (_BlackTintColor).rgb ) , pow( ( 1.0 - min( max( max( break12_g173.r , break12_g173.g ) , break12_g173.b ) , 1.0 ) ) , max( _BlackTintPower , 0.001 ) ));
					float3 lerpResult13_g173 = lerp( temp_output_4_0_g173 , lerpResult7_g173 , _BlackTintFade);
					float4 appendResult11_g173 = (float4(lerpResult13_g173 , break12_g173.a));
					#ifdef _ENABLEBLACKTINT_ON
					float4 staticSwitch79 = appendResult11_g173;
					#else
					float4 staticSwitch79 = staticSwitch93;
					#endif
					float4 temp_output_1_0_g174 = staticSwitch79;
					float saferPower11_g174 = abs( ( 1.0 - temp_output_1_0_g174.a ) );
					float3 lerpResult4_g174 = lerp( (temp_output_1_0_g174).rgb , (_AlphaTintColor).rgb , ( pow( saferPower11_g174 , _AlphaTintPower ) * _AlphaTintFade * step( _AlphaTintMinAlpha , temp_output_1_0_g174.a ) ));
					float4 appendResult13_g174 = (float4(lerpResult4_g174 , temp_output_1_0_g174.a));
					#ifdef _ENABLEALPHATINT_ON
					float4 staticSwitch86 = appendResult13_g174;
					#else
					float4 staticSwitch86 = staticSwitch79;
					#endif
					float4 break2_g175 = staticSwitch86;
					float3 appendResult4_g175 = (float3(break2_g175.r , break2_g175.g , break2_g175.b));
					float3 hsvTorgb16_g175 = RGBToHSV( appendResult4_g175 );
					float clampResult18_g175 = clamp( ( hsvTorgb16_g175.y * _AdjustColorSaturation ) , 0.0 , 1.0 );
					float temp_output_9_0_g176 = max( _AdjustColorContrast , 0.0 );
					float saferPower7_g176 = abs( ( hsvTorgb16_g175.z + ( 0.1 * max( ( 1.0 - temp_output_9_0_g176 ) , 0.0 ) ) ) );
					float3 hsvTorgb24_g175 = HSVToRGB( float3(( hsvTorgb16_g175.x + _AdjustColorHueShift ),clampResult18_g175,( pow( saferPower7_g176 , temp_output_9_0_g176 ) * _AdjustColorBrightness )) );
					float3 lerpResult9_g175 = lerp( appendResult4_g175 , hsvTorgb24_g175 , _AdjustColorFade);
					float4 appendResult3_g175 = (float4(lerpResult9_g175 , break2_g175.a));
					#ifdef _ENABLEADJUSTCOLOR_ON
					float4 staticSwitch84 = appendResult3_g175;
					#else
					float4 staticSwitch84 = staticSwitch86;
					#endif
					

					fixed4 col = staticSwitch84;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "CartoonCoffeeFireVFX.ParticleShaderGUI"
	
	Fallback "2"
}
/*ASEBEGIN
Version=18935
236;102;1485;805;2075.314;834.281;1.937534;True;True
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;41;-3659.831,-724.3906;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;88;-3534.399,678.8388;Inherit;True;Property;_NoiseTexture;Noise Texture;0;0;Create;True;0;0;0;False;0;False;4addb5285d2d96b46bcc3d03bf698f23;4addb5285d2d96b46bcc3d03bf698f23;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;67;-3099.337,-133.7909;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;89;-2788.648,33.35318;Inherit;False;_UVDistort;36;;103;d6b8c102b9317a0418c08eb00598bec7;0;3;1;FLOAT2;0,0;False;26;SAMPLER2D;;False;3;SAMPLER2D;;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;65;-2518.289,-108.6957;Inherit;False;Property;_EnableUVDistort;Enable UV Distort;35;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;92;-2158.013,309.9075;Inherit;False;_UVScroll;46;;122;be39ff8debe04f84baeada43b5b8aeb7;0;1;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;91;-1898.116,202.6187;Inherit;False;Property;_EnableUVScroll;Enable UV Scroll;45;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;17;-1515.499,-172.1698;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;103;-1094.157,-687.4542;Inherit;False;TextureFade;49;;166;b6b9d854da148684082ce91c9e92c063;0;1;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;78;-1098.72,-294.451;Inherit;False;TintVertex;-1;;167;b0b94dd27c0f3da49a89feecae766dcc;0;1;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;90;-1154.998,126.8534;Inherit;False;_CustomFade;9;;168;09a17a2b3ff778e4baeae7d542f88dd6;0;3;57;FLOAT2;0,0;False;56;SAMPLER2D;;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;97;-756.3032,-725.9235;Inherit;False;Property;_EnableTextureSprite;Enable Texture Sprite;48;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;64;-607.1774,-418.1753;Inherit;False;Property;_EnableCustomFade;Enable Custom Fade;8;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;94;-101.8765,-215.0642;Inherit;False;_SplitToning;17;;170;6b87c8196f94bcd478491aaa714b31ef;0;1;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;93;294.7838,-398.5298;Inherit;False;Property;_EnableSplitToning;Enable Split Toning;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;81;746.9941,-187.6142;Inherit;False;_BlackTint;25;;173;e72823b8923579647a619869b654ace9;0;1;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;79;1109.666,-351.9158;Inherit;False;Property;_EnableBlackTint;Enable Black Tint;24;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;85;1573.545,-214.9836;Inherit;False;_AlphaTint;30;;174;ae9f8d24855c66643be02b2aa90f050e;0;1;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;86;1907.981,-348.1771;Inherit;False;Property;_EnableAlphaTint;Enable Alpha Tint;29;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;82;2407.541,-244.2802;Inherit;False;_AdjustColor;2;;175;e7083192d13fe334cab64b3d59374f2b;0;1;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;84;2708.779,-354.0338;Inherit;False;Property;_EnableAdjustColor;Enable Adjust Color;1;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;75;3156.809,-345.4613;Float;False;True;-1;2;CartoonCoffee.ParticleShaderGUI;0;7;CartoonCoffee/Particle Basic;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;True;2;5;False;-1;10;False;-1;3;1;False;-1;10;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;2;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;89;1;67;0
WireConnection;89;26;88;0
WireConnection;89;3;41;0
WireConnection;65;1;67;0
WireConnection;65;0;89;0
WireConnection;92;1;65;0
WireConnection;91;1;65;0
WireConnection;91;0;92;0
WireConnection;17;0;41;0
WireConnection;17;1;91;0
WireConnection;103;1;17;0
WireConnection;78;1;17;0
WireConnection;90;57;91;0
WireConnection;90;56;88;0
WireConnection;90;1;17;0
WireConnection;97;1;78;0
WireConnection;97;0;103;0
WireConnection;64;1;97;0
WireConnection;64;0;90;0
WireConnection;94;1;64;0
WireConnection;93;1;64;0
WireConnection;93;0;94;0
WireConnection;81;1;93;0
WireConnection;79;1;93;0
WireConnection;79;0;81;0
WireConnection;85;1;79;0
WireConnection;86;1;79;0
WireConnection;86;0;85;0
WireConnection;82;1;86;0
WireConnection;84;1;86;0
WireConnection;84;0;82;0
WireConnection;75;0;84;0
ASEEND*/
//CHKSM=0A1A33B91EDA370E0B66A32504165863734AB099