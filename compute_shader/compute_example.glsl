#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 8, local_size_y = 8, local_size_z = 1) in;

// A binding to the buffer we create in our script
layout(set = 0, binding = 0, std430) restrict buffer MyDataBuffer {
    int data[];
}
my_data_buffer;

layout(set = 0, binding = 1) uniform sampler2D tex;

// The code we want to execute in each invocation
void main() {
    // Get the value from the texture
    vec4 texel = texture(tex, ivec2(gl_GlobalInvocationID.xy));
    atomicAdd(my_data_buffer.data[0], texel.r > 0.0 ? 1 : 0);
    atomicAdd(my_data_buffer.data[1], texel.b > 0.0 ? 1 : 0);
}