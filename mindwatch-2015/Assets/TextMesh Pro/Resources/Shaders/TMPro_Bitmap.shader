Shader "TMPro/Bitmap" {

Properties {
	_MainTex		("Font Atlas", 2D) = "white" {}
	_FillTex		("Font Texture", 2D) = "white" {}
	_Color			("Text Color", Color) = (1,1,1,1)
	_DiffusePower	("Diffuse Power", Range(1.0,4.0)) = 1.0
	[Enum(One,1,SrcAlpha,5] _Blend2 ("Blend mode subset", Float) = 1
}

SubShader {

	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

	Lighting Off
	Cull Off
	ZTest Always
	ZWrite Off
	Fog { Mode Off }
	Blend SrcAlpha OneMinusSrcAlpha

	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest

		#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord0 : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
		};

		struct v2f {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord0 : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
		};

		sampler2D 	_MainTex;
		sampler2D 	_FillTex;
		float4		_FillTex_ST;
		fixed4		_Color;
		float		_DiffusePower;

		float2 UnpackUV(float uv)
		{
			return float2(floor(uv) * 4.0 / 4096.0, frac(uv) * 4.0);
		}

		v2f vert (appdata_t v)
		{
			v2f o;
			//o.vertex = UnityPixelSnap(mul(UNITY_MATRIX_MVP, v.vertex));
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.color = v.color;
			if(o.color.a > .5) o.color.a -= .5;
			o.color.a *= 2;
			o.color *= _Color;
			o.color.rgb *= _DiffusePower;
			o.texcoord0 = v.texcoord0;
			o.texcoord1 = TRANSFORM_TEX(UnpackUV(v.texcoord1),_FillTex);
			return o;
		}

		fixed4 frag (v2f i) : COLOR
		{
			fixed4 col = tex2D(_FillTex, i.texcoord1) * i.color;
			col.a *= tex2D(_MainTex, i.texcoord0).a;
			return col;
		}
		ENDCG
	}
}

SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Lighting Off Cull Off ZTest Always ZWrite Off Fog { Mode Off }
	Blend SrcAlpha OneMinusSrcAlpha
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord0
	}
	Pass {
		SetTexture [_MainTex] {
			constantColor [_Color] combine constant * primary, constant * texture
		}
	}
}
}
