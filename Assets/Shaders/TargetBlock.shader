// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Mitecraf/TargetBlock" {
	Properties{
		_MainTex("Outline Texture", 2D) = "white" {}
		_MainColor("Base Color", Color) = (0, 0, 0)
	}

		SubShader{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
			LOD 100

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma multi_compile_fog

					#include "UnityCG.cginc"

					struct appdata_t
		 {
float4 vertex : POSITION;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
float4 vertex : SV_POSITION;
half2 texcoord : TEXCOORD0;
UNITY_FOG_COORDS(1)
};

sampler2D _MainTex;
half3 _MainColor;
float4 _MainTex_ST;
float _HighlightOpacity;

v2f vert(appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
	UNITY_TRANSFER_FOG(o,o.vertex);
	return o;
}

fixed4 frag(v2f i): SV_Target
{
	fixed4 col = tex2D(_MainTex, i.texcoord);
	if (col.a == 0)
	{
		col = float4(_MainColor.rgb, _HighlightOpacity);
	}
	UNITY_APPLY_FOG(i.fogCoord, col);
	return col;
}
ENDCG
}
	}

}