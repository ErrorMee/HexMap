Shader "Custom/Terrain" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_GridTex ("Grid Texture", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Color) = (0.2, 0.2, 0.2)
		_BackgroundColor ("Background Color", Color) = (0,0,0)

		_FocusColor("视距颜色", Color) = (1, 0.95, 0.75, 1)
		_Focus("视距", Vector) = (50, 50, 20, 60)

		_PlatColor("平台颜色", Color) = (1,1,1,1)
		_FloorColor("地面颜色", Color) = (1,1,1,1)

		_UnitHeight("岩层厚度", Range(0,4)) = 1
		_HeightColor0("岩层0", Color) = (1,1,1,1)
		_HeightColor1("岩层1", Color) = (1,1,1,1)
		_HeightColor2("岩层2", Color) = (1,1,1,1)
		_HeightColor3("岩层3", Color) = (1,1,1,1)
		_HeightColor4("岩层4", Color) = (1,1,1,1)
		_HeightColor5("岩层5", Color) = (1,1,1,1)
		_HeightColor6("岩层6", Color) = (1,1,1,1)
		_HeightColor7("岩层7", Color) = (1,1,1,1)

		[NoScaleOffset] _BumpMap("法线图", 2D) = "bump" {}

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
		//#include "MySplat.cginc"

		sampler2D _GridTex;
		sampler2D _BumpMap;

		half _Glossiness;
		fixed3 _Specular;
		fixed4 _Color;
		half3 _BackgroundColor;

		fixed4 _FocusColor;
		fixed4 _Focus;

		fixed4 _PlatColor;
		fixed4 _FloorColor;
		half _UnitHeight;
		fixed4 _HeightColor0;
		fixed4 _HeightColor1;
		fixed4 _HeightColor2;
		fixed4 _HeightColor3;
		fixed4 _HeightColor4;
		fixed4 _HeightColor5;
		fixed4 _HeightColor6;
		fixed4 _HeightColor7;

		struct Input {
			float2 uv_BumpMap;
			float4 color : COLOR;
			float3 worldPos;
			float4 visibility;
			fixed3 worldNormal;
			#if defined(SHOW_MAP_DATA)
				float mapData;
			#endif
			INTERNAL_DATA
		};

		void vert (inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);

			float4 cell0 = GetCellData(v, 0);
			float4 cell1 = GetCellData(v, 1);
			float4 cell2 = GetCellData(v, 2);

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

		fixed4 GetColorful(Input IN)
		{
			float height = IN.worldPos.y;
			height += sin(length(IN.worldPos) * 1.5) * _UnitHeight * 0.1;
			height += sin(IN.worldPos.x * 0.2) * 0.5;
			height += cos(IN.worldPos.z * 0.2) * 2;

			int level = ceil(height / _UnitHeight);//-2 -1 0 1 2
			level = abs(level);//2 1 0 1 2
			level = level % 8.0;

			fixed4 intColor =
				step(0, level) * step(level, 0) * _HeightColor0
				+ step(1, level) * step(level, 1) * _HeightColor1
				+ step(2, level) * step(level, 2) * _HeightColor2
				+ step(3, level) * step(level, 3) * _HeightColor3
				+ step(4, level) * step(level, 4) * _HeightColor4
				+ step(5, level) * step(level, 5) * _HeightColor5
				+ step(6, level) * step(level, 6) * _HeightColor6
				+ step(7, level) * step(level, 7) * _HeightColor7;
			//return intColor;
			//1 0 1
			float frac101 = abs(abs(frac(height / _UnitHeight)) - 0.5) * 2;
			float fadeRang = 0.3;
			float frac101P = clamp(frac101 - (1 - fadeRang), 0, 1);
			float fade = frac101P / fadeRang;

			return lerp(intColor, 0.75, fade);
		}

		float sdCircle(in float2 test, in float3 circle)
		{
			float d = length(test - circle.xy) - circle.z;

			return d;
		}

		float sdBox(in float2 test, in float3 box)
		{
			float2 d = abs(test - box.xy) - box.zz;
			return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
		}

		float randHole(float2 co) {
			return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
		}

		float variation(float2 v1, float2 v2, float strength) {
			return sin(dot(normalize(v1), normalize(v2)) * strength) / 100.0;
		}

		fixed4 GetRockColor(Input IN)
		{
			float centerDis = length(IN.worldPos.xz - _Focus.xy);
			fixed4 colorful = GetColorful(IN);
			fixed4 rockColor;

			if (IN.worldNormal.y > 0.6)
			{
				float fdis = sin(centerDis * 0.6);
				float brightness = 1 - step(fdis, 0) * 0.025;
				
				rockColor = lerp(colorful, _FloorColor, 0.95);
				rockColor = fixed4(rockColor.rgb * brightness, 1);

				//hole
				float holeDensity = 0.25;
				float holeSize = 0.3;
				float holeMinSize = holeSize * 0.5;

				if (IN.worldPos.y < 0.25)
				{
					holeDensity = 0.1;
					holeSize = 0.2;
					holeMinSize = holeSize * 0.5;
				}

				float2 pos = IN.worldPos.xz * holeDensity;
				float2 idx = ceil(pos);
				idx = idx + randHole(idx) * (0.5 - holeSize);
				float radius = abs(randHole(idx));
				float holeRadius = radius * holeSize;

				float2 diff = pos - idx + 0.5;
				float len = length(diff);
				len += variation(diff, normalize(IN.worldPos.xz) - 0.5, radius * 6);
				float holeLen = len;

				if (holeLen <= holeRadius && holeRadius > holeMinSize)
				{
					rockColor = _PlatColor;
				}
				//

			}
			else
			{
				rockColor = lerp(_FloorColor, colorful, saturate(IN.worldPos.y * 0.5));
			}

			float glow = smoothstep(0, 1, (centerDis - _Focus.z) / _Focus.w);

			fixed4 c = lerp(rockColor, _FocusColor, glow);

			return c;
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			fixed4 c;

			c = GetRockColor(IN);

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

			//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
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