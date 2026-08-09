using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace EVTUI;

public static class Utils
{
    // credit: https://www.techiedelight.com/generate-md5-hash-of-string-csharp/
    public static string Hashify(string s)
    {
        StringBuilder sb = new StringBuilder();
        using (MD5 md5 = MD5.Create())
        {
            foreach (byte b in md5.ComputeHash(Encoding.UTF8.GetBytes(s)))
                sb.Append($"{b:X2}");
        }
        return sb.ToString();
    }

    public static void CheckBytes(byte[] bytes, byte expectedValue)
    {
        foreach (byte actualValue in bytes)
            if (actualValue != expectedValue)
            {
                Trace.TraceWarning($"Expected sequence of bytes with value {expectedValue} but reached a byte with value {actualValue} instead");
                break;
            }
    }

    /*************************/
    /*** UNIT VECTOR UTILS ***/
    /*************************/

    public static double VectorToAzimuth(float[] xyz)
    {
        return Double.RadiansToDegrees(Math.Atan2(xyz[0], xyz[2]));
    }

    public static double VectorToAzimuth(Vector3 vec)
    {
        Vector3 norm = Vector3.Normalize(vec);
        return Math.Atan2(norm.X, norm.Z);
    }

    public static double VectorToElevation(float[] xyz)
    {
        if (xyz[0] == 0 && xyz[2] == 0)
            return (xyz[1] < 0) ? -90.0 : 90.0;
        Vector3 direction = new Vector3(xyz[0], xyz[1], xyz[2]);
        Vector3 projection = new Vector3(xyz[0], 0, xyz[2]);
        return Double.RadiansToDegrees(((xyz[1] < 0) ? -1.0 : 1.0)*Math.Acos(Vector3.Dot(Vector3.Normalize(direction), Vector3.Normalize(projection))));
    }

    public static double VectorToElevation(Vector3 vec)
    {
        if (vec.X == 0 && vec.Z == 0)
            return (vec.Y < 0) ? -(Math.PI/2) : (Math.PI/2);
        Vector3 direction = Vector3.Normalize(vec);
        Vector3 projection = Vector3.Normalize(new Vector3(vec.X, 0, vec.Z));
        return ((vec.Y < 0) ? -1.0 : 1.0)*Math.Acos(Vector3.Dot(direction, projection));
    }

    public static float[] AnglesToVector(double azimuth, double elevation)
    {
        azimuth = Double.DegreesToRadians(azimuth);
        elevation = Double.DegreesToRadians(elevation);
        double x = Math.Cos(elevation)*Math.Sin(azimuth);
        double y = Math.Sin(elevation);
        double z = Math.Cos(elevation)*Math.Cos(azimuth);
        Vector3 norm = Vector3.Normalize(new Vector3((float)x, (float)y, (float)z));
        return new float[] { norm.X, norm.Y, norm.Z };
    }

    public static double NormalizeAngle(float angle, bool radians=false)
    {
        double unit = (radians) ? Math.PI : 180.0;
        double ret = angle;
        ret = ((ret % (2*unit)) + (2*unit)) % (2*unit);
        ret = (ret > unit) ? ret - (2*unit) : ret;
        return ret;
    }
}

public class BiDict<TKey, TValue>
{
    public Dictionary<TKey, TValue> Forward = new Dictionary<TKey, TValue>();
    public Dictionary<TValue, TKey> Backward = new Dictionary<TValue, TKey>();

    public BiDict(Dictionary<TKey, TValue> init = null)
    {
        if (!(init is null))
            foreach (TKey key in init.Keys)
                this.Add(key, init[key]);
    }

    public void Add(TKey key, TValue value)
    {
        this.Forward[key] = value;
        this.Backward[value] = key;
    }

    public List<TKey> Keys { get => this.Forward.Keys.ToList(); }

    public List<TValue> Values { get => this.Backward.Keys.ToList(); }

    public void Clear()
    {
        this.Forward.Clear();
        this.Backward.Clear();
    }
}
