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
        layout(location = 5) in vec2 aUV;

        uniform mat4 uViewProj;
        uniform vec3 uCameraPos;

        out vec3 vColor;
        out float vIsFolder;
        out vec2 vUV;
        out float vAlpha;

        void main() {
            vec3 worldPos = aInstancePos + aPos * aInstanceSize;
            gl_Position = uViewProj * vec4(worldPos, 1.0);

            float dist = distance(worldPos, uCameraPos);
            float lodFade;
            if (aIsFolder > 0.5) {
                lodFade = 1.0 - smoothstep(80.0, 500.0, dist);
            } else {
                lodFade = 1.0 - smoothstep(25.0, 180.0, dist);
            }

            vColor = aInstanceColor;
            vIsFolder = aIsFolder;
            vUV = aUV;
            vAlpha = lodFade;
        }
        """;

    public const string NodeFragment =
        """
        #version 330 core
        in vec3 vColor;
        in float vIsFolder;
        in vec2 vUV;
        in float vAlpha;
        out vec4 fragColor;

        void main() {
            vec2 coord = vUV - vec2(0.5);
            float dist = length(coord);
            float alpha = 1.0 - smoothstep(0.0, 0.5, dist);

            if (alpha < 0.01 || vAlpha < 0.01) discard;

            if (vIsFolder > 0.5) {
                vec3 glow = vColor * 1.3;
                fragColor = vec4(mix(vColor, glow, alpha), alpha * 0.9 * vAlpha);
            } else {
                fragColor = vec4(vColor, alpha * 0.8 * vAlpha);
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

    public const string ClusterVertex =
        """
        #version 330 core
        layout(location = 0) in vec3 aRelPos;
        layout(location = 1) in vec3 aCenter;
        layout(location = 2) in vec3 aColor;
        layout(location = 3) in float aRadius;
        layout(location = 4) in float aAlpha;

        uniform mat4 uViewProj;

        out vec3 vColor;
        out float vAlpha;

        void main() {
            vec3 worldPos = aCenter + aRelPos * aRadius;
            vec4 clipPos = uViewProj * vec4(worldPos, 1.0);
            gl_Position = clipPos;

            float dist = abs(clipPos.z);
            gl_PointSize = max(2.0, 28.0 / (1.0 + dist * 0.02));

            vColor = aColor;
            vAlpha = aAlpha;
        }
        """;

    public const string ClusterFragment =
        """
        #version 330 core
        in vec3 vColor;
        in float vAlpha;
        out vec4 fragColor;

        void main() {
            vec2 coord = gl_PointCoord - vec2(0.5);
            float dist = length(coord);
            float alpha = 1.0 - smoothstep(0.0, 0.5, dist);
            if (alpha < 0.01) discard;
            fragColor = vec4(vColor * (1.0 + alpha * 0.4), alpha * vAlpha * 0.55);
        }
        """;

    public const string SkyboxVertex =
        """
        #version 330 core
        layout(location = 0) in vec3 aPos;
        layout(location = 1) in float aBrightness;

        uniform mat4 uViewProj;

        out float vBrightness;

        void main() {
            gl_Position = uViewProj * vec4(aPos, 1.0);
            gl_PointSize = 1.5;
            vBrightness = aBrightness;
        }
        """;

    public const string SkyboxFragment =
        """
        #version 330 core
        in float vBrightness;
        out vec4 fragColor;

        void main() {
            vec2 coord = gl_PointCoord - vec2(0.5);
            float dist = length(coord);
            float alpha = 1.0 - smoothstep(0.0, 0.5, dist);
            if (alpha < 0.01) discard;
            fragColor = vec4(vec3(0.8, 0.85, 1.0) * vBrightness, alpha * 0.7);
        }
        """;
}
