Shader "Unlit/FusionRadialChartShader"
{
    Properties
    {
        _ColorLow ("Tint Color", Color) = (0,1,0,1)
        _ColorMedium ("Tint Color", Color) = (1,1,0,1)
        _ColorHigh ("Tint Color", Color) = (1,0,0,1)
        [MaterialToggle] _Invert ("Invert Fill", Float) = 0
        _Fill ("Fill Amount", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _ColorLow;
            fixed4 _ColorMedium;
            fixed4 _ColorHigh;
            float _Fill;
            float _Invert;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centeredUV = i.uv - 0.5;

                // Discard the bottom half
                if (centeredUV.y < 0)
                {
                    discard;
                }

                float angle = atan2(centeredUV.y, centeredUV.x);
                float normalizedAngle = angle / UNITY_PI;
                float finalAngle = _Invert ? (1.0 - normalizedAngle) : normalizedAngle;

                // gray not used area
                if (finalAngle > _Fill)
                {
                    return tex2D(_MainTex, i.uv) * fixed4(0.5, 0.5, 0.5, 0.5);
                }

                fixed4 col;
                if (finalAngle <= 0.5)
                {
                    col = _ColorLow;
                }
                else if (finalAngle > 0.5 && finalAngle < 0.75)
                {
                    col = _ColorMedium;
                }
                else
                {
                    col = _ColorHigh;
                }
                col = tex2D(_MainTex, i.uv) * col;
                return col;
            }
            ENDCG
        }
    }
}