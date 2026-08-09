using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

using DeepCopy;

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

using static EVTUI.Utils;

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

    protected Dictionary<string, GLNode> NodesByName;
    protected Dictionary<int, GLNode> NodesByHelperId;
    protected Dictionary<int, GLNode> NodesByResId;
    protected Dictionary<int, SceneModel> AttachedModels;

    public float[] BasePosition = new float[3];
    public float[] BaseRotation = new float[3];

    public SceneModel(DataManager config, ModelPack model, Dictionary<string, Texture> fieldtex, bool isField=false, GLNode parent=null)
    {
        if (isField)
        {
            this.NodesByName = new Dictionary<string, GLNode>();
            this.NodesByHelperId = new Dictionary<int, GLNode>();
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
                this.NodesByName.Clear();
                this.NodesByHelperId.Clear();
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

    public void UpdateState()
    {
        if (this.model is null)
            return;

        this.model.UpdateState((float)(this.animationStopwatch.ElapsedMilliseconds)/1000.0f);
    }

    public void Draw(ShaderRegistry mShaderRegistry, GLCamera camera, double animationTime)
    {
        if (this.model is null)
            return;

        this.model.Draw( new DrawContext()
        {
            ShaderRegistry = mShaderRegistry,
            Camera = camera,
            AnimationTime = (this.model.Animation is null && this.model.BlendAnimations.Count == 0) ? 0 : animationTime,
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

        this.NodesByName = new Dictionary<string, GLNode>();
        this.NodesByHelperId = new Dictionary<int, GLNode>();
        this.NodesByResId = new Dictionary<int, GLNode>();
        foreach (GLNode node in this.model.Nodes)
        {
            this.NodesByName[node.Node.Name] = node;
            if (node.Node.Properties.ContainsKey("gfdHelperID"))
                this.NodesByHelperId[(int)node.Node.Properties["gfdHelperID"].GetValue()] = node;
            if (node.Node.Properties.ContainsKey("fldLayoutOfModel_resId"))
                this.NodesByResId[(int)node.Node.Properties["fldLayoutOfModel_resId"].GetValue()] = node;
            else if (node.Node.Properties.ContainsKey("fldLayoutOfModel_major") && node.Node.Properties.ContainsKey("fldLayoutOfModel_minor"))
                this.NodesByResId[0] = node;
        }
    }

    private float[] CurrentPosition = new float[3];
    private float[] CurrentRotation = new float[3];
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
        {
            this.CurrentPosition = new float[] {this.BasePosition[0] + position[0], this.BasePosition[1] + position[1], this.BasePosition[2] + position[2]};
            key.Position = new Vector3(this.CurrentPosition[0], this.CurrentPosition[1], this.CurrentPosition[2]);
        }
        if (!(rotation is null))
        {
            this.CurrentRotation = new float[] { MathHelper.DegreesToRadians(this.BaseRotation[0] + rotation[0]), MathHelper.DegreesToRadians(this.BaseRotation[1] + rotation[1]), MathHelper.DegreesToRadians(this.BaseRotation[2] + rotation[2])};
            key.Rotation = GLModel.EulerToQuat(new Vector3(this.CurrentRotation[0], this.CurrentRotation[1], this.CurrentRotation[2]));
        }
        pos.Controllers[0].Layers[0].Keys.Add(key);

        this.UnloadBlendAnimation(0);
        this.LoadBlendAnimation(pos, 0);
    }

    // TODO: get this actually working correctly, rifp
    public void SetLookAt(float[] target)
    {
        // we need lookat anims and a standard head node. probably...
        if (!this.BaseAnimationPack.Flags.HasFlag(AnimationPackFlags.Bit2) || !this.NodesByName.ContainsKey("Bip01 Head"))
            return;

        //float[] headPos = new float[] { 0f, 0f, 0f };
        //float[] headRot = new float[] { 0f, 0f, 0f };

        // 1. from rotation
        Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].WorldTransform, out var baseScale, out var baseRotation, out var baseTranslation);
        Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].Node.WorldTransform, out var baseBaseScale, out var baseBaseRotation, out var baseBaseTranslation);
        //Vector3 fromRotation = GLModel.QuatToEuler(baseRotation);
        // 2. to rotation
        Vector3 targetTranslation = new Vector3(target[0], target[1], target[2]);
        // 2a. cast ray from head to target
        Vector3 zDir = Vector3.Normalize(targetTranslation - baseTranslation);
        // 2b. create upward vector
        Vector3 up = new Vector3(0f, 1f, 0f);
        // 2c. get transformation??
        Vector3 yDir = Vector3.Normalize(Vector3.Cross(zDir, up));
        Vector3 xDir = Vector3.Cross(yDir, zDir);
        Matrix4x4 transMat = Matrix4x4.Identity;
        transMat.M11 = xDir.X;
        transMat.M12 = xDir.Y;
        transMat.M13 = xDir.Z;
        transMat.M21 = yDir.X;
        transMat.M22 = yDir.Y;
        transMat.M23 = yDir.Z;
        transMat.M31 = zDir.X;
        transMat.M32 = zDir.Y;
        transMat.M33 = zDir.Z;
        // 2d. determine what the fuck we just did lol
        Matrix4x4.Decompose(transMat, out var transScale, out var transRotation, out var transTranslation);
        Quaternion fromRotation = baseRotation * Quaternion.Inverse(baseBaseRotation);
        //Vector3 fromRotation = GLModel.QuatToEuler(baseRotation) - GLModel.QuatToEuler(baseBaseRotation);
        //Vector3 toRotation = GLModel.QuatToEuler(transRotation);
        //Console.WriteLine($"FROM: {fromRotation.X}, {fromRotation.Y}, {fromRotation.Z}");
        //Console.WriteLine($"TO: {toRotation.X}, {toRotation.Y}, {toRotation.Z}");
        var dot = Quaternion.Dot(fromRotation, transRotation);
        Console.WriteLine(dot);
        // 3. slerp
        // 4. profit???

        // 1a. get current head direction
        Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].WorldTransform, out var currentWorldScale, out var currentWorldRotation, out var currentWorldTranslation);
        Vector3 currentHeadWorldRot = GLModel.QuatToEuler(currentWorldRotation);
        // 1b. get base head direction
        Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].Node.WorldTransform, out var baseWorldScale, out var baseWorldRotation, out var baseWorldTranslation);
        Vector3 baseHeadWorldRot = GLModel.QuatToEuler(baseWorldRotation);
        // 1c. get effective head direction (seems like y is reversed in general)
        Vector3 effectiveBaseRot = currentHeadWorldRot - baseHeadWorldRot;
        //float baseAzimuth = (float)NormalizeAngle(effectiveBaseRot.X, radians: true);
        //float baseAltitude = (float)NormalizeAngle(effectiveBaseRot.Y, radians: true);
        Console.WriteLine($"TRUE ROTATION: {NormalizeAngle(effectiveBaseRot.X, radians: true)}, {NormalizeAngle(effectiveBaseRot.Y, radians: true)}, {NormalizeAngle(effectiveBaseRot.Z, radians: true)}");
        float baseAzimuth = effectiveBaseRot.X;
        //if (baseAzimuth >= Math.PI/36f)
        //    baseAzimuth -= (float)Math.PI/36f;
        //else if (baseAzimuth <= -Math.PI/36f)
        //    baseAzimuth += (float)Math.PI/36f;
        float baseAltitude = effectiveBaseRot.Z;
        if (NormalizeAngle(baseAzimuth, radians: true) > 0)
            //baseAltitude *= -1;
            baseAltitude = effectiveBaseRot.Y;
        //if (baseAltitude >= Math.PI/36f)
        //    baseAltitude -= (float)Math.PI/36f;
        //else if (baseAltitude <= -Math.PI/36f)
        //    baseAltitude += (float)Math.PI/36f;
        // 2. get azimuth/altitude of ray connecting head and target (goal direction)
        Vector3 ray = new Vector3(target[0]-currentWorldTranslation.X, target[1]-currentWorldTranslation.Y, target[2]-currentWorldTranslation.Z);
        Vector3 norm = Vector3.Normalize(ray);
        //if (norm.X > 0)
        //    baseAltitude *= -1;
        //float goalAzimuth = (float)Math.Atan2(norm.X, norm.Z);
        //float goalAltitude = (float)-Math.Asin(norm.Y);
        //float goalAzimuth = (float)Math.Atan2(ray.X, ray.Z);
        //float goalAltitude = (float)(Math.Asin(-ray.Y / Math.Sqrt(Math.Pow(ray.X, 2) + Math.Pow(ray.Y, 2) + Math.Pow(ray.Z, 2))));
        //if (ray.Y < 0) goalAltitude *= -1;
        //goalAzimuth -= (float)Math.PI/36f;
        //goalAltitude -= (float)Math.PI/36f;
        float goalAzimuth = (float)VectorToAzimuth(ray);
        float goalAltitude = (float)-VectorToElevation(ray);
        // 2.5. debug printing
        Console.WriteLine($"WORLD ROTATION: {currentHeadWorldRot.X}, {currentHeadWorldRot.Y}, {currentHeadWorldRot.Z}");
        Console.WriteLine($"BASE ROTATION: {baseHeadWorldRot.X}, {baseHeadWorldRot.Y}, {baseHeadWorldRot.Z}");
        Console.WriteLine($"HEAD POSITION: {currentWorldTranslation.X}, {currentWorldTranslation.Y}, {currentWorldTranslation.Z}");
        Console.WriteLine($"TARG POSITION: {target[0]}, {target[1]}, {target[2]}");
        Console.WriteLine($"NORM DIRECTION: {norm.X}, {norm.Y}, {norm.Z}");
        Console.WriteLine($"GOAL DIRECTION: {goalAzimuth}, {goalAltitude}");
        Console.WriteLine($"BASE DIRECTION: {baseAzimuth}, {baseAltitude}");
        //Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].CurrentTransform, out var currentLocalScale, out var currentLocalRotation, out var currentLocalTranslation);
        //Console.WriteLine($"LOCAL DIRECTION: {currentLocalRotation.X}, {currentLocalRotation.Y}, {currentLocalRotation.Z}");
        // 3. determine delta between effective direction and goal direction
        float azimuthDiff = 0f;
        float altitudeDiff = 0f;
        //if (Math.Sqrt(Math.Pow(ray.X, 2) + Math.Pow(ray.Y, 2) + Math.Pow(ray.Z, 2)) > 10)
        //{
            azimuthDiff = (float)NormalizeAngle(goalAzimuth - baseAzimuth, radians: true);
            altitudeDiff = (float)NormalizeAngle(goalAltitude - baseAltitude, radians: true);
            //altitudeDiff = (float)NormalizeAngle(goalAltitude, radians: true);
        //}
        Console.WriteLine($"DELT DIRECTION: {azimuthDiff}, {altitudeDiff}");
        // 4. determine left/right lookat based on which, if either, gets closer to goal
        // left
        if (azimuthDiff > 0)
            this.LoadBlendAnimation(this.InterpolatedAnim(this.BaseAnimationPack.Bit29Data.Field04, this.BaseAnimationPack.Bit29Data.Field14, azimuthDiff), 2);
        // right
        else if (azimuthDiff < 0)
            this.LoadBlendAnimation(this.InterpolatedAnim(this.BaseAnimationPack.Bit29Data.Field00, -this.BaseAnimationPack.Bit29Data.Field10, azimuthDiff), 2);
        // 5. ditto up/down rotation
        this.UpdateState();
        // down
        if (altitudeDiff > 0)
            this.LoadBlendAnimation(this.InterpolatedAnim(this.BaseAnimationPack.Bit29Data.Field0C, this.BaseAnimationPack.Bit29Data.Field1C, altitudeDiff), 3);
        // up
        else if (altitudeDiff < 0)
            this.LoadBlendAnimation(this.InterpolatedAnim(this.BaseAnimationPack.Bit29Data.Field08, -this.BaseAnimationPack.Bit29Data.Field18, altitudeDiff), 3);

        // right
        //this.LoadBlendAnimation(this.BaseAnimationPack.Bit29Data.Field00, 1);
        // left
        //this.LoadBlendAnimation(this.BaseAnimationPack.Bit29Data.Field04, 1);
        // up
        //this.LoadBlendAnimation(this.BaseAnimationPack.Bit29Data.Field08, 1);
        // down
        //this.LoadBlendAnimation(this.BaseAnimationPack.Bit29Data.Field0C, 1);
    }

    // keeping prints commented because i still need to fully debug this
    private Animation InterpolatedAnim(Animation baseAnim, float denominator, float numerator)
    {
        float blend = numerator / denominator;
        Console.WriteLine($"BLEND: {blend} ({numerator} / {denominator})");

        Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].CurrentTransform, out var currentScale, out var currentRotation, out var currentTranslation);
        //Matrix4x4.Decompose(this.NodesByName["Bip01 Head"].WorldTransform, out var currentScale, out var currentRotation, out var currentTranslation);
        Vector3 currentHeadRot = GLModel.QuatToEuler(currentRotation);
        Console.WriteLine($"LOCAL ROTATION: {currentHeadRot.X} {currentHeadRot.Y} {currentHeadRot.Z}");

        Animation interpAnim = DeepCopier.Copy(baseAnim);
        if (blend < 1f)
            foreach (AnimationController controller in interpAnim.Controllers)
            {
                //if (controller.TargetName == "Bip01 Head")
                //    Console.WriteLine($"{controller.TargetKind}: {controller.TargetName} ({controller.TargetId})");
                foreach (AnimationLayer layer in controller.Layers)
                {
                    //Console.WriteLine($"\t{layer.KeyType}");
                    if (layer.HasPRSKeyFrames)
                    {
                        foreach (Key key in layer.Keys)
                        {
                            PRSKey prsKey = (PRSKey)key;
                            /*if (controller.TargetName == "Bip01 Head")
                            {
                                Vector3 rotVec = GLModel.QuatToEuler(prsKey.Rotation);
                                Console.WriteLine($"{rotVec.X}, {rotVec.Y}, {rotVec.Z} -- {key.Time}");
                            }*/
                            if (prsKey.HasRotation)
                                prsKey.Rotation = Quaternion.Slerp(Quaternion.Identity, prsKey.Rotation, blend);
                            if (prsKey.HasPosition)
                                prsKey.Position = Vector3.Lerp(Vector3.Zero * layer.PositionScale, prsKey.Position * layer.PositionScale, blend);
                            if (prsKey.HasScale)
                                prsKey.Scale = Vector3.Lerp(Vector3.Zero * layer.ScaleScale, prsKey.Scale * layer.ScaleScale, blend);
                            /*if (controller.TargetName == "Bip01 Head")
                            {
                                Vector3 rotVec = GLModel.QuatToEuler(prsKey.Rotation);
                                Console.WriteLine($"{rotVec.X}, {rotVec.Y}, {rotVec.Z} -- {key.Time}");
                            }*/
                        }
                    }
                }
            }
        return interpAnim;
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
            if (this.model.AttachedModels.ContainsKey(resId))
            {
                if (onOrOff)
                    this.model.AttachedModels[resId] = this.AttachedModels[resId].model;
                else
                    this.model.AttachedModels.Remove(resId);
            }
            else
                Trace.TraceWarning($"No attachment with res ID {resId} exists for this model");
        }
    }

    public void AnimateAttachment(int resId, int animId, bool isAddAnim)
    {
        if (this.model.AttachedModels.ContainsKey(resId))
        {
            if (isAddAnim)
                this.AttachedModels[resId].LoadAddAnimationTrack(false, animId, 0);
            else
                this.AttachedModels[resId].LoadBaseAnimation(false, animId);
        }
        else
            Trace.TraceWarning($"No attachment with res ID {resId} exists for this model");
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
