using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace OceanWater.shader_system;

public abstract class ComputeBase<TOutput> : ICompute<TOutput>
{
    private readonly List<RDUniform> _uniforms = new();
    protected readonly System.Collections.Generic.Dictionary<int, Rid> Buffers = new();
    private Rid _shader;
    protected RenderingDevice Rd;

    public ICompute<TOutput> FromFile(string path)
    {
        Rd = RenderingServer.CreateLocalRenderingDevice();
        var shaderFile = GD.Load<RDShaderFile>(path);
        var shaderByteCode = shaderFile.GetSpirV();
        _shader = Rd.ShaderCreateFromSpirV(shaderByteCode);
        return this;
    }

    public ICompute<TOutput> AddBufferUniform(byte[] buffer, int binding)
    {
        var bufferRid = Rd.StorageBufferCreate((uint)buffer.Length, buffer);
        Buffers.Add(binding, bufferRid);

        var uniform = CreateUniform(RenderingDevice.UniformType.StorageBuffer, binding, bufferRid);
        _uniforms.Add(uniform);
        return this;
    }

    public ICompute<TOutput> GeneratePipeline(uint xGroups = 1, uint yGroups = 1, uint zGroups = 1)
    {
        var uniformSet = Rd.UniformSetCreate(new Array<RDUniform>(_uniforms), _shader, 0);
        var pipeline = Rd.ComputePipelineCreate(_shader);
        var computeList = Rd.ComputeListBegin();
        Rd.ComputeListBindComputePipeline(computeList, pipeline);
        Rd.ComputeListBindUniformSet(computeList, uniformSet, 0);
        Rd.ComputeListDispatch(computeList, xGroups, yGroups, zGroups);
        Rd.ComputeListEnd();
        return this;
    }

    public ICompute<TOutput> Run()
    {
        Rd.Submit();
        return this;
    }

    public abstract TOutput ReadBack(int binding);

    private static RDUniform CreateUniform(RenderingDevice.UniformType uniformType, int binding, Rid buffer)
    {
        var uniform = new RDUniform
        {
            UniformType = uniformType,
            Binding = binding
        };
        uniform.AddId(buffer);
        return uniform;
    }
}