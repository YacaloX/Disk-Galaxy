namespace DiskGalaxy.Rendering.Engine;

public static class ShaderSources
{
    public const string NodeVertex =
        """
        #version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 aInstancePos;
        layout(location = 2) in vec3 aInstanceColor;
        layout(location = 3) in float aInstanceSize;
        layout(location = 4) in float aIsFolder;

        uniform mat4 uViewProj;

        out vec3 vColor;
        out float vIsFolder;

        void main() {
            vec3 worldPos = aInstancePos + aPos * aInstanceSize;
            gl_Position = uViewProj * vec4(worldPos, 1.0);
            vColor = aInstanceColor;
            vIsFolder = aIsFolder;
        }
        """;

    public const string NodeFragment =
        """
        #version 330 core
        in vec3 vColor;
        in float vIsFolder;
        out vec4 fragColor;

        void main() {
            vec2 coord = gl_PointCoord - vec2(0.5);
            float dist = length(coord);
            float alpha = 1.0 - smoothstep(0.0, 0.5, dist);

            if (vIsFolder > 0.5) {
                vec3 glow = vColor * 1.3;
                fragColor = vec4(mix(vColor, glow, alpha), alpha * 0.9);
            } else {
                fragColor = vec4(vColor, alpha * 0.8);
            }
        }
        """;

    public const string EdgeVertex =
        """
        #version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in vec3 aColor;

        uniform mat4 uViewProj;

        out vec3 vColor;

        void main() {
            gl_Position = uViewProj * vec4(aPos, 1.0);
            vColor = aColor;
        }
        """;

    public const string EdgeFragment =
        """
        #version 330 core
        in vec3 vColor;
        out vec4 fragColor;

        void main() {
            fragColor = vec4(vColor, 1.0);
        }
        """;
}
