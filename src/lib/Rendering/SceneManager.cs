using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

using GFDLibrary;

using GFDLibrary.Animations;
using GFDLibrary.Common;
using GFDLibrary.IO.Common;
using GFDLibrary.Materials;
using GFDLibrary.Rendering.OpenGL;
using GFDLibrary.Textures;

using GFDLibrary.Models;
using GFDLibrary.Utilities;

using OpenTK.Graphics.OpenGL;

namespace EVTUI;

public class SceneModel
{
    protected DataManager Config;
    protected GLModel model;
    protected Stopwatch animationStopwatch = new Stopwatch();  // I think we need one per currently-playing animation...

    public AnimationPack BaseAnimationPack;
    public AnimationPack ExtBaseAnimationPack;
    public AnimationPack AddAnimationPack;
    public AnimationPack ExtAddAnimationPack;

    protected (bool IsExt, int Idx)? BaseAnimInfo;
    protected (bool IsExt, int Idx)?[] AddAnimInfo = new (bool IsExt, int Idx)?[8];

    protected Dictionary<int, GLNode> NodesByResId;
    protected Dictionary<int, SceneModel> AttachedModels;

    public float[] BasePosition = new float[3];
    public float[] BaseRotation = new float[3];

    public SceneModel(DataManager config, ModelPack model, Dictionary<string, Texture> fieldtex, bool isField=false, GLNode parent=null)
    {
        if (isField)
        {
            this.NodesByResId = new Dictionary<int, GLNode>();
            this.AttachedModels = new Dictionary<int, SceneModel>();
        }

        this.Config = config;
        this.LoadModel(model, fieldtex, isField);

        if (!(parent is null))
            this.model.Nodes[0].Parent = parent;
    }

    public void Dispose()
    {
        if (!(this.model is null))
        {
            this.model.Dispose();
            if (!(this.AttachedModels is null))
            {
                foreach (int resId in this.AttachedModels.Keys)
                    this.AttachedModels[resId].Dispose();
                this.AttachedModels.Clear();
                this.model.AttachedModels.Clear();
                this.NodesByResId.Clear();
            }
        }
        this.animationStopwatch.Stop();
        this.animationStopwatch = null;
        this.BaseAnimationPack = null;
        this.ExtBaseAnimationPack = null;
        this.AddAnimationPack = null;
        this.ExtAddAnimationPack = null;
        this.Config = null;
    }

    public void StartAnimTimer() { this.animationStopwatch.Start(); }
    public void StopAnimTimer() { this.animationStopwatch.Stop(); }
    public void ResetAnimTimer() { this.animationStopwatch.Restart(); }

    public void LoadAnimation(Animation animation)
    {
        if (!(this.model is null))
            this.model.LoadAnimation(animation);
    }

    public void UnloadAnimation()
    {
        if (!(this.model is null))
            this.model.UnloadAnimation();
    }

    private void LoadBlendAnimation(Animation animation, int index=-1)
    {
        if (!(this.model is null))
            this.model.LoadBlendAnimation(animation, index);
    }

    private void UnloadBlendAnimation(int index)
    {
        if (!(this.model is null))
            this.model.UnloadBlendAnimation(index);
    }

    public void Draw(ShaderRegistry mShaderRegistry, GLCamera camera, double animationTime)
    {
        if (this.model is null)
            return;

        // There needs to be some kind of rewrite to support looping for multiple anim tracks independently.
        bool isAnimActive = false;
        double duration   = 0;
        if (this.model.Animation is not null)
        {
            isAnimActive = this.model.Animation.Duration >= 1;
            duration = this.model.Animation.Duration;
        }
        foreach (var (index, blendAnimation) in this.model.BlendAnimations)
        {
            isAnimActive |= blendAnimation.Duration >= 1;
            duration = (blendAnimation.Duration > duration) ? blendAnimation.Duration : duration;
        }

        double useAnimationTime = isAnimActive ? (animationTime % duration) : 0;
        this.model.Draw( new DrawContext()
        {
            ShaderRegistry = mShaderRegistry,
            Camera = camera,
            AnimationTime = useAnimationTime,
        } );
    }

    public void Draw(ShaderRegistry mShaderRegistry, GLCamera camera)
    {
        this.Draw(mShaderRegistry, camera, (float)(this.animationStopwatch.ElapsedMilliseconds)/1000.0f);
    }

