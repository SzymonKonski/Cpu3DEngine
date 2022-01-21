using System;
using System.Collections.Generic;
using System.Numerics;

namespace Cpu3DEngine;

public class SphereGenerator
{
    private int index;
    private Dictionary<long, int> middlePointIndexCache;

    private List<Vector3> points;

    public Face[] Create(int recursionLevel)
    {
        middlePointIndexCache = new Dictionary<long, int>();
        points = new List<Vector3>();
        index = 0;
        var t = (float) ((1.0 + Math.Sqrt(5.0)) / 2.0);
        var s = 1;

        AddVertex(new Vector3(-s, t, 0));
        AddVertex(new Vector3(s, t, 0));
        AddVertex(new Vector3(-s, -t, 0));
        AddVertex(new Vector3(s, -t, 0));

        AddVertex(new Vector3(0, -s, t));
        AddVertex(new Vector3(0, s, t));
        AddVertex(new Vector3(0, -s, -t));
        AddVertex(new Vector3(0, s, -t));

        AddVertex(new Vector3(t, 0, -s));
        AddVertex(new Vector3(t, 0, s));
        AddVertex(new Vector3(-t, 0, -s));
        AddVertex(new Vector3(-t, 0, s));

        var faces = new List<Face>();

        // 5 faces around point 0
        faces.Add(new Face(points[0], points[11], points[5]));
        faces.Add(new Face(points[0], points[5], points[1]));
        faces.Add(new Face(points[0], points[1], points[7]));
        faces.Add(new Face(points[0], points[7], points[10]));
        faces.Add(new Face(points[0], points[10], points[11]));

        // 5 adjacent faces 
        faces.Add(new Face(points[1], points[5], points[9]));
        faces.Add(new Face(points[5], points[11], points[4]));
        faces.Add(new Face(points[11], points[10], points[2]));
        faces.Add(new Face(points[10], points[7], points[6]));
        faces.Add(new Face(points[7], points[1], points[8]));

        // 5 faces around point 3
        faces.Add(new Face(points[3], points[9], points[4]));
        faces.Add(new Face(points[3], points[4], points[2]));
        faces.Add(new Face(points[3], points[2], points[6]));
        faces.Add(new Face(points[3], points[6], points[8]));
        faces.Add(new Face(points[3], points[8], points[9]));

        // 5 adjacent faces 
        faces.Add(new Face(points[4], points[9], points[5]));
        faces.Add(new Face(points[2], points[4], points[11]));
        faces.Add(new Face(points[6], points[2], points[10]));
        faces.Add(new Face(points[8], points[6], points[7]));
        faces.Add(new Face(points[9], points[8], points[1]));


        // refine triangles
        for (var i = 0; i < recursionLevel; i++)
        {
            var faces2 = new List<Face>();
            foreach (var tri in faces)
            {
                // replace triangle by 4 triangles
                var a = GetMiddlePoint(tri.V1, tri.V2);
                var b = GetMiddlePoint(tri.V2, tri.V3);
                var c = GetMiddlePoint(tri.V3, tri.V1);

                faces2.Add(new Face(tri.V1, points[a], points[c]));
                faces2.Add(new Face(tri.V2, points[b], points[a]));
                faces2.Add(new Face(tri.V3, points[c], points[b]));
                faces2.Add(new Face(points[a], points[b], points[c]));
            }

            faces = faces2;
        }

        return faces.ToArray();
    }

    private int AddVertex(Vector3 p)
    {
        points.Add(Vector3.Normalize(p));
        return index++;
    }

    // return index of point in the middle of p1 and p2
    private int GetMiddlePoint(Vector3 point1, Vector3 point2)
    {
        long i1 = points.IndexOf(point1);
        long i2 = points.IndexOf(point2);
        // first check if we have it already
        var firstIsSmaller = i1 < i2;
        var smallerIndex = firstIsSmaller ? i1 : i2;
        var greaterIndex = firstIsSmaller ? i2 : i1;
        var key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (middlePointIndexCache.TryGetValue(key, out ret)) return ret;

        // not in cache, calculate it

        var middle = new Vector3(
            (point1.X + point2.X) / 2.0f,
            (point1.Y + point2.Y) / 2.0f,
            (point1.Z + point2.Z) / 2.0f);

        // add vertex makes sure point is on unit sphere
        var i = AddVertex(middle);

        // store it, return index
        middlePointIndexCache.Add(key, i);
        return i;
    }

    public struct Face
    {
        public Vector3 V1;
        public Vector3 V2;
        public Vector3 V3;

        public Face(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
    }
}