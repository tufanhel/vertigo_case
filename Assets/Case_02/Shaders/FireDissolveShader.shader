Shader "Custom/FireDissolveShader"
{
    Properties
    {
        [MainTexture] _MainTex("Main Sprite Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture (Dissolve Pattern)", 2D) = "white" {}
        
        _DissolveAmount("Dissolve Amount", Range(0.0, 1.0)) = 0.0
        _DissolveDirection("Dissolve Direction (0=Top-Down, 1=Bottom-Up)", Range(0.0, 1.0)) = 0.0
        
        _EdgeWidth("Fire Edge Width", Range(0.0, 0.3)) = 0.08
        [HDR] _EdgeColor1("Edge Color Inner (Yellow/White)", Color) = (4.0, 3.0, 1.0, 1.0)
        [HDR] _EdgeColor2("Edge Color Outer (Orange/Red)", Color) = (3.0, 0.8, 0.0, 1.0)
        [HDR] _EdgeColor3("Edge Color Far (Dark Red)", Color) = (1.5, 0.1, 0.0, 1.0)
        
        _EmissionIntensity("Emission Glow Intensity", Range(0.0, 10.0)) = 3.0
        _NoiseScale("Noise UV Scale", Range(0.1, 10.0)) = 1.0
        _VerticalBias("Vertical Dissolve Bias", Range(0.0, 1.0)) = 0.3
        
        _FlickerSpeed("Fire Flicker Speed", Range(0.0, 20.0)) = 8.0
        _FlickerIntensity("Fire Flicker Intensity", Range(0.0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uvNoise : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float _DissolveAmount;
                float _DissolveDirection;
                float _EdgeWidth;
                half4 _EdgeColor1;
                half4 _EdgeColor2;
                half4 _EdgeColor3;
                float _EmissionIntensity;
                float _NoiseScale;
                float _VerticalBias;
                float _FlickerSpeed;
                float _FlickerIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.uvNoise = IN.uv * _NoiseScale;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Sample textures ---
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half noiseVal = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uvNoise).r;
                
                // --- Directional dissolve: combine noise with vertical gradient ---
                // Top-down: UV.y = 1 at top, 0 at bottom
                // We want top to dissolve first, so use (1 - UV.y)
                float verticalGrad = lerp(1.0 - IN.uv.y, IN.uv.y, _DissolveDirection);
                
                // Blend noise with vertical gradient for directional dissolve
                float dissolveValue = lerp(noiseVal, verticalGrad, _VerticalBias);
                
                // --- Fire flicker ---
                float flicker = sin(_Time.y * _FlickerSpeed) * cos(_Time.y * _FlickerSpeed * 0.7) * _FlickerIntensity;
                
                // --- Dissolve threshold ---
                float adjustedDissolve = _DissolveAmount + flicker;
                adjustedDissolve = saturate(adjustedDissolve);
                
                // --- Alpha clip based on dissolve ---
                float diff = dissolveValue - adjustedDissolve;
                
                // If below threshold, pixel is dissolved (invisible)
                if (diff < 0.0)
                {
                    discard;
                }
                
                // --- Fire edge coloring ---
                float edgeDistance = diff / _EdgeWidth;
                edgeDistance = saturate(edgeDistance);
                
                // Three-zone color gradient for fire edge
                half3 fireColor;
                if (edgeDistance < 0.33)
                {
                    // Closest to dissolve edge: bright white/yellow
                    float t = edgeDistance / 0.33;
                    fireColor = lerp(_EdgeColor1.rgb, _EdgeColor2.rgb, t);
                }
                else if (edgeDistance < 0.66)
                {
                    // Middle zone: orange
                    float t = (edgeDistance - 0.33) / 0.33;
                    fireColor = lerp(_EdgeColor2.rgb, _EdgeColor3.rgb, t);
                }
                else
                {
                    // Far from edge: original texture
                    float t = (edgeDistance - 0.66) / 0.34;
                    fireColor = lerp(_EdgeColor3.rgb, mainTex.rgb, t);
                }
                
                // --- Emission intensity on edge ---
                float edgeMask = 1.0 - edgeDistance;
                float emission = edgeMask * _EmissionIntensity;
                
                // --- Final color ---
                half3 finalColor = fireColor + fireColor * emission;
                
                // Keep original alpha for non-edge areas, boost for edge glow
                float finalAlpha = mainTex.a * IN.color.a;
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
