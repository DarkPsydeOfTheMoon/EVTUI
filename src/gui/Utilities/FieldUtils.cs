using System;
using System.Collections.Generic;
using System.Linq;

namespace EVTUI.ViewModels;

public static class FieldUtils
{

    // deprecated in favor of BiDict, but leaving in for posterity... for now
    /*public static List<string> NameList(Enum _enum, bool replaceUnderscores = true)
    {
        if (replaceUnderscores)
            return (Enum.GetNames(_enum.GetType())).Select(x => x.Replace("__", " ").Replace("_", "-")).ToList();
        else
            return (Enum.GetNames(_enum.GetType())).ToList();
    }

    public static string NumToName(Enum _enum, dynamic val, bool replaceUnderscores = true)
    {
        if (replaceUnderscores)
            return Enum.GetName(_enum.GetType(), val).Replace("__", " ").Replace("_", "-");
        else
            return Enum.GetName(_enum.GetType(), val);
    }

    public static dynamic NameToNum(Enum _enum, string name, bool replaceUnderscores = true)
    {
        if (replaceUnderscores)
            return Enum.Parse(_enum.GetType(), name.Replace(" ", "__").Replace("-", "_"));
        else
            return Enum.Parse(_enum.GetType(), name);
    }*/

    public static bool BitToBool(uint field, int pos)
    {
        return (((field >> pos) & 1) != 0);
    }

    // useless because properties can't be passed by reference...
    //public static void SetBit(ref dynamic field, int pos, bool val)
    //{
    //    field &= ~(1 << pos);
    //    field |= (Convert.ToInt32(val) << pos);
    //}

    public static int BoolToBit(bool val, int pos)
    {
        return Convert.ToInt32(val) << pos;
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
