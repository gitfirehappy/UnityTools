Shader "Instanced/WaterParticle2D"
{
    Properties
    {
        _NoiseTexArray ("Ripple Noise Array", 2DArray) = "white" {}
        _ColorTexArray ("Water Color Array", 2DArray) = "blue" {}
        _MainColor ("Main Color", Color) = (0.2, 0.5, 1, 0.8)
        _FoamColor ("Foam Color", Color) = (1,1,1,0.9)
        _NoiseScale ("Noise Scale", Range(0.1, 5)) = 1
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.5
        _ParticleSize ("Particle Size", Range(0.01, 0.5)) = 0.1
        _TexIndexOffset ("Texture Index Offset", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<float2> _Positions;
            StructuredBuffer<float2> _Velocities;
            StructuredBuffer<float2> _Densities;

            UNITY_DECLARE_TEX2DARRAY(_NoiseTexArray);
            UNITY_DECLARE_TEX2DARRAY(_ColorTexArray);
            float4 _MainColor;
            float4 _FoamColor;
            float _NoiseScale;
            float _WaveSpeed;
            float _ParticleSize;
            float _TexIndexOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float density : TEXCOORD1;
                float speed : TEXCOORD2;
                float noiseIndex : TEXCOORD3;
                float colorIndex : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                // 获取粒子数据
                float2 position = _Positions[instanceID];
                float2 velocity = _Velocities[instanceID];
                float density = _Densities[instanceID].x;

                // 计算随机纹理索引（基于实例ID和偏移量）
                o.noiseIndex = fmod(instanceID * 1.234 + _TexIndexOffset, 4.0); // 假设最多4种噪声图
                o.colorIndex = fmod(instanceID * 0.789 + _TexIndexOffset, 4.0);  // 假设最多4种颜色图

                // 计算顶点位置
                float3 worldPos = float3(position, 0) + v.vertex.xyz * _ParticleSize;
                o.pos = UnityObjectToClipPos(float4(worldPos, 1.0));

                o.uv = v.uv * _NoiseScale;
                o.density = saturate(density * 2);
                o.speed = length(velocity);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // 采样噪声纹理数组
                float noise = UNITY_SAMPLE_TEX2DARRAY(
                    _NoiseTexArray,
                    float3(i.uv + _Time.y * _WaveSpeed * 0.1, i.noiseIndex)
                ).r;

                // 采样颜色纹理数组
                float4 waterColor = UNITY_SAMPLE_TEX2DARRAY(
                    _ColorTexArray,
                    float3(i.uv * 0.5, i.colorIndex)
                ) * _MainColor;

                // 泡沫效果（基于速度和密度）
                float foam = saturate(i.speed * 0.5 - 0.2) * (1 - i.density);

                // 波纹效果
                float ripple = sin(_Time.y * _WaveSpeed * 5 + noise * 10) * 0.3 + 0.7;

                // 圆形遮罩
                float2 centerVec = (i.uv - 0.5) * 2;
                float alpha = 1 - smoothstep(0.6, 1.0, length(centerVec)); // 更平缓的过渡
                alpha = clamp(alpha, 0.4, 1.0); // 强制透明度最低为0.4，避免完全消失

                    // 密度权重
                float densityAlpha = lerp(0.6, 1.0, saturate(i.density * 1.5));
                alpha *= densityAlpha;

                // 最终混合
                float4 col = waterColor;
                col.rgb = lerp(col.rgb, _FoamColor.rgb, foam * ripple); // 波纹仅影响颜色
                col.a = alpha; // 透明度独立控制
                return col;
            }
            ENDCG
        }
    }
}