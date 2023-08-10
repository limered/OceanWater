using Godot;

namespace OceanWater.shader_system;

public interface ICompute<out TOutput>
{
    ICompute<TOutput> FromFile(string path);
    ICompute<TOutput> AddBufferUniform(byte[] buffer, int binding);

    ICompute<TOutput> AddImageUniform(
        Texture2D tex,
        RenderingDevice.TextureUsageBits usageBits,
        RenderingDevice.DataFormat format,
        int binding);

    ICompute<TOutput> GeneratePipeline(uint xGroups = 1, uint yGroups = 1, uint zGroups = 1);
    ICompute<TOutput> Run();
    TOutput ReadBack(int binding);
}