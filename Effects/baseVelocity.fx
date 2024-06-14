#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 splatPosition;
uniform float2 splatVelocity;
uniform float2 screenSize;
uniform float splatSize;

DECLARE_TEXTURE(tex, 0);

// uniform float2 texture;

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float size = splatSize;
    float2 position = splatPosition;
    float2 dir = splatVelocity;
    float2 tuv = uv * screenSize;

    float4 splatVector = float4(exp(-pow(size * distance(tuv, position), 2.0)) * dir, 0, 1);
    float4 oldVector = SAMPLE_TEXTURE(tex, uv);

    return oldVector + splatVector;
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