// #define DECLARE_TEXTURE(Name, index) \
//     texture Name##Texture: register(t##index); \
//     sampler Name##Sampler: register(s##index)

// #define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

// uniform float Time; // level.TimeActive

uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;

// DECLARE_TEXTURE(text, 0);
// DECLARE_TEXTURE(Freaky, 0);

texture FirstTexture;
sampler2D FirstSampler = sampler_state {
    Texture = (FirstTexture);
    // MagFilter = Linear;
    // MinFilter = Linear;
    // AddressU = Clamp;
    // AddressV = Clamp;
};

texture SecondTexture;
sampler2D SecondSampler = sampler_state {
    Texture = (SecondTexture);
    // MagFilter = Linear;
    // MinFilter = Linear;
    // AddressU = Clamp;
    // AddressV = Clamp;
};

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
    // float2 worldPos = (uv * Dimensions) + CamPos;
    // float4 color = SAMPLE_TEXTURE(text, uv);
    float4 first = tex2D(FirstSampler, uv);
    float4 second = tex2D(SecondSampler, uv);

    return uv.x < 0.5 ? first : second;
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