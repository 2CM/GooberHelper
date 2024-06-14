#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

DECLARE_TEXTURE(tex, 0);
DECLARE_TEXTURE(velo, 1);

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 texValue = SAMPLE_TEXTURE(tex, uv);
    return texValue;

    // float4 texValue = SAMPLE_TEXTURE(velo, uv);
    // return float4(sin(100.0 * length(texValue.xy)), 0, 0, 1);

    // float4 veloValue = SAMPLE_TEXTURE(velo, uv);

    // veloValue.x = abs(veloValue.x);
    // veloValue.y = abs(veloValue.y);

    // return float4(veloValue.xy, 0, 0);

    // return SAMPLE_TEXTURE(tex, uv) + float4(v, 0, 0);
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