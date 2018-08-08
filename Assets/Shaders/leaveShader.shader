// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/myShader" {

    Properties {
        _Color1 ("First Color", Color) = (1,1,1,1)
        _Color2 ("Second Color", Color) = (1,1,1,1)
        _MainTex ("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
        _RandomValue ("Random Value", Range(0.0, 1.0)) = 0.0
        _Transparency ("Material Transparency", Range(0.0, 1.0)) = 1.0
    }
    
    SubShader {
        Blend SrcAlpha OneMinusSrcAlpha

        Tags { 
               "Queue" = "Transparent"
               "IgnoreProjector" = "True" 
               "RenderType" = "Transparent"
             }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f 
                { 
                    float4  pos : SV_POSITION;
                    float2  uv : TEXCOORD0;
                };

                v2f vert (appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                sampler2D _MainTex;
                float4 _Color1;
                float4 _Color2;
                float _RandomValue;
                float _Transparency;

                fixed4 frag(v2f i) : COLOR
                {
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    fixed4 result = lerp(float4(_Color1.rgb, _Transparency), float4(_Color2.rgb, _Transparency), _RandomValue);   
                    result.a *= step(0.2 ,(tex.r+tex.g+tex.b)/3.0);
                    return result;
                }
                ENDCG
        }
    }
    FallBack "Transparent/Cutout/VertexLit"
}
