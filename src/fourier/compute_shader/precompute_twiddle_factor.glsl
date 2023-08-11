#[compute]
#version 450

layout(local_size_x = 1, local_size_y = 8, local_size_z = 1) in;

layout(set = 0, binding = 0, rgba32f) uniform image2D precompute_buffer;
layout(set = 0, binding = 1, std430) restrict buffer MyDataBuffer{
    uint data;
} my_data_buffer;

const float PI = 3.1415926;

vec2 ComplexExp(vec2 a){
    return vec2(cos(a.y), sin(a.y)) * exp(a.x);
}

void main(){
    ivec2 id = ivec2(gl_GlobalInvocationID.xy);
    uint size = my_data_buffer.data;
    
    uint b = size >> (id.x + 1);
    vec2 mult = 2 * PI * vec2(0, 1) / size;
    uint i = (2 * b * (id.y / b) + id.y % b) % size;
    vec2 twiddle = ComplexExp(-mult * ((id.y / b) * b));
    imageStore(
        precompute_buffer, 
        ivec2(id.xy), 
        vec4(twiddle.x, twiddle.y, i, i + b));
    imageStore(
        precompute_buffer, 
        ivec2(id.x, id.y + size / 2), 
        vec4(-twiddle.x, -twiddle.y, i, i + b));
}