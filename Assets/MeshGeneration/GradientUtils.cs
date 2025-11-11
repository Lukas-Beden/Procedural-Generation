using UnityEngine;

public static class GradientUtils
{
    public static Texture2D CreateGradientTexture(Gradient gradient, int width = 256)
    {
        Texture2D tex = new Texture2D(width, 1, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            Color col = gradient.Evaluate(t);
            tex.SetPixel(x, 0, col);
        }

        tex.Apply();
        return tex;
    }
}
