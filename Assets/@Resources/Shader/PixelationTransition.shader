Shader "Custom/PixelationTransition"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "PixelationTransition"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_PointClamp);

            float4 _BlitTexture_TexelSize;
            float _PixelSize;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 textureSize = _BlitTexture_TexelSize.zw;
                float2 blockSize = max(_PixelSize.xx, 1.0);
                float2 pixelCoordinate = floor(input.texcoord * textureSize / blockSize);
                float2 quantizedUv = (pixelCoordinate * blockSize + blockSize * 0.5) / textureSize;

                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, quantizedUv);
            }
            ENDHLSL
        }
    }
}