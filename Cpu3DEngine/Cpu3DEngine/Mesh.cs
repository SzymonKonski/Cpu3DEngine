using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Cpu3DEngine;

public class Mesh
{
    public Color Color { get; set; }
    public List<Face> Faces { get; set; } = new();

    public Matrix4x4 ModelMatrix { get; set; } = new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1
    );

    public void Rotate(Axis axis, double angle)
    {
        Matrix4x4 rotationMatrix;

        if (axis == Axis.X)
            rotationMatrix = new Matrix4x4
            (
                1, 0, 0, 0,
                0, (float) Math.Cos(angle), (float) -Math.Sin(angle), 0,
                0, (float) Math.Sin(angle), (float) Math.Cos(angle), 0,
                0, 0, 0, 1
            );
        else if (axis == Axis.Y)
            rotationMatrix = new Matrix4x4
            (
                (float) Math.Cos(angle), 0, (float) Math.Sin(angle), 0,
                0, 1, 0, 0,
                (float) -Math.Sin(angle), 0, (float) Math.Cos(angle), 0,
                0, 0, 0, 1
            );
        else if (axis == Axis.Z)
            rotationMatrix = new Matrix4x4
            (
                (float) Math.Cos(angle), (float) -Math.Sin(angle), 0, 0,
                (float) Math.Sin(angle), (float) Math.Cos(angle), 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
        else
            return;

        ModelMatrix = Matrix4x4.Multiply(rotationMatrix, ModelMatrix);
    }

    public void ResetModelMatrix()
    {
        ModelMatrix = new Matrix4x4
        (
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        );
    }

    public void Translate(float x, float y, float z)
    {
        var translationMatrix = new Matrix4x4
        (
            1, 0, 0, x,
            0, 1, 0, y,
            0, 0, 1, z,
            0, 0, 0, 1
        );

        ModelMatrix = Matrix4x4.Multiply(translationMatrix, ModelMatrix);
    }

    public void Scale(float x, float y, float z)
    {
        var scaleMatrix = new Matrix4x4
        (
            x, 0, 0, 0,
            0, y, 0, 0,
            0, 0, z, 0,
            0, 0, 0, 1
        );

        ModelMatrix = Matrix4x4.Multiply(scaleMatrix, ModelMatrix);
    }

    public void ChangeMarkedFacesColor(Color color, bool shining)
    {
        foreach (var face in Faces.Where(face => face.ChangeColor))
        {
            face.IsShining = shining;
            face.Color = color;
        }
    }
}