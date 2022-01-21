using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Cpu3DEngine
{
    public static class Shading
    {
        public static Color Phong(Vector3 position, Vector3 normal, Color_0_1 color01, MeshLightParameters mesh,
            List<Light> lights, Vector3 cameraPosition, FogGenerator? fog)
        {
            var resultColor = new Color_0_1 {R = 0, G = 0, B = 0};

            //ambient
            resultColor.R += color01.R * mesh.Ka;
            resultColor.G += color01.G * mesh.Ka;
            resultColor.B += color01.B * mesh.Ka;

            foreach (var light in lights)
            {
                if (light.IsTurnedOn == false)
                    continue;

                double spotlightFactor = 1;

                var L = Vector3.Normalize(light.TransformedPosition - position);

                if (light.IsSpotLight)
                {
                    var D = light.TransformedDirection;
                    var cos = Vector3.Dot(-D, L);
                    spotlightFactor = cos > 0 ? Math.Pow(cos, light.P) : 0;
                }

                //diffuse
                var lightNormalAngle = Vector3.Dot(normal, L);
                var diffuseR = mesh.Kd * lightNormalAngle * spotlightFactor;
                if (lightNormalAngle < 0) continue;

                resultColor.R += color01.R * diffuseR;
                resultColor.G += color01.G * diffuseR;
                resultColor.B += color01.B * diffuseR;      

                //specular
                var V = Vector3.Normalize(-position);

                var R = 2 * lightNormalAngle * normal - L;
                R = Vector3.Normalize(R);

                var cameraRAngle = Vector3.Dot(R, V);

                if (cameraRAngle < 0) continue;

                var specularR = mesh.Ks * Math.Pow(cameraRAngle, mesh.M) * spotlightFactor;

                resultColor.R += specularR;
                resultColor.G += specularR;
                resultColor.B += specularR;
            }

            if (fog != null)
                resultColor = fog.ApplyFogToColor(resultColor, position, cameraPosition);

            if (resultColor.R > 1) resultColor.R = 1;
            if (resultColor.G > 1) resultColor.G = 1;
            if (resultColor.B > 1) resultColor.B = 1;

            return ScaleFrom_0_1_To_0_255(resultColor);
        }


        public static Color_0_1 Scale_From_0_255_To_0_1(Color color)
        {
            return new Color_0_1 {R = color.R / 255.0, G = color.G / 255.0, B = color.B / 255.0};
        }

        public static Color ScaleFrom_0_1_To_0_255(Color_0_1 color)
        {
            return Color.FromArgb((int) (color.R * 255), (int) (color.G * 255), (int) (color.B * 255));
        }

        public static float GetTriangleDenominator(Point v1, Point v2, Point v3)
        {
            var triangleDenominator = (v2.Y - v3.Y) * (v1.X - v3.X) + (v3.X - v2.X) * (v1.Y - v3.Y);
            if (triangleDenominator == 0)
                triangleDenominator = 1;

            return triangleDenominator;
        }

        public static (float, float, float) GetBarycentricCoordinates(int pX, int pY, Point v1, Point v2, Point v3,
            float denominator)
        {
            var w1 = ((v2.Y - v3.Y) * (pX - v3.X) + (v3.X - v2.X) * (pY - v3.Y)) / denominator;

            if (w1 < 0) w1 = 0;

            var w2 = ((v3.Y - v1.Y) * (pX - v3.X) + (v1.X - v3.X) * (pY - v3.Y)) / denominator;
            if (w2 < 0) w2 = 0;

            var w3 = 1 - w1 - w2;
            if (w3 < 0)
            {
                w3 = 0;
                w1 = 1 - w2;
            }

            return (w1, w2, w3);
        }
    }
}

public class EdgeStruct
{
    public double InvertM;
    public double X;
    public int YMax;
}

public struct Color_0_1
{
    public double R;
    public double G;
    public double B;
}

public struct MeshLightParameters
{
    public float Ka;
    public float Kd;
    public float Ks;
    public int M;
}

public enum ShadingType
{
    Flat,
    Gouraud,
    Phong,
    None
}