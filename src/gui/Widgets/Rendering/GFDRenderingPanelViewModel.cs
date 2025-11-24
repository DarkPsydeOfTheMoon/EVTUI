using System;
using System.Diagnostics;
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
    GLShaderProgram glShaderProgram;

    public SceneManager sceneManager { get; set; } = new SceneManager();

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

    /////////////////////////////
    // *** Setup Functions *** //
    /////////////////////////////
    public void PlaceCamera(TimelineViewModel timeline)
    {
        foreach (CommandPointer cmd in timeline.Categories[0].Commands)
            if (cmd.Code == "CSD_")
            {
                this.sceneManager.PlaceCamera(cmd.CommandData.ViewportCoordinates, cmd.CommandData.ViewportRotation, cmd.CommandData.AngleOfView);
                break;
            }
    }

    public void AddModel(AssetViewModel asset, TimelineViewModel timeline)
    {
        int objectID = (int)asset.ObjectID.Value;
        Console.WriteLine(asset.ObjectID.Value);
        Console.WriteLine(asset.ObjectType.Choice);
        Console.WriteLine(asset.ActiveBaseAnimPath);
        Console.WriteLine(asset.ActiveExtBaseAnimPath);
        Console.WriteLine(asset.ActiveAddAnimPath);
        Console.WriteLine(asset.ActiveExtAddAnimPath);

        if (String.IsNullOrEmpty(asset.ActiveModelPath))
        {
            Trace.TraceWarning($"Asset with ID {objectID} could not be loaded. It seems not to have a valid existing path.");
            return;
        }

        this.sceneManager.LoadObject(objectID, asset.ActiveModelPath);
        if (!String.IsNullOrEmpty(asset.ActiveBaseAnimPath))
            this.sceneManager.sceneModels[objectID].BaseAnimationPack = this.sceneManager.sceneModels[objectID].TryLoadAnimationPack(asset.ActiveBaseAnimPath);
        if (!String.IsNullOrEmpty(asset.ActiveExtBaseAnimPath))
            this.sceneManager.sceneModels[objectID].ExtBaseAnimationPack = this.sceneManager.sceneModels[objectID].TryLoadAnimationPack(asset.ActiveExtBaseAnimPath);
        if (!String.IsNullOrEmpty(asset.ActiveAddAnimPath))
            this.sceneManager.sceneModels[objectID].AddAnimationPack = this.sceneManager.sceneModels[objectID].TryLoadAnimationPack(asset.ActiveAddAnimPath);
        if (!String.IsNullOrEmpty(asset.ActiveExtAddAnimPath))
            this.sceneManager.sceneModels[objectID].ExtAddAnimationPack = this.sceneManager.sceneModels[objectID].TryLoadAnimationPack(asset.ActiveExtAddAnimPath);

        foreach (CommandPointer cmd in timeline.Categories[2].Commands)
            if (cmd.Command.ObjectId == objectID && cmd.Code == "MSD_")
            {
                this.sceneManager.sceneModels[objectID].SetPosition(cmd.CommandData.Position, cmd.CommandData.Rotation);
                if (!cmd.CommandData.Flags[0])
                    this.sceneManager.sceneModels[objectID].LoadBaseAnimation(cmd.CommandData.Flags[2], (int)cmd.CommandData.WaitingAnimation.Index);
                break;
            }
            /*else if (cmd.Command.ObjectId == objectID && cmd.Code == "MMD_")
            {
                //this.sceneManager.sceneModels[objectID].SetPosition(cmd.CommandData.Targets[(int)cmd.CommandData.NumControlGroups - 1], null);
                this.sceneManager.sceneModels[objectID].SetPosition(new float[] { cmd.CommandData.Targets[(int)cmd.CommandData.NumControlGroups - 1, 0], cmd.CommandData.Targets[(int)cmd.CommandData.NumControlGroups - 1, 1], cmd.CommandData.Targets[(int)cmd.CommandData.NumControlGroups - 1, 2] }, null);
            }
            else if (cmd.Command.ObjectId == objectID && cmd.Code == "MRot")
            {
                this.sceneManager.sceneModels[objectID].SetPosition(null, cmd.CommandData.Rotation);
            }*/
    }

}
