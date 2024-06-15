#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 splatPosition;
uniform float3 splatColor;
uniform float2 screenSize;
uniform float splatSize;
uniform bool shockwave;

DECLARE_TEXTURE(tex, 0);

// uniform float2 texture;

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float size = splatSize;
    float2 position = splatPosition;
    float2 tuv = uv * screenSize;
    float3 dir = splatColor;

    if(shockwave) {
        dir = float3(normalize(position - tuv) * -splatColor.x, 0);
        size = splatSize;
    }

    float4 splatVector = float4(exp(-pow(distance(tuv, position) / size, 2.0)) * dir, 1);
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