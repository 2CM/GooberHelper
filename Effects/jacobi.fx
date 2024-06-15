#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 textureSize;

DECLARE_TEXTURE(pressure, 0);
DECLARE_TEXTURE(divergence, 1);


float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
    float L = tex2D(pressureSampler, uv - float2(1, 0) / textureSize.x).x;   
    float R = tex2D(pressureSampler, uv + float2(1, 0) / textureSize.x).x;   
    float B = tex2D(pressureSampler, uv - float2(0, 1) / textureSize.y).x;   
    float T = tex2D(pressureSampler, uv + float2(0, 1) / textureSize.y).x; 

    // b sample, from center     
    float C = tex2D(divergenceSampler, uv).x; 

    // evaluate Jacobi iteration   
    return float4((L + R + B + T - C) * 0.25, 0, 0, 0);
}

void SpriteVertexShader(inout float4 position : SV_Position,
                        inout float2 texCoord : TEXCOORD0)
{
    position = mul(position, ViewMatrix);
    position = mul(position, TransformMatrix);
}

technique Shader
{
    pass pass0
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 SpritePixelShader();
    }
}