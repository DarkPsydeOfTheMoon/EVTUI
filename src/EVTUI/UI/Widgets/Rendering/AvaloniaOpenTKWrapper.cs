using System;
using Avalonia.OpenGL;
using OpenTK;


class AvaloniaOpenTKWrapper : OpenTK.IBindingsContext
{
    private readonly Avalonia.OpenGL.GlInterface _glInterface;

    public AvaloniaOpenTKWrapper(Avalonia.OpenGL.GlInterface glInterface)
    {
        _glInterface = glInterface;
    }

    public IntPtr GetProcAddress(string procName) => _glInterface.GetProcAddress(procName);
}
