using UnityEngine;

namespace StaticMlp.Game.Presentation {
    public static class RuntimeVisualMaterial {
        private static Shader _opaqueShader;
        private static Shader _transparentShader;

        public static Material Create(Color color, bool transparent = false) {
            var shader = ResolveShader(transparent);
            var material = new Material(shader) {
                color = color
            };

            if (transparent)
                ConfigureTransparency(material);

            return material;
        }

        private static Shader ResolveShader(bool transparent) {
            if (transparent) {
                _transparentShader ??= FindShader(
                    "Universal Render Pipeline/Unlit",
                    "Universal Render Pipeline/Lit",
                    "Sprites/Default",
                    "Standard");
                return _transparentShader;
            }

            _opaqueShader ??= FindShader(
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Unlit",
                "Standard");
            return _opaqueShader;
        }

        private static Shader FindShader(params string[] shaderNames) {
            for (var i = 0; i < shaderNames.Length; i++) {
                var shader = Shader.Find(shaderNames[i]);
                if (shader != null)
                    return shader;
            }

            throw new MissingReferenceException("No compatible runtime shader was found for generated visuals.");
        }

        private static void ConfigureTransparency(Material material) {
            if (material.HasFloat("_Surface"))
                material.SetFloat("_Surface", 1f);
            if (material.HasFloat("_Blend"))
                material.SetFloat("_Blend", 0f);
            if (material.HasFloat("_SrcBlend"))
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (material.HasFloat("_DstBlend"))
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (material.HasFloat("_ZWrite"))
                material.SetFloat("_ZWrite", 0f);

            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
    }
}
