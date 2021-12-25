using UnityEngine;

using System;
using System.IO;
using System.Linq;
public static class utils
{
    static System.Random rnd = new System.Random(DateTime.Now.Millisecond);
    public static float infinity = Mathf.Infinity;

    public static float pi = Mathf.PI;

    public static float sqrt(float v)
    {
        return (float)Math.Sqrt(v);
    }
    public static float degrees_to_radians(float degrees)
    {
        return degrees * pi / 180;
    }

    public static float rand()
    {
        return (float)rnd.NextDouble();
    }
    public static float rand(float min, float max)
    {
        return (float)rnd.NextDouble() * (max - min) + min;
    }

    public static float clamp(float x, float min, float max)
    {
        if (x < min) return min;
        if (x > max) return max;
        return x;
    }

    public static  void CaptureImage(RenderTexture renderTexture)
    {
        RenderTexture.active = renderTexture;

        int width = renderTexture.width;
        int height = renderTexture.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        byte[] byteArr = tex.EncodeToPNG();

        string filePath = Application.dataPath + "/captured/";
        DirectoryInfo di = new DirectoryInfo(filePath);
        if (!di.Exists) di.Create();

        string[] filters = { "captured*" };
        int fileNumber = filters.SelectMany(f=> Directory.GetFiles(filePath, f)).Where(name => name.EndsWith(".png")).Count();
        File.WriteAllBytes(filePath + "/captured" + fileNumber + ".png", byteArr);
    }
}
