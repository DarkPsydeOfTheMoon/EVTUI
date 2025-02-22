using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;

using GFDLibrary.Animations;
using GFDLibrary.Common;
using GFDLibrary.Rendering.OpenGL;
using GFDLibrary.Textures;

using OpenTK.Graphics.OpenGL;

namespace EVTUI;

public class SceneModel
{
    protected GLModel model;
    protected AnimationPack GAP;
    protected Stopwatch animationStopwatch = new Stopwatch();  // I think we need one per currently-playing animation...

    protected AnimationPack BaseAnimationPack;
    protected AnimationPack ExtBaseAnimationPack;
    protected AnimationPack AddAnimationPack;
    protected AnimationPack ExtAddAnimationPack;

    protected (bool IsExt, int Idx)? BaseAnimInfo;
    protected (bool IsExt, int Idx)?[] AddAnimInfo = new (bool IsExt, int Idx)?[8];

    public SceneModel(GLModel model, AnimationPack GAP)
    {
        this.model = model;
        this.GAP = GAP;
        // If we need to loads GAPS on a per-model basis, we can put
        // the external GAPs here instead of having a global list.
    }

    public SceneModel(DataManager config, SerialObject obj)
    {
        List<string> assetPaths = config.EventManager.GetAssetPaths(obj.Id, config.CpkList, config.VanillaExtractionPath);
        if (assetPaths.Count > 0)
        {
            this.LoadModel(assetPaths[0]);

            List<string> baseAnimPaths = config.EventManager.GetAnimPaths(obj.Id, true, false, config.CpkList, config.VanillaExtractionPath);
            if (baseAnimPaths.Count > 0)
                this.BaseAnimationPack = this.TryLoadAnimationPack(baseAnimPaths[0]);

            List<string> extBaseAnimPaths = config.EventManager.GetAnimPaths(obj.Id, false, false, config.CpkList, config.VanillaExtractionPath);
            if (extBaseAnimPaths.Count > 0)
                this.ExtBaseAnimationPack = this.TryLoadAnimationPack(extBaseAnimPaths[0]);

            List<string> addAnimPaths = config.EventManager.GetAnimPaths(obj.Id, true, true, config.CpkList, config.VanillaExtractionPath);
            if (addAnimPaths.Count > 0)
                this.AddAnimationPack = this.TryLoadAnimationPack(addAnimPaths[0]);

            List<string> extAddAnimPaths = config.EventManager.GetAnimPaths(obj.Id, false, true, config.CpkList, config.VanillaExtractionPath);
            if (extAddAnimPaths.Count > 0)
                this.ExtAddAnimationPack = this.TryLoadAnimationPack(extAddAnimPaths[0]);
        }
    }

    public void Dispose()
    {
        // can happen if EVT object isn't set up properly
        if (!(this.model is null))
            this.model.Dispose();
    }

    public void StartAnimTimer() { this.animationStopwatch.Start(); }

    public void StopAnimTimer() { this.animationStopwatch.Stop(); }

    public void ResetAnimTimer() { this.animationStopwatch.Restart(); }

    public void LoadAnimation(Animation animation)
    {
        // can maybe get rid of this check once asset management is more robust
        if (!(this.model is null))
            this.model.LoadAnimation(animation);
    }

    public void UnloadAnimation()
    {
        // can maybe get rid of this check once asset management is more robust
        if (!(this.model is null))
            this.model.UnloadAnimation();
    }

    public void Draw(GLShaderProgram shaderProgram, GLCamera camera, double animationTime)
    {
        // can happen if EVT object isn't set up properly
        if (this.model is null)
            return;

        bool isAnimActive = !(this.model.Animation is null || this.model.Animation.Duration < 1);
        double useAnimationTime = isAnimActive ? (animationTime % this.model.Animation.Duration) : 0;
        
        this.model.Draw(shaderProgram, camera, useAnimationTime);
    }

    public void Draw(GLShaderProgram shaderProgram, GLCamera camera)
    {
        this.Draw(shaderProgram, camera, (float)(this.animationStopwatch.ElapsedMilliseconds)/1000.0f);
    }

    private void LoadModel(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"Model file '{filepath}' does not exist.");
        
        var model = GFDLibrary.Api.FlatApi.LoadModel(filepath);
        var glmodel = new GLModel(model, ( material, textureName ) =>
        {
            if ( model.Textures.TryGetTexture( textureName, out var texture ) )
            {
                return new GLTexture( texture );
            }
            else
            {
                Trace.TraceWarning( $"tTexture '{textureName}' used by material '{material.Name}' is missing" );
                return new GLTexture(Texture.CreateDefaultTexture(textureName));
            }
        } );

        ////////////////////////////
        // TODO:
        ////////////////////////////
        // The below has been nicked from GFD Studio, which should correctly handle external texture bins.
        // In order to implement this, we need to integrate the `mIsFieldModel` and `mFieldTextures`
        // variables somehow into the scene management workflow.
        // (This should be possible by relying on object/asset types to tell you whether something is a field model.)
        // var glmodel = new GLModel(model, ( material, textureName ) =>
        // {
        //     if ( mIsFieldModel && mFieldTextures.TryOpenFile( textureName, out var textureStream ) )
        //     {
        //         using ( textureStream )
        //         {
        //             var texture = new FieldTexturePS3( textureStream );
        //             return new GLTexture( texture );
        //         }
        //     }
        //     else if ( model.Textures.TryGetTexture( textureName, out var texture ) )
        //     {
        //         return new GLTexture( texture );
        //     }
        //     else
        //     {
        //         Trace.TraceWarning( $"tTexture '{textureName}' used by material '{material.Name}' is missing" );
        //     }

