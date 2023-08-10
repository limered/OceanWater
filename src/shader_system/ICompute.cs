using Godot;

namespace OceanWater.shader_system;

public interface ICompute<TInput, TOutput>
{
    ICompute<TInput,TOutput> InitRenderingDevice();
    ICompute<TInput,TOutput> InitShader(string path);
    ICompute<TInput,TOutput> InitInputs(TInput input);
    ICompute<TInput,TOutput> Run();
    TOutput ReadBack();
}

public abstract class ComputeBase<TInput, TOutput> : ICompute<TInput, TOutput>
{
    protected RenderingDevice Rd;
    protected Rid Shader;
    
    public ICompute<TInput,TOutput> InitRenderingDevice()
    {
        Rd = RenderingServer.CreateLocalRenderingDevice();
        return this;
    }

    public ICompute<TInput,TOutput> InitShader(string path)
    {
        var shaderFile = GD.Load<RDShaderFile>(path);
        var shaderByteCode = shaderFile.GetSpirV();
        Shader = Rd.ShaderCreateFromSpirV(shaderByteCode);
        return this;
    }

    public ICompute<TInput,TOutput> InitInputs()
    {
        throw new System.NotImplementedException();
    }

    public ICompute<TInput,TOutput> Run()
    {
        Rd.Submit();
        return this;
    }

    protected RDUniform CreateUniform(RenderingDevice.UniformType uniformType, int binding, Rid buffer)
    {
        var uniform = new RDUniform
        {
            UniformType = uniformType,
            Binding = binding,
        };
        uniform.AddId(buffer);
        return uniform;
    }
    
    public abstract ICompute<TInput,TOutput> InitInputs(TInput input);
    public abstract TOutput ReadBack();
}