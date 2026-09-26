Shader "LILO/XRayStencilMask"
{
    Properties
    {
        _StencilAlphaTex ("Stencil Alpha Texture", 2D) = "white" {}
        _UseAlphaMask ("Use Alpha Mask", Float) = 0
        _StencilAlphaThreshold ("Alpha Threshold", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+500" "RenderType"="Transparent" }
        Pass
        {
            Name "PlayerSilhouetteMask"
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            ZTest Always
            ZWrite Off
            ColorMask 0
            // URP Deferred reserves the upper stencil bits; keep Eddie's mask in the user bits.
            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_StencilAlphaTex);
            SAMPLER(sampler_StencilAlphaTex);
            CBUFFER_START(UnityPerMaterial)
                float _UseAlphaMask;
                float _StencilAlphaThreshold;
                float4 _StencilAlphaTex_ST;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
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
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv * _StencilAlphaTex_ST.xy + _StencilAlphaTex_ST.zw;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                if (_UseAlphaMask > 0.5)
                {
                    float alpha = SAMPLE_TEXTURE2D(_StencilAlphaTex, sampler_StencilAlphaTex,
                        saturate(input.uv)).a;
                    clip(alpha - _StencilAlphaThreshold);
                }
                return 0;
            }
            ENDHLSL
        }
    }
}
