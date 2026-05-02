Shader "Custom/XRaySilhouette"
{
    Properties
    {
        _XRayColor ("Color de Rayos X", Color) = (0, 1, 1, 0.8)
        _Thickness ("Grosor del Borde", Range(0.0, 0.05)) = 0.005
    }
    SubShader
    {
        // Usamos una cola transparente para asegurarnos de que se dibuje DESPUÉS de que
        // todos los objetos opacos (como paredes y mesas) ya estén en la pantalla.
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        LOD 100

        Pass
        {
            // Magia 1: No escribimos en el buffer de profundidad (para no tapar otras cosas)
            ZWrite Off
            
            // Magia 2: SOLO dibujamos este color si hay algo MÁS CERCA de la cámara que este objeto
            ZTest Greater 
            
            // Permitimos transparencia en el color de Rayos X
            Blend SrcAlpha OneMinusSrcAlpha 

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _XRayColor;
            float _Thickness;

            v2f vert (appdata v)
            {
                v2f o;
                // Expandimos la malla ligeramente hacia afuera usando sus normales
                // Esto hace que parezca un "borde" exterior en lugar de una plasta de color
                v.vertex.xyz += v.normal * _Thickness;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _XRayColor;
            }
            ENDCG
        }
    }
}
