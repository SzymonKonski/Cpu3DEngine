using System.Drawing;
using System.Numerics;

namespace Cpu3DEngine;

public class FogGenerator
{
    public Color FogColor;
    public float MaxDistance;

    public FogGenerator(Color fogColor, float maxDistance)
    {
        FogColor = fogColor;
        MaxDistance = maxDistance;
    }

    public Color_0_1 ApplyFogToColor(Color_0_1 color, Vector3 vector, Vector3 cameraPosition)
    {
        var dist = Vector3.Distance(vector, cameraPosition);
        var f = (MaxDistance - dist) / MaxDistance;
        color.R = color.R * f + (1 - f) * FogColor.R / 255;
        color.G = color.G * f + (1 - f) * FogColor.G / 255;
        color.B = color.B * f + (1 - f) * FogColor.B / 255;

        return color;
    }
}