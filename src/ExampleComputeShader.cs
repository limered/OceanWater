using Godot;
using Godot.Collections;
using OceanWater.shader_system;

namespace OceanWater;

public class ExampleComputeShader : ComputeBase<byte[],byte[]>
{
    private Rid _buffer;
    
    public override ICompute<byte[], byte[]> InitInputs(byte[] input)
    {
        _buffer = Rd.StorageBufferCreate((uint)input.Length, input);
        
        var uniform = CreateUniform(RenderingDevice.UniformType.StorageBuffer, 0, _buffer);
        
        var uniformSet = Rd.UniformSetCreate(new Array<RDUniform> { uniform }, Shader, 0);
        var pipeline = Rd.ComputePipelineCreate(Shader);
        var computeList = Rd.ComputeListBegin();
        Rd.ComputeListBindComputePipeline(computeList, pipeline);
        Rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
        Rd.ComputeListDispatch(computeList, xGroups: 5, yGroups: 1, zGroups: 1);
        Rd.ComputeListEnd();

        return this;
    }

    public override byte[] ReadBack()
    {
        Rd.Sync();
        return Rd.BufferGetData(_buffer);
    }
}