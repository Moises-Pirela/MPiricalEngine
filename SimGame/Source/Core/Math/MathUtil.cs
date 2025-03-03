using System;
using System.Numerics;
using Microsoft.Xna.Framework;
using Matrix = System.Numerics.Matrix4x4;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;

namespace MPirical.Core.Math
{
    /// <summary>
    /// Provides mathematical utility functions beyond what's available in System.Math
    /// and Microsoft.Xna.Framework.MathHelper
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Value very close to zero, used for floating point comparisons
        /// </summary>
        public const float Epsilon = 0.00001f;
        
        /// <summary>
        /// Converts degrees to radians
        /// </summary>
        /// <param name="degrees">Angle in degrees</param>
        /// <returns>Angle in radians</returns>
        public static float ToRadians(float degrees)
        {
            return degrees * (MathF.PI / 180.0f);
        }
        
        /// <summary>
        /// Converts radians to degrees
        /// </summary>
        /// <param name="radians">Angle in radians</param>
        /// <returns>Angle in degrees</returns>
        public static float ToDegrees(float radians)
        {
            return radians * (180.0f / MathF.PI);
        }
        
        /// <summary>
        /// Linearly interpolates between two values
        /// </summary>
        /// <param name="a">Start value</param>
        /// <param name="b">End value</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>Interpolated value</returns>
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }
        
        /// <summary>
        /// Linearly interpolates between two vectors
        /// </summary>
        /// <param name="a">Start vector</param>
        /// <param name="b">End vector</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>Interpolated vector</returns>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            t = Clamp01(t);
            return new Vector3(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }
        
        /// <summary>
        /// Clamps a value between 0 and 1
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <returns>Clamped value</returns>
        public static float Clamp01(float value)
        {
            return value < 0.0f ? 0.0f : value > 1.0f ? 1.0f : value;
        }
        
        /// <summary>
        /// Clamps a value between min and max
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Clamped value</returns>
        public static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
        
        /// <summary>
        /// Clamps a vector's magnitude between min and max
        /// </summary>
        /// <param name="vector">Vector to clamp</param>
        /// <param name="maxLength">Maximum length</param>
        /// <returns>Clamped vector</returns>
        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
            float sqrMagnitude = vector.LengthSquared();
            if (sqrMagnitude > maxLength * maxLength)
            {
                float magnitude = MathF.Sqrt(sqrMagnitude);
                return vector * (maxLength / magnitude);
            }
            return vector;
        }
        
        /// <summary>
        /// Moves a value towards a target
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="maxDelta">Maximum change</param>
        /// <returns>New value</returns>
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathF.Abs(target - current) <= maxDelta)
                return target;
            return current + MathF.Sign(target - current) * maxDelta;
        }
        
        /// <summary>
        /// Moves a vector towards a target
        /// </summary>
        /// <param name="current">Current vector</param>
        /// <param name="target">Target vector</param>
        /// <param name="maxDistanceDelta">Maximum distance change</param>
        /// <returns>New vector</returns>
        public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        {
            Vector3 toVector = target - current;
            float dist = toVector.Length();
            
            if (dist <= maxDistanceDelta || dist < Epsilon)
                return target;
            
            return current + toVector / dist * maxDistanceDelta;
        }
        
        /// <summary>
        /// Smoothly interpolates between two values using a spring-damper function
        /// </summary>
        /// <param name="current">Current value</param>
        /// <param name="target">Target value</param>
        /// <param name="currentVelocity">Current velocity (ref parameter, modified by the function)</param>
        /// <param name="smoothTime">Approximate time to reach target</param>
        /// <param name="deltaTime">Time since last update</param>
        /// <param name="maxSpeed">Maximum speed</param>
        /// <returns>Smoothed value</returns>
        public static float SmoothDamp(
            float current, float target, ref float currentVelocity, 
            float smoothTime, float deltaTime, float maxSpeed = float.MaxValue)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = MathF.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;
            
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            
            float change = current - target;
            float originalTo = target;
            
            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = Clamp(change, -maxChange, maxChange);
            target = current - change;
            
            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;
            
            // Prevent overshooting
            if (originalTo - current > 0.0f == output > originalTo)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }
            
            return output;
        }
        
        /// <summary>
        /// Smoothly interpolates between two vectors using a spring-damper function
        /// </summary>
        /// <param name="current">Current vector</param>
        /// <param name="target">Target vector</param>
        /// <param name="currentVelocity">Current velocity (ref parameter, modified by the function)</param>
        /// <param name="smoothTime">Approximate time to reach target</param>
        /// <param name="deltaTime">Time since last update</param>
        /// <param name="maxSpeed">Maximum speed</param>
        /// <returns>Smoothed vector</returns>
        public static Vector3 SmoothDamp(
            Vector3 current, Vector3 target, ref Vector3 currentVelocity, 
            float smoothTime, float deltaTime, float maxSpeed = float.MaxValue)
        {
            float vx = currentVelocity.X;
            float vy = currentVelocity.Y;
            float vz = currentVelocity.Z;
            
            float x = SmoothDamp(current.X, target.X, ref vx, smoothTime, deltaTime, maxSpeed);
            float y = SmoothDamp(current.Y, target.Y, ref vy, smoothTime, deltaTime, maxSpeed);
            float z = SmoothDamp(current.Z, target.Z, ref vz, smoothTime, deltaTime, maxSpeed);
            
            currentVelocity = new Vector3(vx, vy, vz);
            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Gets a point on a quadratic Bezier curve
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">Control point</param>
        /// <param name="p2">End point</param>
        /// <param name="t">Curve parameter (0-1)</param>
        /// <returns>Point on the curve</returns>
        public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 
                   2f * oneMinusT * t * p1 + 
                   t * t * p2;
        }
        
        /// <summary>
        /// Gets a point on a cubic Bezier curve
        /// </summary>
        /// <param name="p0">Start point</param>
        /// <param name="p1">First control point</param>
        /// <param name="p2">Second control point</param>
        /// <param name="p3">End point</param>
        /// <param name="t">Curve parameter (0-1)</param>
        /// <returns>Point on the curve</returns>
        public static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * p0 + 
                   3f * oneMinusT * oneMinusT * t * p1 + 
                   3f * oneMinusT * t * t * p2 + 
                   t * t * t * p3;
        }
        
        /// <summary>
        /// Checks if two floating point values are approximately equal
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="epsilon">Comparison threshold</param>
        /// <returns>True if values are approximately equal</returns>
        public static bool Approximately(float a, float b, float epsilon = Epsilon)
        {
            return MathF.Abs(a - b) < epsilon;
        }
        
        /// <summary>
        /// Checks if a vector's components are approximately zero
        /// </summary>
        /// <param name="vector">Vector to check</param>
        /// <param name="epsilon">Comparison threshold</param>
        /// <returns>True if vector is approximately zero</returns>
        public static bool ApproximatelyZero(Vector3 vector, float epsilon = Epsilon)
        {
            return MathF.Abs(vector.X) < epsilon && 
                   MathF.Abs(vector.Y) < epsilon && 
                   MathF.Abs(vector.Z) < epsilon;
        }
        
        /// <summary>
        /// Checks if two vectors are approximately equal
        /// </summary>
        /// <param name="a">First vector</param>
        /// <param name="b">Second vector</param>
        /// <param name="epsilon">Comparison threshold</param>
        /// <returns>True if vectors are approximately equal</returns>
        public static bool Approximately(Vector3 a, Vector3 b, float epsilon = Epsilon)
        {
            return MathF.Abs(a.X - b.X) < epsilon && 
                   MathF.Abs(a.Y - b.Y) < epsilon && 
                   MathF.Abs(a.Z - b.Z) < epsilon;
        }
        
        /// <summary>
        /// Converts a direction vector to a rotation quaternion
        /// </summary>
        /// <param name="direction">Direction vector (should be normalized)</param>
        /// <param name="up">Up vector (default is Vector3.UnitY)</param>
        /// <returns>Rotation quaternion</returns>
        public static Quaternion LookRotation(Vector3 direction, Vector3 up = default)
        {
            if (up == default)
                up = Vector3.UnitY;
            
            // Early exit if direction is approximately zero
            if (ApproximatelyZero(direction))
                return Quaternion.Identity;
            
            // Normalize the direction vector
            direction = Vector3.Normalize(direction);
            
            // Compute the rotation matrix
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, direction));
            up = Vector3.Cross(direction, right);
            
            // Convert the rotation matrix to a quaternion
            Matrix rotationMatrix = Matrix.Identity;
            rotationMatrix.M11 = right.X;
            rotationMatrix.M12 = right.Y;
            rotationMatrix.M13 = right.Z;
            rotationMatrix.M21 = up.X;
            rotationMatrix.M22 = up.Y;
            rotationMatrix.M23 = up.Z;
            rotationMatrix.M31 = direction.X;
            rotationMatrix.M32 = direction.Y;
            rotationMatrix.M33 = direction.Z;
            
            Quaternion rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);
            
            return rotation;
        }
        
        /// <summary>
        /// Smoothly interpolates between two rotations
        /// </summary>
        /// <param name="from">Start rotation</param>
        /// <param name="to">End rotation</param>
        /// <param name="t">Interpolation factor (0-1)</param>
        /// <returns>Interpolated rotation</returns>
        public static Quaternion Slerp(Quaternion from, Quaternion to, float t)
        {
            t = Clamp01(t);
            return Quaternion.Slerp(from, to, t);
        }
        
        /// <summary>
        /// Maps a value from one range to another
        /// </summary>
        /// <param name="value">Value to map</param>
        /// <param name="fromMin">Source range minimum</param>
        /// <param name="fromMax">Source range maximum</param>
        /// <param name="toMin">Target range minimum</param>
        /// <param name="toMax">Target range maximum</param>
        /// <returns>Mapped value</returns>
        public static float Map(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }
        
        /// <summary>
        /// Maps a value from one range to another, clamping the result
        /// </summary>
        /// <param name="value">Value to map</param>
        /// <param name="fromMin">Source range minimum</param>
        /// <param name="fromMax">Source range maximum</param>
        /// <param name="toMin">Target range minimum</param>
        /// <param name="toMax">Target range maximum</param>
        /// <returns>Mapped value, clamped to the target range</returns>
        public static float MapClamped(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float mapped = Map(value, fromMin, fromMax, toMin, toMax);
            return Clamp(mapped, MathF.Min(toMin, toMax), MathF.Max(toMin, toMax));
        }
        
        /// <summary>
        /// Gets the shortest angle between two angles
        /// </summary>
        /// <param name="from">Start angle in degrees</param>
        /// <param name="to">End angle in degrees</param>
        /// <returns>Shortest angle in degrees</returns>
        public static float DeltaAngle(float from, float to)
        {
            float delta = (to - from) % 360.0f;
            if (delta > 180.0f)
                delta -= 360.0f;
            else if (delta < -180.0f)
                delta += 360.0f;
            return delta;
        }
        
        /// <summary>
        /// Convert from System.Numerics.Vector3 to Microsoft.Xna.Framework.Vector3
        /// </summary>
        /// <param name="vector">Vector to convert</param>
        /// <returns>Converted vector</returns>
        public static Microsoft.Xna.Framework.Vector3 ToXna(Vector3 vector)
        {
            return new Microsoft.Xna.Framework.Vector3(vector.X, vector.Y, vector.Z);
        }
        
        /// <summary>
        /// Convert from Microsoft.Xna.Framework.Vector3 to System.Numerics.Vector3
        /// </summary>
        /// <param name="vector">Vector to convert</param>
        /// <returns>Converted vector</returns>
        public static Vector3 FromXna(Microsoft.Xna.Framework.Vector3 vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
        
        /// <summary>
        /// Convert from System.Numerics.Quaternion to Microsoft.Xna.Framework.Quaternion
        /// </summary>
        /// <param name="quaternion">Quaternion to convert</param>
        /// <returns>Converted quaternion</returns>
        public static Microsoft.Xna.Framework.Quaternion ToXna(Quaternion quaternion)
        {
            return new Microsoft.Xna.Framework.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
        
        /// <summary>
        /// Convert from Microsoft.Xna.Framework.Quaternion to System.Numerics.Quaternion
        /// </summary>
        /// <param name="quaternion">Quaternion to convert</param>
        /// <returns>Converted quaternion</returns>
        public static Quaternion FromXna(Microsoft.Xna.Framework.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }
        
        /// <summary>
        /// Convert from System.Numerics.Matrix4x4 to Microsoft.Xna.Framework.Matrix
        /// </summary>
        /// <param name="matrix">Matrix to convert</param>
        /// <returns>Converted matrix</returns>
        public static Microsoft.Xna.Framework.Matrix ToXna(Matrix matrix)
        {
            return new Microsoft.Xna.Framework.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }
        
        /// <summary>
        /// Convert from Microsoft.Xna.Framework.Matrix to System.Numerics.Matrix4x4
        /// </summary>
        /// <param name="matrix">Matrix to convert</param>
        /// <returns>Converted matrix</returns>
        public static Matrix FromXna(Microsoft.Xna.Framework.Matrix matrix)
        {
            return new Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }
    }
}