        //     return null;
        // } );
        ////////////////////////////

        this.model = glmodel;
        this.GAP = model.AnimationPack;
    }

    public AnimationPack TryLoadAnimationPack(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"GAP file '{filepath}' does not exist.");
        return GFDLibrary.Api.FlatApi.LoadModel(filepath).AnimationPack;
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
                this.LoadAnimation(this.ExtAddAnimationPack.BlendAnimations[idx]);
                this.StartAnimTimer();
            }
            else if (!isExt && !(this.AddAnimationPack is null) && idx < this.AddAnimationPack.BlendAnimations.Count)
            {
                this.LoadAnimation(this.AddAnimationPack.BlendAnimations[idx]);
                this.StartAnimTimer();
            }
            else
            {
                this.StopAnimTimer();
                this.UnloadAnimation();
            }
        }
    }
}


public class SceneManager
{
    public Dictionary<int, SceneModel> sceneModels = new Dictionary<int, SceneModel>();
    public List<AnimationPack>   externalGAPs = [];
    List<GLPerspectiveCamera>    cameras      = [];
    public List<GLShaderProgram> shaders      = [];

    GLPerspectiveCamera fallbackCamera = new GLPerspectiveCamera(
        0.01f, 1000.0f, (float)45.0f, 4.0f/3.0f, 
        new BoundingSphere(new Vector3(0, 0, 0), 100), 
        new OpenTK.Mathematics.Vector3(0, -80, -100), 
        new OpenTK.Mathematics.Vector3(0, 0, 0)
    );

    public GLPerspectiveCamera activeCamera;

    public SceneManager()
    {
        this.activeCamera = fallbackCamera;
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
                Console.WriteLine("OpenGLError: Invalid Enum");
                break;
            default:
                Console.WriteLine("OpenGLError Code: 0x" + String.Format("{0:X}", errorcode));
                break;
        }
    }

    ///////////////////////////////
    // *** Cleanup Functions *** //
    ///////////////////////////////
    public void teardown()
    {
        foreach (int objectID in this.sceneModels.Keys)
            this.sceneModels[objectID].Dispose();
        this.sceneModels.Clear();

        for (int i=this.externalGAPs.Count-1; i>=0; --i)
            this.UnloadGAP(i);
        this.externalGAPs.Clear();

        for (int i=this.shaders.Count-1; i>=0; --i)
            this.UnloadShader(i);
        this.shaders.Clear();
    }

    /////////////////////////////////////
    // *** Model Memory Management *** //
    /////////////////////////////////////
    public void LoadObjects(DataManager config, int[] objectIDs)
    {
        ////////////////////////////
        // TODO: Implement external texture loading.
        //       In the function below is a code snippet that should work (it's been nicked from
        //       GFD Studio), but the texture-bin management systems need to be worked into the
        //       SceneManager before it can be integrated.
        //       (IMO, we should handle the bins in here and pass them to the model initializers.)
        ////////////////////////////
        foreach (int objectID in objectIDs)
            this.sceneModels[objectID] = new SceneModel(config, config.EventManager.ObjectsById[objectID]);
    }

    ///////////////////////////////////
    // *** GAP Memory Management *** //
    ///////////////////////////////////
    //public void LoadGAP(string filepath)
    public int LoadGAP(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"GAP file '{filepath}' does not exist.");
        var anims = GFDLibrary.Api.FlatApi.LoadModel(filepath);
        this.externalGAPs.Add(anims.AnimationPack);
        return this.externalGAPs.Count-1;
    }

    public void UnloadGAP(int index)
    {
        if (index >= this.externalGAPs.Count)
        {
            Trace.TraceWarning($"Cannot unload GAP at index {index} because there are only {this.externalGAPs.Count} GAPs loaded");
            return;
        }
        this.externalGAPs.RemoveAt(index);
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
    public void ActivateAnimationOnModel(int model_index, int gap_index, int animation_index)
    {
        var animation = this.externalGAPs[gap_index].Animations[animation_index];
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].LoadAnimation(animation);
        else
            Trace.TraceWarning($"Tried to load animation for asset #{model_index}, which hasn't been loaded.");
    }

    public void ActivateBlendAnimationOnModel(int model_index, int gap_index, int animation_index)
    {
        var animation = this.externalGAPs[gap_index].BlendAnimations[animation_index];
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].LoadAnimation(animation);
        else
            Trace.TraceWarning($"Tried to load animation for asset #{model_index}, which hasn't been loaded.");
    }

    public void DeactivateModelAnimations(int model_index)
    {
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].UnloadAnimation();
        else
            Trace.TraceWarning($"Tried to unload animations for asset #{model_index}, which hasn't been loaded.");
    }

    public void LoadBaseAnimation(int model_index, bool isExt, int idx)
    {
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].LoadBaseAnimation(isExt, idx);
        else
            Trace.TraceWarning($"Tried to load animation for asset #{model_index}, which hasn't been loaded.");
    }

    public void LoadAddAnimationTrack(int model_index, bool isExt, int idx, int track)
    {
        if (this.sceneModels.ContainsKey(model_index))
            this.sceneModels[model_index].LoadAddAnimationTrack(isExt, idx, track);
        else
            Trace.TraceWarning($"Tried to load animation for asset #{model_index}, which hasn't been loaded.");
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
}
