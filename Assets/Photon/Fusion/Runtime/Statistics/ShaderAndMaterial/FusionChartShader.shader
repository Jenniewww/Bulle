Shader "Unlit/FusionChartShader"
{
    Properties
    {
        _FadeInHorizontal ("Fade Horizontal", Range(0.0, 0.4)) = 0.2
        _FadeInHorizontalOffset ("Fade Horizontal Offset", Range(0.0, 0.2)) = 0.1
        _FadeOutBottomOffset ("Fade Out Vertical At The Bottom", Range(0.0, 0.2)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            static const int BUFFER_SAMPLES = 180;
            static const fixed4 CLEAR_COLOR = fixed4(0.0, 0.0, 0.0, 0.0);

            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _ThresholdTopColor;
            fixed4 _ThresholdBottomColor;
            int _ZeroIsTransparent;
            float _FadeInHorizontal;
            float _FadeInHorizontalOffset;
            float _FadeOutBottomOffset;
            float4 _ClipRect;
            uniform float _Values[BUFFER_SAMPLES];
            uniform float _ValueMin;
            uniform float _ValueMax;
            uniform float _Threshold;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };


            v2f vert(appdata v)
            {
                v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float max = 10 * ceil(_ValueMax/10);
                float min = -max * .1f; // keep 0 values visible on the chart.

                // avoid all zero display as full chart
                if (_ValueMin == 0 && _ValueMax == 0)
                {
                    discard;
                }
                
                float currentValue = _Values[floor(i.uv.x * BUFFER_SAMPLES)];
                float nextValue = 0.0;
                if (i.uv.x < 1.0 - (1.0 / BUFFER_SAMPLES))
                {
                    nextValue = _Values[floor(i.uv.x * BUFFER_SAMPLES) + 1];
                } else {
                    nextValue = currentValue;
                }
                
                float controlPoint = (currentValue + nextValue) * 0.5;

                // Normalize the values
                float normalizedValue = (currentValue - min) / (max - min);
                nextValue = (nextValue - min) / (max - min);
                controlPoint = (controlPoint - min) / (max - min);

                float t = frac(i.uv.x * BUFFER_SAMPLES);

                // quadratic Bézier formula
                float curveY = (1.0 - t) * (1.0 - t) * normalizedValue + 2.0 * (1.0 - t) * t * controlPoint + t * t * nextValue;

                fixed4 col;
                if (_Threshold > 0 && currentValue >= _Threshold)
                {
                    col = lerp(_ThresholdBottomColor, _ThresholdTopColor, i.uv.y);
                }
                else
                {
                    col = lerp(_BottomColor, _TopColor, i.uv.y);
                }

                if (_ZeroIsTransparent >= 1 && currentValue <= 0)
                {
                    col = CLEAR_COLOR;
                }
                
                if (i.uv.y > curveY)
                {
                    discard;
                }

                // Apply vertical and horizontal fades.
                if (i.uv.y < _FadeOutBottomOffset)
                {
                    float fadeFactor = i.uv.y / _FadeOutBottomOffset;
                    col.a *= fadeFactor * fadeFactor * fadeFactor;
                }
                
                float fade_start = _FadeInHorizontalOffset;
                float fade_end = 1 - _FadeInHorizontalOffset;
                float fade_length = _FadeInHorizontal;

                if (i.uv.x < fade_start + fade_length)
                {
                    float remapped_x = (i.uv.x - fade_start) / fade_length;
                    float x_in_fade = saturate(remapped_x);
                    col.a *= x_in_fade;
                }
                else if (i.uv.x > fade_end - fade_length)
                {
                    float remapped_x = (i.uv.x - (fade_end - fade_length)) / fade_length;
                    float x_out_fade = saturate(1.0 - remapped_x);
                    col.a *= x_out_fade;
                }

                // handle rect mask 2d
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                return col;
            }
            ENDCG
        }
    }
}