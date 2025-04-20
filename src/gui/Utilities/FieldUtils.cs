using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EVTUI.ViewModels;

public static class FieldUtils
{

    /*************************/
    /*** UNIT VECTOR UTILS ***/
    /*************************/

    public static double VectorToAzimuth(float[] xyz)
    {
        return Double.RadiansToDegrees(Math.Atan2(xyz[0], xyz[2]));
    }

    public static double VectorToElevation(float[] xyz)
    {
        if (xyz[0] == 0 && xyz[2] == 0)
            return (xyz[1] < 0) ? -90.0 : 90.0;
        Vector3 direction = new Vector3(xyz[0], xyz[1], xyz[2]);
        Vector3 projection = new Vector3(xyz[0], 0, xyz[2]);
        return Double.RadiansToDegrees(((xyz[1] < 0) ? -1.0 : 1.0)*Math.Acos(Vector3.Dot(Vector3.Normalize(direction), Vector3.Normalize(projection))));
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
}
