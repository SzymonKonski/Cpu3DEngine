using System.Numerics;

namespace Cpu3DEngine;

public static class Library
{
    public static Vector4 Multiply(this Matrix4x4 matrix, Vector4 self)
    {
        return new Vector4(
            matrix.M11 * self.X + matrix.M12 * self.Y + matrix.M13 * self.Z + matrix.M14 * self.W,
            matrix.M21 * self.X + matrix.M22 * self.Y + matrix.M23 * self.Z + matrix.M24 * self.W,
            matrix.M31 * self.X + matrix.M32 * self.Y + matrix.M33 * self.Z + matrix.M34 * self.W,
            matrix.M41 * self.X + matrix.M42 * self.Y + matrix.M43 * self.Z + matrix.M44 * self.W
        );
    }
}

public enum Axis
{
    X,
    Y,
    Z
}