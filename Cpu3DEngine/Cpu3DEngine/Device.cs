using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Cpu3DEngine;

public class Device
{
    private DirectBitmap bitmap;
    public FogGenerator? Fog;
    public Camera SelectedCamera;
    private float[,] zTable;

    public Device(DirectBitmap bitmap)
    {
        Fog = null;
        Bitmap = bitmap;
        Near = 1;
        Far = 100;
        var directBitmap = this.bitmap;
        if (directBitmap != null) Aspect = (float) directBitmap.Height / directBitmap.Width;
        Ka = 0.2f;
        Kd = 1f;
        Ks = 0.2f;
        M = 5;
        ShadingType = ShadingType.Flat;
    }

    public float Ka { get; set; }
    public float Kd { get; set; }
    public float Ks { get; set; }
    public int M { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }
    public float Aspect { get; set; }
    public ShadingType ShadingType { get; set; }
    public List<Mesh> Meshes { get; set; } = new();
    public List<Light> Lights { get; set; } = new();

    public DirectBitmap Bitmap
    {
        get => bitmap;
        set
        {
            if (bitmap != null)
                bitmap.Dispose();

            bitmap = value;
            Aspect = (float) bitmap.Height / bitmap.Width;
        }
    }

    private Matrix4x4 GetProjectionMatrix()
    {
        var e = (float) (1 / Math.Tan(SelectedCamera.Fov * Math.PI / 360));

        return new Matrix4x4
        (
            e, 0, 0, 0,
            0, e / Aspect, 0, 0,
            0, 0, -(Far + Near) / (Far - Near), -(2 * Far * Near) / (Far - Near),
            0, 0, -1, 0
        );
    }

    private void ClearZTable()
    {
        for (var i = 0; i < bitmap.Height; i++)
        for (var j = 0; j < bitmap.Width; j++)
            zTable[j, i] = float.MaxValue;
    }

    public void Render()
    {
        zTable = new float[Bitmap.Width, Bitmap.Height];
        var meshLightParameters = new MeshLightParameters {Ka = Ka, Kd = Kd, Ks = Ks, M = M};
        ClearZTable();

        var projectionMatrix = GetProjectionMatrix();
        var viewMatrix = SelectedCamera.GetViewMatrix();

        foreach (var light in Lights)
            light.Transform(viewMatrix);

        foreach (var mesh in Meshes)
        {
            foreach (var vertex in mesh.Faces.SelectMany(face => face.Points)) vertex.ResetVertex();

            var modelMatrix = mesh.ModelMatrix;
            var viewmodelMatrix = Matrix4x4.Multiply(viewMatrix, modelMatrix);

            foreach (var face in mesh.Faces)
            {
                foreach (var point in face.Points)
                    point.Transform(viewmodelMatrix, true);

                var v = face.Position;
                var n = face.Normal;

                if (face.Points[0].Z > 0 && face.Points[1].Z > 0 && face.Points[2].Z > 0) 
                    continue;

                if (n.X * v.X + n.Y * v.Y + n.Z * v.Z <= 0)
                    DrawTriangle(projectionMatrix, meshLightParameters, face);
            }
        }
    }

    public void DrawTriangle(Matrix4x4 projectionMatrix, MeshLightParameters meshLightParameters, Face face)
    {
        var positions = face.Points.Select(p => new Vector3(p.X, p.Y, p.Z)).ToList();
        var normals = face.Points.Select(p =>
                Vector3.Normalize(new Vector3(p.TransformedNormal.X, p.TransformedNormal.Y, p.TransformedNormal.Z)))
            .ToList();

        foreach (var point in face.Points)
            point.Transform(projectionMatrix, false);

        var projectedPoints = new List<Point>();
        var zPoints = new List<float>();

        foreach (var point in face.Points)
        {
            projectedPoints.Add(new Point((int) ((point.X + 1) * Bitmap.Width / 2),
                (int) ((point.Y + 1) * Bitmap.Width / 2)));
            zPoints.Add(point.Z);
        }

        var shadingType = ShadingType;

        if (face.IsShining)
            shadingType = ShadingType.None;

        Fill(projectedPoints, face.Color, zPoints, positions, normals, meshLightParameters, shadingType);
    }

