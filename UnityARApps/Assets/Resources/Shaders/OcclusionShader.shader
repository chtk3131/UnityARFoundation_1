// shader名の設定
Shader "AR/Occlusion"
{
    SubShader {
        Tags { "RenderType" = "Opaque" }

        // 他の通常の仮想物体（Queue=Geometry）より先にレンダリングするように設定
        Tags { "Queue" = "Geometry-1" }
        
        // 深度バッファへの書き込み許可
        ZWrite On

        // 深度の判定条件
        ZTest LEqual

        // 色の成分のマスク設定
        ColorMask 0

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma "UnityCG.cginc"

            struct appdata {
                float4 vertex:POSITION;
            };

            struct v2f {
                float4 vertex:SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET {
                return fixed4(0.0,0.0,0.0,0.0);
            }
            ENDCG
        }
    }

}