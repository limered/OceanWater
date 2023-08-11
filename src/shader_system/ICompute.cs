using Godot;

namespace OceanWater.shader_system;

public interface ICompute<out TOutput>
{
    ICompute<TOutput> FromFile(string path);
    ICompute<TOutput> AddBufferUniform(byte[] buffer, int binding);

    ICompute<TOutput> AddUintUniform(uint[] i, int binding);
    ICompute<TOutput> AddSamplerTextureUniform(
        Texture2D tex,
        RenderingDevice.TextureUsageBits usageBits,
        RenderingDevice.DataFormat format,
        int binding);
    ICompute<TOutput> AddImageUniform(Image image, RenderingDevice.TextureUsageBits usage, RenderingDevice.DataFormat format, int binding);

    ICompute<TOutput> GeneratePipeline(uint xGroups = 1, uint yGroups = 1, uint zGroups = 1);
    ICompute<TOutput> Run();
    TOutput ReadBack(int binding);
}