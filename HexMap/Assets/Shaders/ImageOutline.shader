Shader "Hidden/Custom/ImageOutline"
{

    HLSLINCLUDE

    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"


    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    float4 _MainTex_TexelSize;
    
    sampler2D _CameraDepthNormalsTexture;
    float _Threshold;
    half4 _EdgeColor;

    float DecodeFloatRG(float2 enc)
    {
        float2 kDecodeDot = float2(1.0, 1 / 255.0);
        return dot(enc, kDecodeDot);
    }

    void DecodeDepthNormal(float4 enc, out float depth, out float3 normal)
    {
        depth = DecodeFloatRG(enc.zw);
        normal = DecodeViewNormalStereo(enc);
    }

    float4 GetPixelValue(in float2 uv) {
        half3 normal;
        float depth;
        DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv), depth, normal);
        return float4(normal, depth * 1000);
    }

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
        float2 center = i.texcoord.xy - float2(0.5, 0.5);
        float dis = length(center);
        if (dis < 0.5)
        {
            float4 orValue = GetPixelValue(i.texcoord);
            float thickness = 1;
            float2 offsets[8] = {
                        float2(-thickness, -thickness),
                        float2(-thickness, 0),
                        float2(-thickness, thickness),
                        float2(0, -thickness),
                        float2(0, thickness),
                        float2(thickness, -thickness),
                        float2(thickness, 0),
                        float2(thickness, thickness)
            };
            float4 sampledValue = float4(0, 0, 0, 0);

            for (int j = 0; j < 8; j++) {
                sampledValue += GetPixelValue(i.texcoord + offsets[j] * _MainTex_TexelSize.xy);
            }
            sampledValue /= 8;
            return lerp(color, _EdgeColor, step(_Threshold, length(orValue - sampledValue)));
        }
        return color;
    }

        ENDHLSL

        SubShader
    {
        Cull Off ZWrite Off ZTest Always

            Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment Frag
            ENDHLSL
        }
    }
}