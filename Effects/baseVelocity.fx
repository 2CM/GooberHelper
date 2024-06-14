#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 splatPosition;
uniform float2 splatDirection;

DECLARE_TEXTURE(tex, 0);

// uniform float2 texture;

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float size = 0.1;
    float2 position = splatPosition;
    float2 dir = splatDirection / 60.0;
    float2 tuv = uv * float2(320.0, 180.0);

    float4 splatVector = float4(exp(-pow(size * distance(tuv, position), 2.0)) * dir, 0, 1);
    float4 oldVector = SAMPLE_TEXTURE(tex, uv);

    return oldVector + splatVector * 0.01;
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