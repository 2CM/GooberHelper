#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float size = 10.0;
    float2 position = float2(0.5,0.5);
    float2 dir = float2(1,1);

    float v = exp(-pow(size * distance(uv, position), 2.0));

    return float4(v * dir, 0, 0);
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