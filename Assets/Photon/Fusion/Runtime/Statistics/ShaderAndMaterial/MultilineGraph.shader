Shader "Fusion/UI/MultiLineGraph"
{
    Properties
    {
        _DataTex ("Data Texture", 2D) = "black" {}
        _BackgroundColor ("Background Color", Color) = (0.1, 0.1, 0.1, 0.9)
        _LineWidth ("Line Width", Range(0.01, 0.05)) = 0.01
        _ThresholdWidth ("Threshold Width", Range(0.01, 0.05)) = 0.01
        _DashLength ("Dash Length", Range(0.01, 0.1)) = 0.03
        _GapLength ("Gap Length", Range(0.01, 0.1)) = 0.02
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
            
            sampler2D _DataTex;
            float4 _BackgroundColor;
            float _LineWidth;
            float _ThresholdWidth;
            float _DashLength;
            float _GapLength;
            float4 _ClipRect;
            
            uint _MaxSamples;
            uint _LineCount;
            uint _ThresholdCount;
            float _MinValue;
            float _MaxValue;
            
            #define MAX_LINES 4
            #define MAX_THRESHOLDS 4
            
            float4 _LineColors[MAX_LINES];
            float _Thresholds[MAX_THRESHOLDS];
            float4 _ThresholdColors[MAX_THRESHOLDS];
            
            float _WriteIndices[MAX_LINES];
            float _SampleCounts[MAX_LINES];
            float _LineVisible[MAX_LINES];
            float _ThresholdVisible[MAX_THRESHOLDS];

            float valueToY(float value)
            {
                return saturate((value - _MinValue) / (_MaxValue - _MinValue));
            }
            
            float sampleLineData(int logicalIndex, int lineIdx, int count, int writeIdx)
            {
                // Branchless start index: full ? writeIdx : 0
                float fullMask = step((float)_MaxSamples, (float)count + 0.5);
                int startIdx = (int)(fullMask * writeIdx);
                int actualIdx = (startIdx + logicalIndex) % _MaxSamples;
                
                float texU = (actualIdx + 0.5) / (float)_MaxSamples;
                float texV = (lineIdx + 0.5) / (float)MAX_LINES;
                
                return tex2Dlod(_DataTex, float4(texU, texV, 0, 0)).r;
            }
            
            // Returns squared distance to avoid sqrt until needed
            float distToSegmentSq(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float len2 = dot(ba, ba);
                float t = saturate(dot(pa, ba) / max(len2, 0.0001));
                float2 diff = pa - ba * t;
                return dot(diff, diff);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 col = _BackgroundColor;
                
                // Precompute values shared across iterations of thresholds
                float dashPhase = fmod(uv.x, _DashLength + _GapLength);
                float inDash = step(dashPhase, _DashLength);
                
                [unroll]
                for (int t = 0; t < MAX_THRESHOLDS; t++)
                {
                    float active = step((float)t + 0.5, (float)_ThresholdCount);
                    
                    float threshY = valueToY(_Thresholds[t]);
                    float dist = abs(uv.y - threshY);
                    
                    float alpha = (1.0 - smoothstep(0, _ThresholdWidth, dist)) 
                                * inDash 
                                * active 
                                * _ThresholdVisible[t]
                                * _ThresholdColors[t].a;
                    
                    col = lerp(col, _ThresholdColors[t], alpha);
                }
                
                
                [unroll]
                for (int lineIdx = 0; lineIdx < MAX_LINES; lineIdx++)
                {
                    int count = (int)_SampleCounts[lineIdx];
                    int writeIdx = (int)_WriteIndices[lineIdx];
                    
                    // Active if line exists and has >= 2 samples
                    float active = step((float)lineIdx + 0.5, (float)_LineCount) 
                                 * step(2.0, (float)count)
                                 * _LineVisible[lineIdx];
                    
                    // Safe values for math (avoid div by zero when inactive)
                    float safeCount = max((float)count, 2.0);
                    float stepSize = 1.0 / (safeCount - 1.0);
                    
                    // Find segment index
                    float sampleIndexF = uv.x * (safeCount - 1.0);
                    int sampleIndex = clamp((int)floor(sampleIndexF), 0, max(count - 2, 0));
                    
                    // Current segment
                    float x1 = sampleIndex * stepSize;
                    float x2 = x1 + stepSize;
                    float y1 = valueToY(sampleLineData(sampleIndex, lineIdx, count, writeIdx));
                    float y2 = valueToY(sampleLineData(sampleIndex + 1, lineIdx, count, writeIdx));
                    
                    float minDistSq = distToSegmentSq(uv, float2(x1, y1), float2(x2, y2));
                    
                    // Previous segment 
                    float hasPrev = step(1.0, (float)sampleIndex);
                    float x0 = (sampleIndex - 1) * stepSize;
                    float y0 = valueToY(sampleLineData(max(0, sampleIndex - 1), lineIdx, count, writeIdx));
                    float distPrevSq = distToSegmentSq(uv, float2(x0, y0), float2(x1, y1));
                    minDistSq = lerp(minDistSq, min(minDistSq, distPrevSq), hasPrev);
                    
                    // Next segment
                    float hasNext = step((float)sampleIndex + 2.5, safeCount);
                    float x3 = (sampleIndex + 2) * stepSize;
                    float y3 = valueToY(sampleLineData(min(sampleIndex + 2, max(count - 1, 0)), lineIdx, count, writeIdx));
                    float distNextSq = distToSegmentSq(uv, float2(x2, y2), float2(x3, y3));
                    minDistSq = lerp(minDistSq, min(minDistSq, distNextSq), hasNext);
                    
                    // Single sqrt at the end
                    float dist = sqrt(minDistSq);
                    float alpha = (1.0 - smoothstep(_LineWidth * 0.5, _LineWidth, dist)) 
                                * active 
                                * _LineColors[lineIdx].a;
                    
                    col = lerp(col, _LineColors[lineIdx], alpha);
                }

                // handle rect mask 2d
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                
                return col;
            }
            ENDCG
        }
    }
}
