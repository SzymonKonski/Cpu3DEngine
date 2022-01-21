using System;
using System.Drawing;
using System.Numerics;

namespace Cpu3DEngine;

public static class ShapesGenerator
{
    private static readonly Random Rnd = new();

    public static Mesh CreateFloor(Color color)
    {
        var floor = new Mesh();

        Vector3[] vertices =
        {
            new(-1, 0, 1),
            new(1, 0, 1),
            new(1, 0, -1),
            new(-1, 0, -1),
        };

        Vector3[] normals =
        {
            new(0, 1, 0),
            new(0, 1, 0),
            new(0, 1, 0),
            new(0, 1, 0)
        };

        floor.Faces.Add(new Face(vertices[0], vertices[1], vertices[3],
            normals[0], normals[1], normals[2])
        {
            Color = color
        });

        floor.Faces.Add(new Face(vertices[3], vertices[2], vertices[1],
            normals[0], normals[2], normals[3])
        {
            Color = color
        });

        return floor;
    }

    public static Mesh CreateCube()
    {
        var cube = new Mesh();

        Vector3[] vertices =
        {
            new(-1, -1, -1),
            new(1, -1, -1),
            new(1, 1, -1),
            new(-1, 1, -1),
            new(-1, 1, 1),
            new(1, 1, 1),
            new(1, -1, 1),
            new(-1, -1, 1)
        };

        Vector3[] normals =
        {
            new(0, 0, -1),
            new(0, 1, 0),
            new(1, 0, 0),
            new(-1, 0, 0),
            new(0, 0, 1),
            new(0, -1, 0)
        };

        int[] triangles =
        {
            0, 2, 1, 
            0, 3, 2,
            2, 3, 4, 
            2, 4, 5,
            1, 2, 5, 
            1, 5, 6,
            0, 7, 4, 
            0, 4, 3,
            5, 4, 7, 
            5, 7, 6,
            0, 6, 7,
            0, 1, 6
        };

        var color = Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256));

        for (var i = 0; i < triangles.Length; i += 3)
            cube.Faces.Add(new Face(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]], normals[i / 6], normals[i / 6], normals[i / 6])
            {
                Color = color
            });

        return cube;
    }


    public static Mesh CreateSphere(int recursionLevel, Color color)
    {
        var generator = new SphereGenerator();
        var faces = generator.Create(recursionLevel);
        var sphere = new Mesh();

        foreach (var face in faces)
        {
            var side = new Face(face.V1, face.V2, face.V3, Vector3.Normalize(face.V1), Vector3.Normalize(face.V2), Vector3.Normalize(face.V3))
            {
                Color = color
            };
            sphere.Faces.Add(side);
        }

        return sphere;
    }

    public static Mesh CreateCone(int subdivisions, float radius, float height)
    {
        var cone = new Mesh();

        var vertices = new Vector3[subdivisions + 2];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[subdivisions * 2 * 3];

        vertices[0] = new Vector3(0, 0, 1);
        uv[0] = new Vector2(0.5f, 0f);
        for (int i = 0, n = subdivisions - 1; i < subdivisions; i++)
        {
            var ratio = (float) i / n;
            var r = (float) (ratio * (Math.PI * 2f));
            var x = (float) (Math.Cos(r) * radius);
            var z = (float) (Math.Sin(r) * radius);
            vertices[i + 1] = new Vector3(x, 0f, z+1);

            uv[i + 1] = new Vector2(ratio, 0f);
        }

        vertices[subdivisions + 1] = new Vector3(0f, height, 1f);
        uv[subdivisions + 1] = new Vector2(0.5f, 1f);

        // construct bottom
        for (int i = 0, n = subdivisions - 1; i < n; i++)
        {
            var offset = i * 3;
            triangles[offset] = 0;
            triangles[offset + 1] = i + 1;
            triangles[offset + 2] = i + 2;
        }

        // construct sides
        var bottomOffset = subdivisions * 3;
        for (int i = 0, n = subdivisions - 1; i < n; i++)
        {
            var offset = i * 3 + bottomOffset;
            triangles[offset] = i + 1;
            triangles[offset + 1] = subdivisions + 1;
            triangles[offset + 2] = i + 2;
        }


        var color = Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256));

        for (var i = 0; i < triangles.Length; i += 3)
        {
            var v1 = triangles[i];
            var v2 = triangles[i + 1];
            var v3 = triangles[i + 2];

            var nv1 = Vector3.Normalize(vertices[v1]);
            var nv2 = Vector3.Normalize(vertices[v2]);
            var nv3 = Vector3.Normalize(vertices[v3]);

            var side = new Face(vertices[v1], vertices[v2], vertices[v3], nv1, nv2, nv3)
            {
                Color = color
            };
            cone.Faces.Add(side);
        }


        return cone;
    }

    public static Mesh CreateCylinder(int division, double cylinderLength, double cylinderRadius, Color color)
    {
        var cylinder = new Mesh {Color = color};
        var angleDifference = 2 * Math.PI / division;
        double end = -cylinderLength/2;
        var beginning = cylinderLength/2;

        for (var i = 0; i < division; i++)
        {
            var firstAngle = angleDifference * i - Math.PI;
            var secondAngle = angleDifference * (i + 1) - Math.PI;

            var firstX = cylinderRadius * Math.Sin(firstAngle);
            var firstY = cylinderRadius * Math.Cos(firstAngle);

            var secondX = cylinderRadius * Math.Sin(secondAngle);
            var secondY = cylinderRadius * Math.Cos(secondAngle);

            var ending = new Face(new Vector3(0, 0, (float) end),
                    new Vector3((float) firstX, (float) firstY, (float) end),
                    new Vector3((float) secondX, (float) secondY, (float) end),
                    new Vector3(0, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 0, -1))
                {Color = color};


            var longSide1 = new Face(new Vector3((float) firstX, (float) firstY, (float) end),
                    new Vector3((float) secondX, (float) secondY, (float) end),
                    new Vector3((float) firstX, (float) firstY, (float) beginning),
                    Vector3.Normalize(new Vector3((float) firstX, (float) firstY, 0)),
                    Vector3.Normalize(new Vector3((float) secondX, (float) secondY, 0)),
                    Vector3.Normalize(new Vector3((float) firstX, (float) firstY, 0)))
                {Color = color};

            var longSide2 = new Face(new Vector3((float) secondX, (float) secondY, (float) end),
                    new Vector3((float) secondX, (float) secondY, (float) beginning),
                    new Vector3((float) firstX, (float) firstY, (float) beginning),
                    Vector3.Normalize(new Vector3((float) secondX, (float) secondY, 0)),
                    Vector3.Normalize(new Vector3((float) secondX, (float) secondY, 0)),
                    Vector3.Normalize(new Vector3((float) firstX, (float) firstY, 0)))
                {Color = color};


            var opening = new Face(new Vector3(0, 0, (float)beginning),
                    new Vector3((float)firstX, (float)firstY, (float)beginning),
                    new Vector3((float)secondX, (float)secondY, (float)beginning),
                    new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1))
                { Color = color, ChangeColor = true };

            cylinder.Faces.Add(opening);
            cylinder.Faces.Add(ending);
            cylinder.Faces.Add(longSide1);
            cylinder.Faces.Add(longSide2);
        }

        return cylinder;
    }
}