using OceanWater.shader_system;

namespace OceanWater;

public class ExampleComputeShader : ComputeBase<byte[]>
{
    public override byte[] ReadBack(int binding)
    {
        Rd.Sync();
        return Rd.BufferGetData(Buffers[binding]);
    }
}