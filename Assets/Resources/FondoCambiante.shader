Shader "Unlit/FondoCambiante"
{
   Properties
    {
        _Color1 ("Color 1", Color) = (0.129,0.102,0.569,1)    // #211951
        _Color2 ("Color 2", Color) = (0.525,0.438,1,1)       // #836FFF
        _Color3 ("Color 3", Color) = (0.082,0.961,0.729,1)   // #15F5BA
        _Color4 ("Color 4", Color) = (0.941,0.953,1,1)       // #F0F3FF
        _Speed  ("Velocidad (ciclos por segundo)", Float) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Transparent" }
        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            float4 _Color1, _Color2, _Color3, _Color4;
            float  _Speed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // t oscila suave de 0 a 1 y de vuelta sin saltos
                float t = 0.5 * (sin(_Time.y * _Speed * UNITY_PI * 2.0) + 1.0);

                // mezclamos dos parejas de colores
                fixed4 topColor    = lerp(_Color1, _Color2, t);
                fixed4 bottomColor = lerp(_Color3, _Color4, t);

                // aquí aplicamos un gradiente vertical
                float y = saturate(i.uv.y);
                return lerp(bottomColor, topColor, y);
            }
            ENDCG
        }
    }
}
