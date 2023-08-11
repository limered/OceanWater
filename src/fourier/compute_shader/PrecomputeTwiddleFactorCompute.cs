using Godot;
using OceanWater.shader_system;

namespace OceanWater.fourier.compute_shader;

public class PrecomputeTwiddleFactorCompute : ComputeBase<Image>
{
    private readonly int _width;
    private readonly int _height;

    public PrecomputeTwiddleFactorCompute(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public override Image ReadBack(int binding)
    {
        Rd.Sync();
        var data = Rd.TextureGetData(Buffers[binding], 0);
        return Image.CreateFromData(
            _width,
            _height,
            false,
            Image.Format.Rgbaf,
            data);
    }
}