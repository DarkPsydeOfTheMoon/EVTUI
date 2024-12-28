using System;

namespace EVTUI.Test;

sealed class Program
{
    public static void Main(string[] args)
    {
        SerialTests.TestEVT();
        SerialTests.TestBMD();
        Console.WriteLine("All tests succeeded!");
    }
}
