using System.IO;
using System.Linq;
using GFDLibrary.Rendering.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace EVTUI.ViewModels;

public class GFDRenderingPanelViewModel : ViewModelBase
{
    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public float r;
    public float g;
    public float b;

    public double width;
    public double height;
    public SceneManager sceneManager { get; set; } = new SceneManager();
    GLShaderProgram glShaderProgram;
    bool testDataInitialised=false;
    
    public GFDRenderingPanelViewModel(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    ///////////////////////////////
    // *** Drawing Functions *** //
    ///////////////////////////////
    public void InitTestData()
    {
        if (this.testDataInitialised)
            return;
        
        this.testDataInitialised = true;

        string modelPath = "./Assets/test_model.GMD";
        string animPath  = "./Assets/test_gap.GAP";
        string vsPath = "./Assets/shaders/default.glsl.vs";
        string fsPath = "./Assets/shaders/default.glsl.fs";

        if (!(File.Exists(modelPath) && File.Exists(vsPath) && File.Exists(fsPath)))
            return;

        this.sceneManager.LoadModel(modelPath);
        this.glShaderProgram = this.sceneManager.LoadShader(vsPath, fsPath);

        if (!File.Exists(animPath))
            return;

        this.sceneManager.LoadGAP(animPath);
        this.sceneManager.ActivateAnimationOnModel(0, 0, 0);
        this.sceneManager.sceneModels.Last().StartAnimTimer();
    }

    public void RefreshSceneState()
    {
        GL.ClearColor(this.r,this.g, this.b, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);
        GL.Viewport(0, 0, (int)this.width, (int)this.height);
        this.sceneManager.activeCamera.AspectRatio = (float)(this.width/this.height);
    }

    public void DrawScene()
    {
        // Each model can have its own shaderprogram if we go that route,
        // meaning that we won't need to pass in a global shader here.
        foreach (var scenemodel in this.sceneManager.sceneModels)
            scenemodel.Draw(this.glShaderProgram, this.sceneManager.activeCamera);
    }
}
