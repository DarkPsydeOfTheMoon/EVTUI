using System;

namespace EVTUI.Test;

sealed class Program
{
    public static void Main(string[] args)
    {
        SerialTests.TestEVT();
        SerialTests.TestBMD();
        Console.WriteLine("All Serialization tests succeeded!");

        ASTTests.TestDecompileBF();
        ASTTests.TestCompileFlow();
        ASTTests.TestDecompileBMD();
        ASTTests.TestCompileMsg();
        Console.WriteLine("All AtlusScriptLibrary tests succeeded!");

        Console.WriteLine("All tests succeeded!");
    }
}
