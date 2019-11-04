// Copyright (C) 2014 Stephan Schaem - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

// Simplified SDF shader:
// - No Shading Option (bevel / bump / env map)
// - No Glow Option
// - Softness is applied on both side of the outline

Shader "TMPro/Mobile/Distance Field" {

Properties { // Material Serialized Properties
	_FaceColor			("Face Color", Color) = (1,1,1,1)
	_FaceDilate			("Face Dilate", Range(-1,1)) = 0.0

	_OutlineColor		("Outline Color", Color) = (0,0,0,1)
	_OutlineWidth		("Outline Thickness", Range(0.0, 1.0)) = 0.0
	_OutlineSoftness	("Outline Softness", Range(0,1)) = 0.0

	_UnderlayColor		("Border Color", Color) = (0,0,0, 0.5)
	_UnderlayOffsetX 	("Border OffsetX", Range(-1,1)) = 0
	_UnderlayOffsetY 	("Border OffsetY", Range(-1,1)) = 0
	_UnderlayDilate		("Border Dilate", Range(-1,1)) = 0
	_UnderlaySoftness 	("Border Softness", Range(0,1)) = 0

	_WeightNormal		("Weight Normal", float) = 0
	_WeightBold			("Weight Bold", float) = 0.5

	// Should not be directly exposed to the user
	_ShaderFlags		("Flags", float) = 0
	_ScaleRatioA		("Scale RatioA", float) = 1
	_ScaleRatioB		("Scale RatioB", float) = 1
	_ScaleRatioC		("Scale RatioC", float) = 1

	_MainTex			("Font Atlas", 2D) = "white" {}
	_TextureWidth		("Texture Width", float) = 512
	_TextureHeight		("Texture Height", float) = 512
	_GradientScale		("Gradient Scale", float) = 5.0
	_ScaleX				("Scale X", float) = 1.0
	_ScaleY				("Scale Y", float) = 1.0
	_PerspectiveFilter	("Perspective Correction", Range(0, 1)) = 0.0

	_VertexOffsetX		("Vertex OffsetX", float) = 0
	_VertexOffsetY		("Vertex OffsetY", float) = 0
	_MaskCoord			("Mask Coords", vector) = (0,0,0,0)
	_MaskSoftnessX		("Mask SoftnessX", float) = 0
	_MaskSoftnessY		("Mask SoftnessY", float) = 0
}

SubShader {
	Tags {
		"Queue"="Transparent"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
	}

	Cull Off
	ZWrite Off
	Lighting Off
	Fog { Mode Off }
	Ztest [_ZTestMode]
	Blend One OneMinusSrcAlpha

	Pass {
		CGPROGRAM
		#pragma vertex VertShader
		#pragma fragment PixShader
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma multi_compile UNDERLAY_OFF UNDERLAY_ON UNDERLAY_INNER
		#pragma multi_compile MASK_OFF MASK_HARD MASK_SOFT

		#include "UnityCG.cginc"

		#include "TMPro_Properties.cginc"

		struct vertex_t {
			float4	vertex			: POSITION;
			float3	normal			: NORMAL;
			fixed4	color			: COLOR;
			float2	texcoord0		: TEXCOORD0;
			float2	texcoord1		: TEXCOORD1;
		};

		struct pixel_t {
			float4	vertex			: SV_POSITION;
			fixed4	faceColor		: COLOR;
			fixed4	outlineColor	: COLOR1;
			float2	texcoord0		: TEXCOORD0;
			half3	param			: TEXCOORD1;			// Scale(x), BiasIn(y), BiasOut(z)
			half2	mask			: TEXCOORD2;			// Position(xy) in object space, pixel Size(zw) in screen space
		#if (UNDERLAY_ON | UNDERLAY_INNER)
			float2	texcoord1		: TEXCOORD3;
			fixed4	underlayColor	: TEXCOORD4;
			half2	underlayParam	: TEXCOORD5;			// Scale(x), Bias(y)
		#endif
		};

		pixel_t VertShader(vertex_t input)
		{
			float opacity = input.color.a;
			float bold = step(128.0/255.0, opacity);
			opacity = (opacity - (128.0/255.0)*bold)*(255.0/127.0);

			float4 vert = input.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;
			float4 vPosition = mul(UNITY_MATRIX_MVP, vert);

			float2 pixelSize = vPosition.w;
			pixelSize /= float2(_ScaleX * _ScreenParams.x*UNITY_MATRIX_P[0][0], _ScaleY * _ScreenParams.y * UNITY_MATRIX_P[1][1]);			
			float scale = rsqrt(dot(pixelSize, pixelSize));
			scale *= input.texcoord1.y * _GradientScale * 1.5;			
			if (UNITY_MATRIX_P[3][3] == 0) scale = lerp(scale * (1 - _PerspectiveFilter), scale, abs(dot(input.normal.xyz, normalize(ObjSpaceViewDir(vert)))));			
			
			float weight = lerp(_WeightNormal, _WeightBold, bold) / _GradientScale;
			weight += _FaceDilate * _ScaleRatioA * 0.5;

			float bScale = scale;

			scale /= 1+(_OutlineSoftness*_ScaleRatioA*scale);
			float bias = (.5-weight)*scale - .5;
			float outline = _OutlineWidth*_ScaleRatioA*.5*scale;

			fixed4 faceColor = fixed4(input.color.rgb, opacity)*_FaceColor;
			fixed4 outlineColor = _OutlineColor;
			faceColor.rgb *= faceColor.a;
			outlineColor.a *= opacity;
			outlineColor.rgb *= outlineColor.a;
			outlineColor = lerp(faceColor, outlineColor, sqrt(min(1.0, (outline*2))));

		#if (UNDERLAY_ON | UNDERLAY_INNER)
			float4 underlayColor = _UnderlayColor;
			underlayColor.a *= opacity;
			underlayColor.rgb *= underlayColor.a;

			bScale /= 1+((_UnderlaySoftness*_ScaleRatioC)*bScale);
			float bBias = (.5-weight)*bScale - .5 - ((_UnderlayDilate*_ScaleRatioC)*.5*bScale);

			float x = -_UnderlayOffsetX*_ScaleRatioC*_GradientScale/_TextureWidth;
			float y = -_UnderlayOffsetY*_ScaleRatioC*_GradientScale/_TextureHeight;
			float2 bOffset = float2(x, y);
		#endif

			pixel_t output = {
				vPosition,
				faceColor,
				outlineColor,
				input.texcoord0,
				half3(scale, bias - outline, bias + outline),
				half2((abs(vert.xy-_MaskCoord.xy)-_MaskCoord.zw) * (.5/pixelSize)),
			#if (UNDERLAY_ON | UNDERLAY_INNER)
				input.texcoord0+bOffset,
				underlayColor,
				half2(bScale, bBias),
			#endif
			};

			return output;
		}

		fixed4 PixShader(pixel_t input) : COLOR
		{
			half d = tex2D(_MainTex, input.texcoord0).a * input.param.x;
			half sd = saturate(d - input.param.z);
			fixed4 c = lerp(input.outlineColor, input.faceColor, sd);
			c *= saturate(d - input.param.y);

		#if UNDERLAY_ON
			d = tex2D(_MainTex, input.texcoord1).a * input.underlayParam.x;
			c += input.underlayColor * saturate(d - input.underlayParam.y) * (1-c.a);
		#endif

		#if UNDERLAY_INNER
			half mask = saturate(d - input.param.z + (input.param.z-input.param.y)*.25);
			d = tex2D(_MainTex, input.texcoord1).a * input.underlayParam.x;
			c += input.underlayColor * (1-saturate(d - input.underlayParam.y))*mask * (1-c.a);
		#endif

		#if MASK_HARD
			half2 m = 1-saturate(input.mask.xy);
			c *= m.x*m.y;
		#endif

		#if MASK_SOFT
			half2 s = half2(_MaskSoftnessX, _MaskSoftnessY);
			half2 m = 1-saturate((max(input.mask.xy+s, 0)) / (1+s));
			c *= m.x*m.y;
		#endif

			return c;
		}
		ENDCG
	}
}

CustomEditor "TMPro_SDFMaterialEditor"
}
