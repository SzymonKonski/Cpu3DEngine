using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Cpu3DEngine;

public class Face
{
    public bool ChangeColor = false;

    public Color Color = Color.White;

    public Face(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n0, Vector3 n1, Vector3 n2)
    {
        Points = new List<Vertex>
        {
            new(v0.X, v0.Y, v0.Z) {Normal = new Vector4(n0.X, n0.Y, n0.Z, -(n0.X * v0.X + n0.Y * v0.Y + n0.Z * v0.Z))},
            new(v1.X, v1.Y, v1.Z) {Normal = new Vector4(n1.X, n1.Y, n1.Z, -(n1.X * v1.X + n1.Y * v1.Y + n1.Z * v1.Z))},
            new(v2.X, v2.Y, v2.Z) {Normal = new Vector4(n2.X, n1.Y, n2.Z, -(n2.X * v2.X + n2.Y * v2.Y + n2.Z * v2.Z))}
        };
    }

    public List<Vertex> Points { get; set; }

    public bool IsShining { get; set; } = false;

    public Vector4 Normal => 1.0f / 3 * (Points[0].TransformedNormal + Points[1].TransformedNormal + Points[2].TransformedNormal);

    public Vector4 Position => 1.0f / 3 * (Points[0].Vector + Points[1].Vector + Points[2].Vector);
}