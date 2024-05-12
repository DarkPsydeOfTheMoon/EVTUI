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


public class SceneModel
{
    protected GLModel model;
    protected AnimationPack GAP;
    protected Stopwatch animationStopwatch = new Stopwatch();  // I think we need one per currently-playing animation...

    public SceneModel(GLModel model, AnimationPack GAP)
    {
        this.model = model;
        this.GAP = GAP;
        // If we need to loads GAPS on a per-model basis, we can put
        // the external GAPs here instead of having a global list.
    }

    public void Dispose()
    {
        this.model.Dispose();
    }

    public void StartAnimTimer() { this.animationStopwatch.Start(); }

    public void StopAnimTimer() { this.animationStopwatch.Stop(); }

    public void ResetAnimTimer() { this.animationStopwatch.Restart(); }

    public void LoadAnimation(Animation animation) { this.model.LoadAnimation(animation); }

    public void UnloadAnimation() { this.model.UnloadAnimation(); }

    public void Draw(GLShaderProgram shaderProgram, GLCamera camera, double animationTime)
    {
        bool isAnimActive = this.model.Animation is null || this.model.Animation.Duration < 1;
        double useAnimationTime = isAnimActive? 0 : (animationTime % this.model.Animation.Duration);
        
        this.model.Draw(shaderProgram, camera, useAnimationTime);
    }

    public void Draw(GLShaderProgram shaderProgram, GLCamera camera)
    {
        this.Draw(shaderProgram, camera, (float)(this.animationStopwatch.ElapsedMilliseconds)/1000.0f);
    }
}


public class SceneManager
{
    public List<SceneModel>      sceneModels  = [];
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
        for (int i=this.sceneModels.Count-1; i>=0; --i)
            this.UnloadModel(i);
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
    public void LoadModel(string filepath)
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

        // // This should work with field models too...
        // // But need to find a value for mIsFieldModel and mFieldTextures first.
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

        sceneModels.Add(new SceneModel(glmodel, model.AnimationPack));
    }

    public void UnloadModel(int index)
    {
        if (index >= this.sceneModels.Count)
        {
            Trace.TraceWarning($"Cannot unload model at index {index} because there are only {this.sceneModels.Count} models loaded");
            return;
        }
        this.sceneModels[index].Dispose();
        this.sceneModels.RemoveAt(index);
    }

    ///////////////////////////////////
    // *** GAP Memory Management *** //
    ///////////////////////////////////
    public void LoadGAP(string filepath)
    {
        if (!File.Exists(filepath))
            throw new FileNotFoundException($"GAP file '{filepath}' does not exist.");
        var anims = GFDLibrary.Api.FlatApi.LoadModel(filepath);
        this.externalGAPs.Add(anims.AnimationPack);
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
        this.sceneModels[model_index].LoadAnimation(animation);
    }

    public void ActivateBlendAnimationOnModel(int model_index, int gap_index, int animation_index)
    {
        var animation = this.externalGAPs[gap_index].BlendAnimations[animation_index];
        this.sceneModels[model_index].LoadAnimation(animation);
    }

    public void DeactivateModelAnimations(int model_index)
    {
        this.sceneModels[model_index].UnloadAnimation();
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