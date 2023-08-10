using System;
using Godot;

namespace OceanWater;

public partial class ComputeExample : Node
{
    public override void _Ready()
    {
        var input = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var inputBytes = new byte[input.Length * sizeof(float)];
        Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

        var cs = new ExampleComputeShader()
            .FromFile("res://compute_shader/compute_example.glsl")
            .AddBufferUniform(inputBytes, 0)
            .GeneratePipeline()
            .Run();

        var outputBytes = cs.ReadBack(0);

        var output = new float[input.Length];
        Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);
        GD.Print("Input: ", string.Join(", ", input));
        GD.Print("Output: ", string.Join(", ", output));
    }

    public override void _Process(double delta)
    {
    }
}