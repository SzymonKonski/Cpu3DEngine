using System.Numerics;

namespace Cpu3DEngine;

public class Camera
{
    public Camera(Vector3 position, Vector3 target, Vector3 upVector, int fov)
    {
        Position = position;
        Target = target;
        UpVector = upVector;
        Fov = fov;
    }

    public Vector3 Position { get; set; }

    public Vector3 Target { get; set; }

    public Vector3 UpVector { get; set; }

    public int Fov { get; set; }

    public Matrix4x4 GetViewMatrix()
    {
        var za = Vector3.Normalize(Position - Target);
        var xa = Vector3.Normalize(Vector3.Cross(UpVector, za));
        var ya = Vector3.Normalize(Vector3.Cross(za, xa));

        var matrix1 = new Matrix4x4
        (
            xa.X, ya.X, za.X, Position.X,
            xa.Y, ya.Y, za.Y, Position.Y,
            xa.Z, ya.Z, za.Z, Position.Z,
            0, 0, 0, 1
        );

        Matrix4x4.Invert(matrix1, out matrix1);

        return matrix1;
    }
}