    private void LoadModel(ModelPack model, Dictionary<string, Texture> fieldtex, bool isField=false)
    {
        this.model = new GLModel(model, ( material, textureName ) =>
        {
            if (fieldtex.ContainsKey(textureName.ToLower()))
                return new GLTexture(fieldtex[textureName.ToLower()]);
            else if ( model.Textures.TryGetTexture( textureName, out var texture ) )
            {
                return new GLTexture( texture );
            }
            else
            {
                Trace.TraceWarning( $"tTexture '{textureName}' used by material '{material.Name}' is missing" );
                return new GLTexture(Texture.CreateDefaultTexture(textureName));
            }
        } );

        if (isField)
        {
            this.NodesByResId = new Dictionary<int, GLNode>();
            foreach (GLNode node in this.model.Nodes)
                if (node.Node.Properties.ContainsKey("fldLayoutOfModel_resId"))
                    this.NodesByResId[(int)node.Node.Properties["fldLayoutOfModel_resId"].GetValue()] = node;
        }
    }

    public void SetPosition(float[] position, float[] rotation)
    {
        Animation pos = new Animation();
        pos.Controllers.Add(new AnimationController());
        pos.Controllers[0].TargetKind = TargetKind.Node;
        pos.Controllers[0].TargetId = 0;
        pos.Controllers[0].TargetName = this.model.Nodes[0].Node.Name;
        pos.Controllers[0].Layers.Add(new AnimationLayer());
        pos.Controllers[0].Layers[0].KeyType = KeyType.NodePRS;

        PRSKey key = new PRSKey(KeyType.NodePRS);
        // TODO: this should probably be more properly split out if we only want to set position or only want to set rotation
        if (!(position is null))
            key.Position = new Vector3(this.BasePosition[0] + position[0], this.BasePosition[1] + position[1], this.BasePosition[2] + position[2]);
        if (!(rotation is null))
            key.Rotation = GLModel.EulerToQuat(new Vector3(MathHelper.DegreesToRadians(this.BaseRotation[0] + rotation[0]), MathHelper.DegreesToRadians(this.BaseRotation[1] + rotation[1]), MathHelper.DegreesToRadians(this.BaseRotation[2] + rotation[2])));
        pos.Controllers[0].Layers[0].Keys.Add(key);

        this.UnloadBlendAnimation(0);
        this.LoadBlendAnimation(pos, 0);
    }

    public void LoadBaseAnimation(bool isExt, int idx)
    {
        (bool IsExt, int Idx) newInfo = (isExt, idx);
        if (this.BaseAnimInfo != newInfo)
        {
            this.BaseAnimInfo = newInfo;
            if (isExt && !(this.ExtBaseAnimationPack is null) && idx < this.ExtBaseAnimationPack.Animations.Count)
            {
                this.LoadAnimation(this.ExtBaseAnimationPack.Animations[idx]);
                this.StartAnimTimer();
            }
            else if (!isExt && !(this.BaseAnimationPack is null) && idx < this.BaseAnimationPack.Animations.Count)
            {
                this.LoadAnimation(this.BaseAnimationPack.Animations[idx]);
                this.StartAnimTimer();
            }
            else
            {
                this.StopAnimTimer();
                this.UnloadAnimation();
            }
        }
    }

    public void LoadAddAnimationTrack(bool isExt, int idx, int track)
    {
        if (track >= this.AddAnimInfo.Length)
            throw new Exception($"Track index must be 0-7 -- {track} is invalid.");
        (bool IsExt, int Idx) newInfo = (isExt, idx);
        if (this.AddAnimInfo[track] != newInfo)
        {
            this.AddAnimInfo[track] = newInfo;
            if (isExt && !(this.ExtAddAnimationPack is null) && idx < this.ExtAddAnimationPack.BlendAnimations.Count)
            {
                this.LoadBlendAnimation(this.ExtAddAnimationPack.BlendAnimations[idx], track);
                this.StartAnimTimer();
            }
            else if (!isExt && !(this.AddAnimationPack is null) && idx < this.AddAnimationPack.BlendAnimations.Count)
            {
                this.LoadBlendAnimation(this.AddAnimationPack.BlendAnimations[idx], track);
                this.StartAnimTimer();
            }
            else
            {
                this.StopAnimTimer();
                this.UnloadBlendAnimation(track);
            }
        }
    }

