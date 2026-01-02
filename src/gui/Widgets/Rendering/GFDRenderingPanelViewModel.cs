using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GFDLibrary.Rendering.OpenGL;
using OpenTK.Graphics.OpenGL;

using ReactiveUI;

namespace EVTUI.ViewModels;

public class GFDRenderingPanelViewModel : ViewModelBase
{
    private List<IDisposable> subscriptions;
    protected DataManager Config;

    private ShaderRegistry mShaderRegistry;

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

    public SceneManager sceneManager { get; set; }

    private bool _readyToRender = false;
    public bool ReadyToRender
    {
        get => _readyToRender;
        set => this.RaiseAndSetIfChanged(ref _readyToRender, value);
    }
    
    public GFDRenderingPanelViewModel(DataManager config, float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 0.0f)
    {
        this.subscriptions = new List<IDisposable>();

        this.Config = config;
        this.sceneManager = new SceneManager(this.Config);

        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;

        mShaderRegistry = new ShaderRegistry();
        this.subscriptions.Add(this.WhenAnyValue(x => x.ReadyToRender).Subscribe(x => 
        {
            if (x)
            {
                // TODO: get the shaders from dniwe working!
                string vsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GFDStudio", "app_data", "shaders", "default.glsl.vs");
                //string vsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TOON.VS.18.glsl.vs");
                string fsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GFDStudio", "app_data", "shaders", "default.glsl.fs");
                //string fsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TOON.PS.6.glsl.fs");
                this.glShaderProgram = this.sceneManager.LoadShader(vsPath, fsPath);
                mShaderRegistry.mDefaultShader = this.glShaderProgram;
                //mShaderRegistry.mDefaultShader.Use();
            }
        }));
    }

    public void Dispose()
    {
        foreach (IDisposable subscription in this.subscriptions)
            subscription.Dispose();
        this.subscriptions.Clear();
        this.sceneManager.Dispose();
        this.sceneManager = null;
        mShaderRegistry = null;
        this.glShaderProgram = null;
        this.Config = null;
    }

    ///////////////////////////////
    // *** Drawing Functions *** //
    ///////////////////////////////
    public void RefreshSceneState()
    {
        GL.ClearColor(this.r, this.g, this.b, this.a);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Clear(ClearBufferMask.AccumBufferBit);
        GL.Enable(EnableCap.DepthTest);
        GL.Viewport(0, 0, (int)this.width, (int)this.height);
        this.sceneManager.activeCamera.AspectRatio = (float)(this.width/this.height);
    }

    public void DrawScene()
    {
        // Each model can have its own shaderprogram if we go that route,
        // meaning that we won't need to pass in a global shader here.
        foreach (var scenemodel in this.sceneManager.sceneModels.Values)
            scenemodel.Draw(mShaderRegistry, this.sceneManager.activeCamera);
        foreach (int objectID in this.sceneManager.fieldModels.Keys)
            foreach (int subID in this.sceneManager.fieldModels[objectID].Keys)
                this.sceneManager.fieldModels[objectID][subID].Draw(mShaderRegistry, this.sceneManager.activeCamera);
    }

    /////////////////////////////
    // *** Setup Functions *** //
    /////////////////////////////
    public void PlaceCamera(TimelineViewModel timeline)
    {
        float[] position = new float[] {0f, 0f, 0f};
        float[] rotation = new float[] {0f, 0f, 0f};
        float angleOfView = 45f;
        float nearClip = 1f;
        float farClip = 60000f;
        foreach (CommandPointer cmd in timeline.Categories[0].Commands)
            if (cmd.Code == "CSD_")
            {
                position = cmd.CommandData.ViewportCoordinates;
                rotation = cmd.CommandData.ViewportRotation;
                angleOfView = cmd.CommandData.AngleOfView;
                break;
            }
        foreach (CommandPointer cmd in timeline.Categories[0].Commands)
            if (cmd.Code == "CClp")
            {
                nearClip = cmd.CommandData.NearClip;
                farClip = cmd.CommandData.FarClip;
                break;
            }
        this.sceneManager.PlaceCamera(position, rotation, angleOfView, nearClip, farClip);
        //mShaderRegistry.mDefaultShader.SetUniform( "uView", this.sceneManager.activeCamera.View );
        //mShaderRegistry.mDefaultShader.SetUniform( "uProjection", this.sceneManager.activeCamera.Projection );
    }

    public void AddTextures(Dictionary<int, string> texturePaths)
    {
        Parallel.ForEach(texturePaths.Keys, subId =>
        {
            this.sceneManager.LoadTextures(texturePaths[subId]);
        });
    }

    //public void AddModel(AssetViewModel asset, TimelineViewModel timeline)
    public void AddModel(AssetViewModel asset)
    {
        int objectID = (int)asset.ObjectID.Value;

        if (asset.ObjectType.Choice == "Field")
        {
            lock (this.sceneManager.fieldModels) { this.sceneManager.LoadField(objectID, asset.ActiveSubModelPaths, asset.ActiveAttachmentPaths, asset.ActiveModels); }
            if (!(asset.ActiveMap is null))
                //for (int subId=0; subId<asset.ActiveMap.Entries.Length; subId++)
                Parallel.For(0, asset.ActiveMap.Entries.Length, subId =>
                {
                    float[] position = new float[] { (float)(400*(asset.ActiveMap.Spacing+1)*(asset.ActiveMap.Entries[subId].X - 60)), (float)(300*(asset.ActiveMap.Spacing+1)*(asset.ActiveMap.Entries[subId].Y)), (float)(400*(asset.ActiveMap.Spacing+1)*(asset.ActiveMap.Entries[subId].Z - 60)) };
                    float[] rotation = new float[] { 0f, (float)(asset.ActiveMap.Entries[subId].Direction*90), 0f };
                    this.sceneManager.fieldModels[objectID][subId].SetPosition(position, rotation);
                });
        }
        else
        {
            if (String.IsNullOrEmpty(asset.ActiveModelPath))
            {
                Trace.TraceWarning($"Asset with ID {objectID} could not be loaded. It seems not to have a valid existing path.");
                return;
            }

            lock (this.sceneManager.sceneModels) { this.sceneManager.LoadObject(objectID, asset.ActiveModels[asset.ActiveModelPath], false); }
        }

        if (!String.IsNullOrEmpty(asset.ActiveBaseAnimPath))
            this.sceneManager.sceneModels[objectID].BaseAnimationPack = asset.ActiveModels[asset.ActiveBaseAnimPath].AnimationPack;
        if (!String.IsNullOrEmpty(asset.ActiveExtBaseAnimPath))
            this.sceneManager.sceneModels[objectID].ExtBaseAnimationPack = asset.ActiveModels[asset.ActiveExtBaseAnimPath].AnimationPack;
        if (!String.IsNullOrEmpty(asset.ActiveAddAnimPath))
            this.sceneManager.sceneModels[objectID].AddAnimationPack = asset.ActiveModels[asset.ActiveAddAnimPath].AnimationPack;
        if (!String.IsNullOrEmpty(asset.ActiveExtAddAnimPath))
            this.sceneManager.sceneModels[objectID].ExtAddAnimationPack = asset.ActiveModels[asset.ActiveExtAddAnimPath].AnimationPack;
    }

    public void PositionModel(AssetViewModel asset, TimelineViewModel timeline)
    {
        int objectID = (int)asset.ObjectID.Value;

        if (String.IsNullOrEmpty(asset.ActiveModelPath))
        {
            Trace.TraceWarning($"Asset with ID {objectID} could not be loaded. It seems not to have a valid existing path.");
            return;
        }

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
