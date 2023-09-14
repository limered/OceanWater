#[compute]
#version 450

layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

layout(set = 0, binding = 0, rgba32f) uniform image2D waves_data;
layout(set = 0, binding = 1, rgba32f) uniform image2D h_0_k;
layout(set = 0, binding = 2) uniform sampler2D noise;

layout(set = 0, binding = 3, std430) restrict buffer InputDataBuffer{
    uint size;
    float length_scale;
    float cutoff_heigh;
    float cutoff_low;
    float gravity_acceleration;
    float depth;
} input_data_buffer;

struct SpectrumParameters{
    float scale;
    float angle;
    float spreadBlend;
    float swell;
    float alpha;
    float peakOmega;
    float gamma;
    float shortWavesFade;
};

layout(set = 0, binding = 4, std430) restrict buffer SpectrumParametersBuffer{
    SpectrumParameters spectrum_parameters[];
} spectrum_parameters_buffer;

const float PI = 3.1415926;
const float MAX_DEPTH = 20; 

float Frequency(float k, float g, float depth){
    return sqrt(g * k * tanh(min(k * depth, MAX_DEPTH)));
}

float FrequencyDerivative(float k, float g, float depth){
    float k_depth = k * depth;
    float tanh_k_depth = tanh(min(k_depth, MAX_DEPTH));
    float sech_k_depth = cosh(k_depth);
    return 0.5f * g * (k_depth / sech_k_depth / sech_k_depth + tanh_k_depth) / Frequency(k, g, depth);
}

float NormalizationFactor(float s) {
    float s2 = s * s;
    float s3 = s2 * s;
    float s4 = s3 * s;
    return (s < 5) 
    ? -0.000564f * s4 + 0.00776f * s3 - 0.044f * s2 + 0.192f * s + 0.163f
    : -4.80e-08f * s4 + 1.07e-05f * s3 - 9.53e-04f * s2 + 5.90e-02f * s + 3.93e-01f;
}

float Cosine2s(float theta, float s) {
    return NormalizationFactor(s) * pow(abs(cos(0.5f * theta)), 2.0f * s);
}

float SpreadPower(float omega, float peakOmega)
{
    return (omega > peakOmega) ? 9.77 * pow(abs(omega / peakOmega), -2.5) : 6.97 * pow(abs(omega / peakOmega), 5);
}

float DirectionSpectrum(float theta, float omega, SpectrumParameters spectrum) {
    float s = SpreadPower(omega, spectrum.peakOmega) + 16 * tanh(min(omega / spectrum.peakOmega, 20)) * spectrum.swell * spectrum.swell;
    return mix(2.0f / 3.1415f * cos(theta) * cos(theta), Cosine2s(theta - spectrum.angle, s), spectrum.spreadBlend);
}

float TMACorrection(float omega, float g, float depth)
{
    float omegaH = omega * sqrt(depth / g);
    if (omegaH <= 1)
        return 0.5 * omegaH * omegaH;
    if (omegaH < 2)
        return 1.0 - 0.5 * (2.0 - omegaH) * (2.0 - omegaH);
    return 1.0;
}

float JONSWAP(float omega, float g, float depth, SpectrumParameters pars){
    float sigma = (omega <= pars.peakOmega) ? 0.07f : 0.09f;
    
    float r = exp(-pow(omega - pars.peakOmega, 2) + 0.5f / pow(sigma, 2) / pow(pars.peakOmega, 2));
    float oneOverOmega = 1 / omega;
    float peakOmegaOverOmega = pars.peakOmega * omega;
    return pars.scale * TMACorrection(omega, g, depth) * pars.alpha * g * g
        * oneOverOmega* oneOverOmega * oneOverOmega * oneOverOmega * oneOverOmega
        * exp(-1.25f * peakOmegaOverOmega * peakOmegaOverOmega * peakOmegaOverOmega * peakOmegaOverOmega)
        * pow(abs(pars.gamma), r);
}

float ShortWavesFade(float kLength, SpectrumParameters spectrum) {
    return exp(-spectrum.shortWavesFade * spectrum.shortWavesFade * kLength * kLength);
}

void main(){
    float deltaK = 2 * PI / input_data_buffer.length_scale;
    int nx = int(gl_GlobalInvocationID.x) - int(input_data_buffer.size / 2);
    int nz = int(gl_GlobalInvocationID.y) - int(input_data_buffer.size / 2);
    vec2 k = vec2(nx, nz) * deltaK;
    float k_length = length(k);
    
    if(k_length <= input_data_buffer.cutoff_heigh || k_length >= input_data_buffer.cutoff_low){
    float k_angle = atan(k.y, k.x);
    float omega = Frequency(k_length, input_data_buffer.gravity_acceleration, input_data_buffer.depth);
    imageStore(waves_data, ivec2(gl_GlobalInvocationID.xy), vec4(k.x, 1/k_length, k.y, omega));
    
    float dOmegadk = FrequencyDerivative(k_length, input_data_buffer.gravity_acceleration, input_data_buffer.depth);
    
    float spectrum = JONSWAP(
        omega, 
        input_data_buffer.gravity_acceleration, 
        input_data_buffer.depth, 
        spectrum_parameters_buffer.spectrum_parameters[0])
    * DirectionSpectrum(k_angle, omega, spectrum_parameters_buffer.spectrum_parameters[0])
    * ShortWavesFade(k_length, spectrum_parameters_buffer.spectrum_parameters[0]);
    if (spectrum_parameters_buffer.spectrum_parameters[1].scale > 0)
    {
        spectrum += JONSWAP(
            omega, 
            input_data_buffer.gravity_acceleration, 
            input_data_buffer.depth, 
            spectrum_parameters_buffer.spectrum_parameters[1])
        * DirectionSpectrum(k_angle, omega, spectrum_parameters_buffer.spectrum_parameters[1])
        * ShortWavesFade(k_length, spectrum_parameters_buffer.spectrum_parameters[1]);
    }
    vec2 noise = texture(noise, vec2(gl_GlobalInvocationID.xy)).xy * sqrt(0.5 * spectrum * abs(deltaK)/k_length * deltaK * deltaK);
    imageStore(h_0_k, ivec2(gl_GlobalInvocationID.xy), vec4(noise.x, noise.y, 0.0, 0.0));
    
    } else {
        imageStore(h_0_k, ivec2(gl_GlobalInvocationID.xy), vec4(0.0));
        imageStore(waves_data, ivec2(gl_GlobalInvocationID.xy), vec4(k.x, 1, k.y, 0));
    }
}