    public void AddAttachment(int resId, ModelPack attachedModel, Dictionary<string, Texture> fieldtex)
    {
        if (!(this.NodesByResId.ContainsKey(resId)))
            return;

        this.AttachedModels[resId] = new SceneModel(this.Config, attachedModel, fieldtex, false);
        float[] position = new float[] {this.NodesByResId[resId].Node.Translation.X, this.NodesByResId[resId].Node.Translation.Y, this.NodesByResId[resId].Node.Translation.Z};
        Vector3 rotVec = GLModel.QuatToEuler(this.NodesByResId[resId].Node.Rotation);
        float[] rotation = new float[] {0f, MathHelper.RadiansToDegrees(rotVec.Y), 0f};
        if (!(this.model.Nodes[0].Parent is null))
            this.AttachedModels[resId].model.Nodes[0].Parent = this.model.Nodes[0].Parent;
        this.AttachedModels[resId].SetPosition(position, rotation);
        this.AttachedModels[resId].BaseAnimationPack = attachedModel.AnimationPack;
        this.AttachedModels[resId].AddAnimationPack = attachedModel.AnimationPack;
        if (this.NodesByResId[resId].Node.Properties.ContainsKey("fldLayoutOfModel_animNo"))
            this.AttachedModels[resId].LoadBaseAnimation(false, (int)this.NodesByResId[resId].Node.Properties["fldLayoutOfModel_animNo"].GetValue());
        lock (this.model.AttachedModels) { this.model.AttachedModels[resId] = this.AttachedModels[resId].model; }
    }

    public void ToggleAttachment(int resId, bool onOrOff)
    {
        lock (this.model.AttachedModels)
        {
            if (onOrOff)
                this.model.AttachedModels[resId] = this.AttachedModels[resId].model;
            else
                this.model.AttachedModels.Remove(resId);
        }
    }

    public void AnimateAttachment(int resId, int animId, bool isAddAnim)
    {
        if (isAddAnim)
            this.AttachedModels[resId].LoadAddAnimationTrack(false, animId, 0);
        else
            this.AttachedModels[resId].LoadBaseAnimation(false, animId);
    }
}


public class SceneManager
{
    protected DataManager Config;

    public Dictionary<int, SceneModel> sceneModels;
    public Dictionary<int, Dictionary<int, SceneModel>> fieldModels;
    public Dictionary<int, GLNode> fieldRootNodes;
    public List<GLPerspectiveCamera> cameras;
    public List<GLShaderProgram> shaders;
    public Dictionary<string, Texture> FieldTex;

    GLPerspectiveCamera fallbackCamera = new GLPerspectiveCamera(
        1.0f, 1000.0f, (float)45.0f, 4.0f/3.0f, 
        new BoundingSphere(new Vector3(0, 0, 0), 100), 
        new OpenTK.Mathematics.Vector3(0, -80, -100), 
        new OpenTK.Mathematics.Vector3(0, 0, 0)
    );

    GLPerspectiveCamera closeupCamera = new GLPerspectiveCamera(
        1.0f, 1000.0f, (float)45.0f, 4.0f/3.0f, 
        new BoundingSphere(new Vector3(0, 90, 0), -20), 
        new OpenTK.Mathematics.Vector3(0, -80, -100), 
        new OpenTK.Mathematics.Vector3(0, 0, 0)
    );

    public GLPerspectiveCamera activeCamera;

    public SceneManager(DataManager config)
    {
        this.Config = config;
        this.activeCamera = fallbackCamera;

        this.sceneModels = new Dictionary<int, SceneModel>();
        this.fieldModels = new Dictionary<int, Dictionary<int, SceneModel>>();
        this.fieldRootNodes = new Dictionary<int, GLNode>();
        this.cameras = new List<GLPerspectiveCamera>();
        this.shaders = new List<GLShaderProgram>();
        this.FieldTex = new Dictionary<string, Texture>();
    }

    ////////////////////////////////
    // *** OpenGL Interaction *** //
    ////////////////////////////////
    public static void CheckGLError()
    {
        int err;
        while ((err = (int)GL.GetError()) != 0)
        {
            ReportError(err);
        }
    }

    public static void ReportError(int errorcode)
    {
        switch(errorcode)
        {
            case 0x500:
                Trace.TraceError("OpenGLError: Invalid Enum");
                break;
            default:
                Trace.TraceError("OpenGLError Code: 0x" + String.Format("{0:X}", errorcode));
                break;
        }
    }

    ///////////////////////////////
    // *** Cleanup Functions *** //
    ///////////////////////////////
    public void Dispose()
    {
        this.teardown();
        this.Config = null;
    }

    public void teardown()
    {
        foreach (int objectID in this.sceneModels.Keys)
            this.sceneModels[objectID].Dispose();
        this.sceneModels.Clear();

        foreach (int objectID in this.fieldModels.Keys)
        {
            foreach (int subID in this.fieldModels[objectID].Keys)
                this.fieldModels[objectID][subID].Dispose();
            this.fieldModels[objectID].Clear();
            this.fieldRootNodes[objectID].Dispose();
        }
        this.fieldModels.Clear();
        this.fieldRootNodes.Clear();

        for (int i=this.shaders.Count-1; i>=0; --i)
            this.UnloadShader(i);
        this.shaders.Clear();
    }

