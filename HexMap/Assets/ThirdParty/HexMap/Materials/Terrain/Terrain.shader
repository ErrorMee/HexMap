Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("平地颜色", 2DArray) = "white" {}
		_GridTex ("Grid Texture", 2D) = "white" {}
		_ElevationTex("峭壁颜色", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
		_BackgroundColor ("Background Color", Color) = (0,0,0)
		_CliffColor("峭壁叠加1", Color) = (1, 0.95, 0.75, 1)
		_RockColor("峭壁叠加2", Color) = (1, 0.95, 0.75, 1)
		_StepColor("台阶颜色", Color) = (1, 0.95, 0.75, 1)
		_SandColor("视距颜色", Color) = (1, 0.95, 0.75, 1)
		_ElevationStep("ElevationStep", Range(1,5)) = 2.1
		_Focus("Focus", Vector) = (50, 50, 20, 60)
		[Toggle(SHOW_MAP_DATA)]_ShowMapData ("Show Map Data", Float) = 0

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf StandardSpecular fullforwardshadows vertex:vert
		#pragma target 3.5

		#pragma multi_compile _ GRID_ON
		#pragma multi_compile _ HEX_MAP_EDIT_MODE

		#pragma shader_feature SHOW_MAP_DATA

		#include "../HexMetrics.cginc"
		#include "../HexCellData.cginc"

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

		sampler2D _GridTex;
		sampler2D _ElevationTex;
		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;
		half3 _BackgroundColor;
		fixed4 _CliffColor;
		fixed4 _RockColor;
		fixed4 _StepColor;
		fixed4 _SandColor;
		half _ElevationStep;
		fixed4 _Focus;

		struct Input {
			float4 color : COLOR;
			float3 worldPos;
			float3 terrain;
			float4 visibility;
			fixed3 worldNormal : TEXCOORD0;
			#if defined(SHOW_MAP_DATA)
				float mapData;
			#endif
		};

		void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

			data.terrain.x = cell0.w;
			data.terrain.y = cell1.w;
			data.terrain.z = cell2.w;

			data.visibility.x = cell0.x;
			data.visibility.y = cell1.x;
			data.visibility.z = cell2.x;
			data.visibility.xyz = lerp(0.25, 1, data.visibility.xyz);
			data.visibility.w =
				cell0.y * v.color.x + cell1.y * v.color.y + cell2.y * v.color.z;

			data.worldNormal = UnityObjectToWorldNormal(v.normal);
			
			#if defined(SHOW_MAP_DATA)
				data.mapData = cell0.z * v.color.x + cell1.z * v.color.y +
					cell2.z * v.color.z;
			#endif
				
		}

		fixed4 GetRockColor(Input IN)
		{
			float tilingScale = 6 * TILING_SCALE;

			float height = IN.worldPos.y + step(IN.worldNormal.y, 0.85) * sin(length(IN.worldPos.xz) * 1.5) / 7;

			float rawIndex = height / _ElevationStep + _ElevationStep / 3;
			int index = (rawIndex);

			fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(
				IN.worldPos.xz * tilingScale,
				index));
			
			if (IN.worldNormal.y < 0.85)
			{
				c = tex2D(_ElevationTex, float2(length(IN.worldPos.xz), height) / (_ElevationStep * 7));
				c.rgb = lerp(c.rgb, _CliffColor.rgb, _CliffColor.a);
			}

			float blockHeight = height % _ElevationStep;
			float isStep = step(0.85, IN.worldNormal.y) * 
				step(_ElevationStep * 0.25, blockHeight) * step(blockHeight, _ElevationStep * 0.75);
			c = lerp(c, _StepColor, isStep * 0.5);

			float centerDis = length(IN.worldPos.xz - _Focus.xy);

			float glow = smoothstep(0, 1, (centerDis - _Focus.z) / _Focus.w);

			c = lerp(c, _SandColor, glow);

			return c;
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			fixed4 c;

			c = GetRockColor(IN);

			/*float h = IN.worldPos.y % 2 + sin(IN.worldPos.x * 4) / 5;
			c.rgb = lerp(c.rgb, step(h, 1) * _RockColor.rgb + step(1, h) * _CliffColor.rgb, 
				(step(h, 1) * _RockColor.a + step(1, h) * _CliffColor.a) * step(IN.worldNormal.y, 0.85));*/

			fixed4 grid = 1;
			#if defined(GRID_ON)
				float2 gridUV = IN.worldPos.xz;
				gridUV.x *= 1 / (4 * 8.66025404);
				gridUV.y *= 1 / (2 * 15.0);
				grid = tex2D(_GridTex, gridUV);
			#endif

			float explored = IN.visibility.w;
			o.Albedo = c.rgb * grid * _Color * explored;
			#if defined(SHOW_MAP_DATA)
				o.Albedo = IN.mapData * grid;
			#endif
			o.Specular = _Specular * explored;
			o.Smoothness = _Glossiness;
			o.Occlusion = explored;
			o.Emission = _BackgroundColor * (1 -  explored);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}