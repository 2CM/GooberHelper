// #define DECLARE_TEXTURE(Name, index) \
//     texture Name: register(t##index); \
//     sampler Name##Sampler: register(s##index)

// #define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

// uniform float Time; // level.TimeActive

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

// DECLARE_TEXTURE(text, 0);
// DECLARE_TEXTURE(other, 1);

// texture FreakyTexture;
// sampler2D TextureSampler = sampler_state {
//     Texture = (FreakyTexture);
//     MagFilter = Linear;
//     MinFilter = Linear;
//     AddressU = Clamp;
//     AddressV = Clamp;
// };

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    // float2 worldPos = (uv * Dimensions) + CamPos;
    // float4 color = SAMPLE_TEXTURE(text, uv);
    // float4 other = SAMPLE_TEXTURE(other, uv);

    return float4(uv,0,1);
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