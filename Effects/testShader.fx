#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

// uniform float Time; // level.TimeActive

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform bool Mode;

DECLARE_TEXTURE(first, 0);
DECLARE_TEXTURE(second, 1);

// texture SelfTexture;
// sampler2D SelfSampler = sampler_state {
//     Texture = (SelfTexture);
//     // MagFilter = Linear;
//     // MinFilter = Linear;
//     // AddressU = Clamp;
//     // AddressV = Clamp;
// };

// texture PlayerTexture;
// sampler2D PlayerSampler = sampler_state {
//     Texture = (PlayerTexture);
//     // MagFilter = Linear;
//     // MinFilter = Linear;
//     // AddressU = Clamp;
//     // AddressV = Clamp;
// };

float4 blur(float2 uv, float size, sampler sam) {
    return (
        tex2D(sam, uv) + 
        tex2D(sam, uv + float2(size, 0)) + 
        tex2D(sam, uv + float2(0, size)) + 
        tex2D(sam, uv - float2(size, 0)) + 
        tex2D(sam, uv - float2(0, size))
    ) * 0.2 * 0.99;
}

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    // float2 worldPos = (uv * Dimensions) + CamPos;
    float4 f = SAMPLE_TEXTURE(first, uv);
    float4 s = SAMPLE_TEXTURE(second, uv);

    // float4 self = tex2D(SelfSampler, uv);
    // float4 player = tex2D(PlayerSampler, uv + float2(0.5,0));

    // return s;
    return blur(uv, 0.003, firstSampler) + float4(s.rgb * (Mode == true ? -0.5 : 2.0), 0);
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