    public void Fill(List<Point> points, Color color, List<float> zs, List<Vector3> positions, List<Vector3> normals,
        MeshLightParameters mesh, ShadingType shadingType)
    {
        const int margin = 500;
        var maxY = points.Max(x => x.Y);
        if (maxY < -margin || maxY > Bitmap.Height + margin) return;

        var minY = points.Min(x => x.Y);
        if (minY < -margin || minY > Bitmap.Height + margin) return;

        var maxX = points.Max(x => x.X);
        if (maxX < -margin || maxX > Bitmap.Width + margin) return;

        var minX = points.Min(x => x.X);
        if (minX < -margin || minX > Bitmap.Width + margin) return;

        var ET = new List<EdgeStruct>[maxY - minY + 1];

        var denominator = Shading.GetTriangleDenominator(points[0], points[1], points[2]);

        for (var i = 0; i < points.Count; i++)
        {
            var p1 = points[i];
            var p2 = points[i + 1 == points.Count ? 0 : i + 1];
            if (p1.Y == p2.Y) continue;
            if (p1.Y > p2.Y) (p2, p1) = (p1, p2);

            var str = new EdgeStruct
                {YMax = p2.Y, X = p1.Y < p2.Y ? p1.X : p2.X, InvertM = (double) (p1.X - p2.X) / (p1.Y - p2.Y)};

            if (ET[p1.Y - minY] == null) ET[p1.Y - minY] = new List<EdgeStruct>();
            ET[p1.Y - minY].Add(str);
        }

        var AET = new List<EdgeStruct>();
        var y = minY;

        var shadingColor = color;
        var color01 = Shading.Scale_From_0_255_To_0_1(color);

        if (shadingType == ShadingType.Flat)
        {
            var middle = (float) (1.0 / 3) * (positions[0] + positions[1] + positions[2]);
            var faceNormal = Vector3.Normalize(normals[0] + normals[1] + normals[2]);
            shadingColor = Shading.Phong(middle, faceNormal, color01, mesh, Lights, SelectedCamera.Position, Fog);
        }

        var vertexColor1 = Color.White;
        var vertexColor2 = Color.White;
        var vertexColor3 = Color.White;

        if (shadingType == ShadingType.Gouraud)
        {
            vertexColor1 = Shading.Phong(positions[0], normals[0], color01, mesh, Lights, SelectedCamera.Position, Fog);
            vertexColor2 = Shading.Phong(positions[1], normals[1], color01, mesh, Lights, SelectedCamera.Position, Fog);
            vertexColor3 = Shading.Phong(positions[2], normals[2], color01, mesh, Lights, SelectedCamera.Position, Fog);
        }

        while (y <= maxY)
        {
            if (ET[y - minY] != null)
            {
                var tmp = ET[y - minY];
                AET.AddRange(tmp);
            }

            AET = AET.OrderBy(x => x.X).ToList();
            for (var i = 0; i < AET.Count; i += 2)
            {
                if (AET.Count - i == 1) continue;

                for (var j = (int) AET[i].X; j < AET[i + 1].X; j++)
                {
                    if (!(j >= 0 && j < Bitmap.Width && y >= 0 && y < Bitmap.Height)) continue;

                    var w = Shading.GetBarycentricCoordinates(j, y, points[0], points[1], points[2], denominator);

                    if (w.Item1 < 0 || w.Item2 < 0 || w.Item3 < 0) continue;
                    var z = zs[0] * w.Item1 + zs[1] * w.Item2 + zs[2] * w.Item3;

                    if (!(z < zTable[j, y])) continue;

                    zTable[j, y] = z;

                    if (shadingType == ShadingType.Gouraud)
                    {
                        var red = w.Item1 * vertexColor1.R + w.Item2 * vertexColor2.R + w.Item3 * vertexColor3.R;
                        var green = w.Item1 * vertexColor1.G + w.Item2 * vertexColor2.G + w.Item3 * vertexColor3.G;
                        var blue = w.Item1 * vertexColor1.B + w.Item2 * vertexColor2.B + w.Item3 * vertexColor3.B;
                        shadingColor = Color.FromArgb((int) red, (int) green, (int) blue);
                    }
                    else if (shadingType == ShadingType.Phong)
                    {
                        var normal = w.Item1 * normals[0] + w.Item2 * normals[1] + w.Item3 * normals[2];
                        normal = Vector3.Normalize(normal);
                        var position = w.Item1 * positions[0] + w.Item2 * positions[1] + w.Item3 * positions[2];
                        shadingColor = Shading.Phong(position, normal, Shading.Scale_From_0_255_To_0_1(color), mesh,
                            Lights, SelectedCamera.Position, Fog);
                    }

                    Bitmap.SetPixel(j, y, shadingColor);
                }
            }

            foreach (var edge in AET.ToList())
                if (edge.YMax == y + 1)
                    AET.Remove(edge);
                else
                    edge.X += edge.InvertM;
            y++;
        }
    }
}