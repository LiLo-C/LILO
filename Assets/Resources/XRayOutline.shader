Shader "LILO/XRayOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0.86,0.48,0.95)
        _OutlineWidth ("Outline Width", Range(0.001,0.03)) = 0.006
        _RadialExpansion ("Radial Expansion", Float) = 0
        _OutlineCull ("Outline Cull Mode", Float) = 1
        _OutlineAlphaTex ("Outline Alpha Texture", 2D) = "white" {}
        _UseAlphaOutline ("Use Alpha Silhouette", Float) = 0
        _AlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.5
        _AlphaOutlineRadius ("Alpha Outline Radius", Float) = 2
        _DepthTest ("Depth Test", Float) = 5
        _XRayFade ("X-ray Fade", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+501" "RenderType"="Transparent" }
        Pass
        {
            Name "XRayOutline"
            Tags { "LightMode"="UniversalForward" }
            Cull [_OutlineCull]
            ZTest [_DepthTest]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            // Bit 1 is the combined body silhouette; bit 2 prevents mesh overlap from stacking.
            Stencil
            {
                Ref 0
                ReadMask 3
                WriteMask 2
                Comp Equal
                Pass Invert
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                float _OutlineWidth;
                float _RadialExpansion;
                float _UseAlphaOutline;
                float _AlphaThreshold;
                float _AlphaOutlineRadius;
                float4 _OutlineAlphaTex_ST;
                float4 _OutlineAlphaTex_TexelSize;
                float _XRayFade;
            CBUFFER_END

            TEXTURE2D(_OutlineAlphaTex);
            SAMPLER(sampler_OutlineAlphaTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 clipPosition = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
                float2 expansionDirection = normalVS.xy;
                if (_RadialExpansion > 0.5)
                {
                    float4 centerClip = TransformObjectToHClip(float3(0.0, 0.0, 0.0));
                    expansionDirection = clipPosition.xy / clipPosition.w - centerClip.xy / centerClip.w;
                }
                clipPosition.xy += normalize(expansionDirection + float2(0.00001, 0.00001))
                    * _OutlineWidth * clipPosition.w;
                output.positionCS = clipPosition;
                output.uv = input.uv * _OutlineAlphaTex_ST.xy + _OutlineAlphaTex_ST.zw;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                if (_UseAlphaOutline > 0.5)
                {
                    float2 uv = input.uv;
                    float insideTexture = step(0.0, uv.x) * step(0.0, uv.y)
                        * step(uv.x, 1.0) * step(uv.y, 1.0);
                    float centerAlpha = SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex,
                        saturate(uv)).a * insideTexture;
                    clip(_AlphaThreshold - centerAlpha - 0.001);

                    float2 d = _OutlineAlphaTex_TexelSize.xy * _AlphaOutlineRadius;
                    float neighborAlpha = 0.0;
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + float2(d.x, 0.0))).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + float2(-d.x, 0.0))).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + float2(0.0, d.y))).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + float2(0.0, -d.y))).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + d)).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv - d)).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + float2(d.x, -d.y))).a);
                    neighborAlpha = max(neighborAlpha, SAMPLE_TEXTURE2D(_OutlineAlphaTex, sampler_OutlineAlphaTex, saturate(uv + float2(-d.x, d.y))).a);
                    clip(neighborAlpha - _AlphaThreshold);
                }
                return half4(_OutlineColor.rgb, _OutlineColor.a * saturate(_XRayFade));
            }
            ENDHLSL
        }
    }
}
