Shader "LILO/XRayStencilMask"
{
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

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
