using System;
using Godot;

namespace OceanWater;

public partial class ComputeExample : Node
{
    [Export] public Texture2D Texture { get; set; }

    public override void _Ready()
    {
        var input = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var inputBytes = new byte[input.Length * sizeof(int)];
        Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

        var cs = new ExampleComputeShader()
            .FromFile("res://compute_shader/compute_example.glsl")
            .AddBufferUniform(inputBytes, 0)
            .AddSamplerTextureUniform(
                Texture,
                RenderingDevice.TextureUsageBits.CanUpdateBit | RenderingDevice.TextureUsageBits.SamplingBit,
                RenderingDevice.DataFormat.R8G8B8A8Srgb,
                1)
            .GeneratePipeline(256 / 8, 256 / 8)
            .Run();

        var outputBytes = cs.ReadBack(0);

        var output = new int[input.Length];
        Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);
        GD.Print("Input: ", string.Join(", ", input));
        GD.Print("Output: ", string.Join(", ", output));
    }

    public override void _Process(double delta)
    {
    }
}