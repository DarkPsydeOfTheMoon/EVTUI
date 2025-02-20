using System;
using System.IO;
using System.Linq;
using GFDLibrary.Rendering.OpenGL;
using OpenTK.Graphics.OpenGL;

using ReactiveUI;

namespace EVTUI.ViewModels;

public class GFDRenderingPanelViewModel : ViewModelBase
{
    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public float r;
    public float g;
    public float b;
    public float a;

    public double width;
    public double height;
    public SceneManager sceneManager { get; set; } = new SceneManager();
    GLShaderProgram glShaderProgram;

    private bool _readyToRender = false;
    public bool ReadyToRender
    {
        get => _readyToRender;
        set => this.RaiseAndSetIfChanged(ref _readyToRender, value);
    }
    
    public GFDRenderingPanelViewModel(float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 0.0f)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;

        this.WhenAnyValue(x => x.ReadyToRender).Subscribe(x => 
        {
            if (x)
            {
                string vsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GFDStudio", "app_data", "shaders", "default.glsl.vs");
                string fsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GFDStudio", "app_data", "shaders", "default.glsl.fs");
                this.glShaderProgram = this.sceneManager.LoadShader(vsPath, fsPath);
            }
        });
    }

    ///////////////////////////////
    // *** Drawing Functions *** //
    ///////////////////////////////
    public void RefreshSceneState()
    {
        GL.ClearColor(this.r, this.g, this.b, this.a);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);
        GL.Viewport(0, 0, (int)this.width, (int)this.height);
        this.sceneManager.activeCamera.AspectRatio = (float)(this.width/this.height);
    }

    public void DrawScene()
    {
        // Each model can have its own shaderprogram if we go that route,
        // meaning that we won't need to pass in a global shader here.
        foreach (var scenemodel in this.sceneManager.sceneModels.Values)
            scenemodel.Draw(this.glShaderProgram, this.sceneManager.activeCamera);
    }
}
