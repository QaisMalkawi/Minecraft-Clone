﻿Shader "Minecraft/Transparent Blocks" {

	Properties{
		_MainTex("Block Texture Atlas", 2D) = "white" {}
		aoStrength("Ambient Occlusion Strength", Range(0.01, 4)) = 1
	}

		SubShader{

			Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
			LOD 100
			Lighting Off

			Pass {

				CGPROGRAM
					#pragma vertex vertFunction
					#pragma fragment fragFunction
					#pragma target 2.0

					#include "UnityCG.cginc"

					struct appdata
	 {

float4 vertex : POSITION;
float2 uv : TEXCOORD0;
float4 color : COLOR;

};

struct v2f
{

float4 vertex : SV_POSITION;
float2 uv : TEXCOORD0;
float4 color : COLOR;

};

sampler2D _MainTex;
float GlobalLightLevel;
float minGlobalLightLevel;
float maxGlobalLightLevel;
float aoStrength;

v2f vertFunction(appdata v)
{

v2f o;

o.vertex = UnityObjectToClipPos(v.vertex);
o.uv = v.uv;
o.color = v.color;

return o;

}

fixed4 fragFunction(v2f i): SV_Target {


	fixed4 col = tex2D(_MainTex, i.uv);

	float shade = 1 - lerp(minGlobalLightLevel, maxGlobalLightLevel, GlobalLightLevel);
	shade /= i.color.r / aoStrength;//AO
	shade /= i.color.a;//lightLevel

	clip(col.a - 0.5);
	col = lerp(col, float4(0, 0, 0, 1), shade);

	return col;

}

ENDCG

}


	}

}