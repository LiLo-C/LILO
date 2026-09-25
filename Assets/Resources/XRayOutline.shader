Shader "LILO/XRayOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,0.86,0.48,0.95)
        _OutlineWidth ("Outline Width", Range(0.001,0.03)) = 0.006
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
            Cull Front
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
                float _XRayFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float4 clipPosition = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
                clipPosition.xy += normalize(normalVS.xy + float2(0.00001, 0.00001)) * _OutlineWidth * clipPosition.w;
                output.positionCS = clipPosition;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, _OutlineColor.a * saturate(_XRayFade));
            }
            ENDHLSL
        }
    }
}
