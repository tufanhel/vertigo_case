Shader "Custom/HologramShader"
{
    Properties
    {
        [MainTexture] _MainTex("Main Sprite Texture", 2D) = "white" {}
        _HologramTex("Hologram Scanline Texture", 2D) = "white" {}
        
        [HDR] _HologramColor("Hologram Color", Color) = (0.0, 0.8, 1.0, 1.0)
        
        _ScanlineScrollSpeed("Scanline Scroll Speed", Range(0.0, 5.0)) = 0.5
        _ScanlineTiling("Scanline Tiling", Range(0.1, 20.0)) = 3.0
        _ScanlineIntensity("Scanline Intensity", Range(0.0, 1.0)) = 0.3
        
        _FlickerSpeed("Flicker Speed", Range(0.0, 20.0)) = 5.0
        _FlickerIntensity("Flicker Intensity", Range(0.0, 0.5)) = 0.1
        
        _GlowIntensity("Glow Intensity", Range(0.0, 5.0)) = 1.5
        _Alpha("Overall Alpha", Range(0.0, 1.0)) = 0.85
        
        _DistortionAmount("Distortion Amount", Range(0.0, 0.1)) = 0.005
        _DistortionSpeed("Distortion Speed", Range(0.0, 10.0)) = 2.0
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
                float2 uvHologram : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_HologramTex);
            SAMPLER(sampler_HologramTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _HologramTex_ST;
                half4 _HologramColor;
                float _ScanlineScrollSpeed;
                float _ScanlineTiling;
                float _ScanlineIntensity;
                float _FlickerSpeed;
                float _FlickerIntensity;
                float _GlowIntensity;
                float _Alpha;
                float _DistortionAmount;
                float _DistortionSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                // Hologram texture UV: tile vertically and scroll over time
                float2 hologramUV = IN.uv;
                hologramUV.y = IN.uv.y * _ScanlineTiling + _Time.y * _ScanlineScrollSpeed;
                OUT.uvHologram = hologramUV;
                
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // --- Distortion ---
                float distortion = sin(IN.uv.y * 50.0 + _Time.y * _DistortionSpeed) * _DistortionAmount;
                float2 distortedUV = float2(IN.uv.x + distortion, IN.uv.y);
                
                // --- Main sprite texture ---
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);
                
                // --- Hologram scanline texture (scrolling) ---
                half4 hologramTex = SAMPLE_TEXTURE2D(_HologramTex, sampler_HologramTex, IN.uvHologram);
                
                // --- Scanline effect: darken based on hologram texture ---
                float scanlineEffect = lerp(1.0, hologramTex.r, _ScanlineIntensity);
                
                // --- Flicker effect: subtle random alpha oscillation ---
                float flicker = 1.0 - _FlickerIntensity * abs(sin(_Time.y * _FlickerSpeed) * cos(_Time.y * _FlickerSpeed * 0.7));
                
                // --- Combine ---
                half3 finalColor = mainTex.rgb * _HologramColor.rgb * _GlowIntensity * scanlineEffect;
                float finalAlpha = mainTex.a * _Alpha * flicker * IN.color.a;
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Sprites/Default"
}