    /////////////////////////////////////
    // *** Model Memory Management *** //
    /////////////////////////////////////
    public void LoadTextures(string texturePath)
    {
        if (!String.IsNullOrEmpty(texturePath))
        {
            AtlusArchive bin = new AtlusArchive();
            bin.Read(texturePath);
            foreach (FileEntry entry in bin.Entries)
            {
                string textureName = entry.Name.Replace("\0", "").ToLower();
                lock (this.FieldTex) { this.FieldTex[textureName] = new Texture(textureName, TextureFormat.DDS, entry.Data); }
            }
        }
    }

    public void LoadObject(int objectID, ModelPack model, bool isField=false)
    {
        this.sceneModels[objectID] = new SceneModel(this.Config, model, this.FieldTex, isField);
    }

    public void LoadField(int objectID, Dictionary<int, string> modelPaths, Dictionary<int, Dictionary<int, string>> attachmentPaths, Dictionary<string, ModelPack> models)
    {
        this.fieldModels[objectID] = new Dictionary<int, SceneModel>();
        this.fieldRootNodes[objectID] = new GLNode(new Node("FieldRoot"));
        foreach (int subID in modelPaths.Keys)
        {
            this.fieldModels[objectID][subID] = new SceneModel(this.Config, models[modelPaths[subID]], this.FieldTex, true, parent: this.fieldRootNodes[objectID]);
            foreach (int resId in attachmentPaths[subID].Keys)
                this.fieldModels[objectID][subID].AddAttachment(resId, models[attachmentPaths[subID][resId]], this.FieldTex);
        }
    }

    //////////////////////////////////////
    // *** Shader Memory Management *** //
    //////////////////////////////////////
    public GLShaderProgram LoadShader(string vertexFilepath, string fragmentFilepath)
    {
        GLShaderProgram program;
        if (!GLShaderProgram.TryCreate(vertexFilepath, fragmentFilepath, out program))
        {
            CheckGLError();
            var ecode = GL.GetError();
            throw new Exception("Shader compilation error: " + ecode.ToString());
        }
        this.shaders.Add(program);
        
        return program;
    }

    public void UnloadShader(int index)
    {
        if (index >= this.shaders.Count)
        {
            Trace.TraceWarning($"Cannot unload shader at index {index} because there are only {this.shaders.Count} shaders loaded");
            return;
        }
        this.shaders[index].Dispose();
        this.shaders.RemoveAt(index);
    }

    /////////////////////////////////////
    // *** Model State Management *** //
    ////////////////////////////////////
    public void LoadBaseAnimation(int model_index, bool isExt, int idx)
    {
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].LoadBaseAnimation(isExt, idx);
        else
            Trace.TraceWarning($"Tried to load animation for asset #{model_index}, which hasn't been loaded.");
    }

    public void LoadAddAnimationTrack(int model_index, bool isExt, int idx, int track)
    {
        this.activeCamera = closeupCamera;
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].LoadAddAnimationTrack(isExt, idx, track);
        else
            Trace.TraceWarning($"Tried to load animation for asset #{model_index}, which hasn't been loaded.");
    }

    public void SetFieldPosition(int objectID, float[] position, float[] rotation)
    {
        this.fieldRootNodes[objectID].Node.Translation = new Vector3(position[0], position[1], position[2]);
        this.fieldRootNodes[objectID].Node.Rotation = GLModel.EulerToQuat(new Vector3(MathHelper.DegreesToRadians(rotation[0]), MathHelper.DegreesToRadians(rotation[1]), MathHelper.DegreesToRadians(rotation[2])));
        this.fieldRootNodes[objectID].WorldTransform = this.fieldRootNodes[objectID].Node.WorldTransform;
    }

    /////////////////////////////////////
    // *** Camera State Management *** //
    /////////////////////////////////////
    public void ActivateCamera(int camera_index)
    {
        if (camera_index < 0 || camera_index >= this.cameras.Count)
            this.activeCamera = fallbackCamera;
        else
            this.activeCamera = this.cameras[camera_index];
    }

    public void PlaceCamera(float[] position, float[] rotation, float angleOfView, float nearClip, float farClip)
    {
        this.activeCamera = new GLPerspectiveCamera(
            nearClip, farClip, angleOfView, 16.0f/9.0f,
            // translation 
            new OpenTK.Mathematics.Vector3(0, 0, 0),
            // offset
            new OpenTK.Mathematics.Vector3(-position[0], -position[1], -position[2]), 
            // modelTranslation
            new OpenTK.Mathematics.Vector3(0, 0, 0),
            // modelRotation
            new OpenTK.Mathematics.Vector3(MathHelper.DegreesToRadians(-rotation[1]), MathHelper.DegreesToRadians(-rotation[0]), MathHelper.DegreesToRadians(-rotation[2]))
        );
    }
}
