//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2018 //
/// Shader generate with Shadero 1.9.6                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "2D/Sprites/SlicedSprite"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _SliceLeftPos("SliceLeftPos", Range(0.001, 0.5)) = 0
        _SliceLeftSize("SliceLeftSize", Range(0.001, 8)) = 1
        _SliceRightPos("SliceRightPos", Range(0.001, 0.5)) = 0
        _SliceRightSize("SliceRightSize", Range(0.001,8)) = 1
        _SliceUpPos("SliceUpPos", Range(0.001, 0.5)) = 0
        _SliceUpSize("SliceUpSize", Range(0.001, 8)) = 1
        _SliceDownPos("SliceDownPos", Range(0.001, 0.5)) = 0
        _SliceDownSize("SliceDownSize", Range(0.001, 8)) = 1
   
        _ColorHSV_Hue_Offset("_ColorHSV_Hue_Offset", Range(-180, 180)) = 180
        _ColorHSV_Saturation_Offset("_ColorHSV_Saturation_Offset", Range(-1, 1)) = 0
        _ColorHSV_Brightness_Offset("_ColorHSV_Brightness_Offset", Range(-1, 1)) = 0
        _SpriteFade("SpriteFade", Range(0, 1)) = 1.0
        
        // required for UI.Mask
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("Color Mask", Float) = 15
    
    }

    SubShader
    {
    
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off 
    
        // required for UI.Mask
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
    
        Pass
        {
        
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            
            struct appdata_t{
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
        
            struct v2f
            {
                float2 texcoord  : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
            };
        
            sampler2D _MainTex;
            float _SpriteFade;
            float _SliceLeftPos;
            float _SliceLeftSize;
            float _SliceRightPos;
            float _SliceRightSize;
            float _SliceUpPos;
            float _SliceUpSize;
            float _SliceDownPos;
            float _SliceDownSize;
            float _ColorHSV_Hue_Offset;
            float _ColorHSV_Saturation_Offset;
            float _ColorHSV_Brightness_Offset;
            #define EPSILON         1.0e-4
            
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }
            
            //
            // Hue, Saturation, Value
            // Ranges:
            //  Hue [0.0, 1.0]
            //  Sat [0.0, 1.0]
            //  Lum [0.0, HALF_MAX]
            //
            float3 RgbToHsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                float d = q.x - min(q.w, q.y);
                float e = EPSILON;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
        
            float3 HsvToRgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
        
            float SlicedHorizontalBarUV(float uv, float2 uvpos, float2 size)
            {
                float ov = uv;
                
                float leftSize = uvpos.x * size.x;
                float rightSize =(uvpos.y) * size.y;
                
                float s1 = lerp(0,leftSize,ov/uvpos.x);
                float s2 = lerp(1 - rightSize,1,(ov-1+uvpos.y)/(uvpos.y));
               
               float l = (ov-uvpos.x) / (1. - (uvpos.x + uvpos.y));
               float s3 =  lerp(leftSize, 1-rightSize, l);
               
                //edge=step(edge,_Edge); //if(edge<=_Edge) edge=1 , else edge=0
                ov =  step(ov,uvpos.x)*s1 + step(uvpos.x,ov)*((s3 * step(ov,1 - uvpos.y) + s2 * step(1 - uvpos.y,ov)));
                return ov;
            }
            
            float2 SlicedVerticalBarUV(float2 uv, float b1, float s)
            {
                float ov = uv.y;
                float muv =uv.y;
                muv *= s;
                float s1 = muv;
                float s2 = 1+muv-s;
                float z = b1 / s;
                muv = lerp(b1, 1. - b1, ov);
                muv -= z;
                uv.y = muv / (1. - (z * 2.));
                if (ov < z) { uv.y = s1; }
                if (ov > 1. - z) { uv.y = s2; }
                return uv;
            }
            
            float4 frag (v2f i) : COLOR
            {
                float2 _SliceUV = i.texcoord;
                _SliceUV.x = SlicedHorizontalBarUV(i.texcoord.x,float2(_SliceLeftPos,_SliceRightPos),float2(_SliceLeftSize,_SliceRightSize));
                _SliceUV.y = SlicedHorizontalBarUV(i.texcoord.y,float2(_SliceDownPos,_SliceUpPos),float2(_SliceDownSize,_SliceUpSize));
                float4 _MainTex_1 = tex2D(_MainTex,_SliceUV);
                
                float3 hsv = RgbToHsv(_MainTex_1.rgb);
                
                hsv += float3((_ColorHSV_Hue_Offset)/360.0,_ColorHSV_Saturation_Offset,_ColorHSV_Brightness_Offset);
                
                _MainTex_1.rgb = HsvToRgb(hsv);
                
                float4 FinalResult = _MainTex_1;
                FinalResult.rgb *= i.color.rgb;
                FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
                return FinalResult;
            }
        
        ENDCG
        }
     }
    Fallback "Sprites/Default"
}
