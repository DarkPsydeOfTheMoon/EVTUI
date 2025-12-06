using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Media;

using ReactiveUI;

namespace EVTUI.ViewModels;

public class FieldBase : ViewModelBase
{
    public FieldBase(string name, bool editable, string info = "")
    {
        this.Name     = name;
        this.Editable = editable;
        this.Info     = info;
    }

    public string Name     { get; }
    public bool   Editable { get; }
    public string Info     { get; }
}

// ints/floats with open ranges
// (to be numerical entry)
public class NumEntryField : FieldBase
{
    public NumEntryField(string name, bool editable, dynamic val, dynamic? lowerLimit, dynamic? upperLimit, dynamic increment, string info = "") : base(name, editable, info)
    {
        if (Double.IsNaN(val) || Double.IsInfinity(val))
            val = 0;
        if (lowerLimit is null)
            lowerLimit = Decimal.MinValue;
        if (upperLimit is null)
            upperLimit = Decimal.MaxValue;

        _value          = (decimal?)val;
        _lowerLimit     = (decimal?)lowerLimit;
        _upperLimit     = (decimal?)upperLimit;
        this.Increment  = (decimal)increment;
    }

    private decimal? _value;
    public decimal? Value
    {
        get => _value;
        set
        {
            if (!(value is null))
                this.RaiseAndSetIfChanged(ref _value, value);
        }
    }

    private decimal? _lowerLimit;
    public decimal? LowerLimit
    {
        get => _lowerLimit;
        set
        {
            this.RaiseAndSetIfChanged(ref _lowerLimit, value);
            OnPropertyChanged(nameof(LowerLimit));
        }
    }

    private decimal? _upperLimit;
    public decimal? UpperLimit
    {
        get => _upperLimit;
        set
        {
            this.RaiseAndSetIfChanged(ref _upperLimit, value);
            OnPropertyChanged(nameof(UpperLimit));
        }
    }

    public decimal  Increment  { get; set; }
}

// TODO: enable inline non-string elements...?
public class StringEntryField : FieldBase
{
    public StringEntryField(string name, bool editable, string text, int? maxLength, string info = "") : base(name, editable, info)
    {
        _text     = text;
        MaxLength = maxLength;
    }

    private string _text;
    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    // i THINK this should be readonly, but subject to change
    public int? MaxLength { get; }
}

// ints/floats with definite ranges
// (to be sliders)
public class NumRangeField : FieldBase
{
    public NumRangeField(string name, bool editable, dynamic val, dynamic lowerLimit, dynamic upperLimit, dynamic? increment, string info = "") : base(name, editable, info)
    {
        if (Double.IsNaN(val) || Double.IsInfinity(val))
            val = 0;

        _value          = (decimal?)val;
        this.LowerLimit = (decimal)lowerLimit;
        this.UpperLimit = (decimal)upperLimit;
        this.Increment  = (decimal?)increment;
    }

    private decimal? _value;
    public decimal? Value
    {
        get => _value;
        set
        {
            if (!(value is null))
                this.RaiseAndSetIfChanged(ref _value, value);
                OnPropertyChanged(nameof(Value));
        }
    }

    public decimal  LowerLimit { get; set; }
    public decimal  UpperLimit { get; set; }
    public decimal? Increment  { get; set; }
}

// indices, maybe unknown enums
// (to be dropdowns)
public class IntSelectionField : FieldBase
{
    public IntSelectionField(string name, bool editable, int choiceIndex, List<int> choices, string info = "") : base(name, editable, info)
    {
        _choice = choiceIndex;
        _choices = new ObservableCollection<int>(choices);
    }

    private int _choice;
    public int Choice
    {
        get => _choice;
        set => this.RaiseAndSetIfChanged(ref _choice, value);
    }

    private ObservableCollection<int> _choices;
    public ObservableCollection<int> Choices
    {
        get => _choices;
        set => this.RaiseAndSetIfChanged(ref _choices, value);
    }
}

// known enums
// (to be dropdowns)
public class StringSelectionField : FieldBase
{
    public StringSelectionField(string name, bool editable, string choiceIndex, List<string> choices, string info = "") : base(name, editable, info)
    {
        _choice = choiceIndex;
        _choices = new ObservableCollection<string>(choices);
    }

    private string _choice;
    public string Choice
    {
        get => _choice;
        set
        {
            this.RaiseAndSetIfChanged(ref _choice, value);
            OnPropertyChanged(nameof(Choice));
        }
    }

    private ObservableCollection<string> _choices;
    public ObservableCollection<string> Choices
    {
        get => _choices;
        set => this.RaiseAndSetIfChanged(ref _choices, value);
    }
}

// (to be checkboxes)
public class BoolChoiceField : FieldBase
{
    public BoolChoiceField(string name, bool editable, bool val, string info = "") : base(name, editable, info)
    {
        _value = val;
    }

    private bool _value;
    public bool Value
    {
        get => _value;
        set
        {
            this.RaiseAndSetIfChanged(ref _value, value);
            OnPropertyChanged(nameof(Value));
        }
    }
}

public class ColorSelectionField : FieldBase
{
    public ColorSelectionField(string name, bool editable, UInt32 rgba, string info = "") : base(name, editable, info)
    {
        _selectedColor = new Color(
            (byte)(rgba & 0xFF),
            (byte)((rgba >> 24) & 0xFF),
            (byte)((rgba >> 16) & 0xFF),
            (byte)((rgba >> 8) & 0xFF)
        );
    }

    public UInt32 ToUInt32()
    {
        UInt32 ret = 0;
        ret |= (uint)_selectedColor.R << 24;
        ret |= (uint)_selectedColor.G << 16;
        ret |= (uint)_selectedColor.B << 8;
        ret |= (uint)_selectedColor.A;
        return ret;
    }

    private Color _selectedColor;
    public Color SelectedColor
    {
        get => _selectedColor;
        set => this.RaiseAndSetIfChanged(ref _selectedColor, value);
    }
}

public class AnimationWidget : ViewModelBase
{
    public AnimationWidget(DataManager config, IntSelectionField modelID, AnimationStruct animation, BitfieldBase bitfield, string name, int? enabledInd = null, int? extInd = null, int? frameBlendingInd = null, bool enabledFlip = false, bool extFlip = false, bool frameBlendingFlip = false, bool loopFlip = false, int? trackNum = null)
    {
        _modelID = modelID;

        this.Name = name;
        this.Editable = !config.ReadOnly;

        if (enabledInd is null)
            this.AnimationEnabled = new BoolChoiceField("", false, true);
        else
        {
            this.AnimationEnabled = new BoolChoiceField("Enabled?", this.Editable, (enabledFlip) ? (!bitfield[(int)enabledInd]) : (bitfield[(int)enabledInd]));
            this.WhenAnyValue(_ => _.AnimationEnabled.Value).Subscribe(_ => bitfield[(int)enabledInd] = (enabledFlip) ? (!this.AnimationEnabled.Value) : (this.AnimationEnabled.Value));
        }

        if (extInd is null)
            this.AnimationFromExt = new BoolChoiceField("", false, false);
        else
        {
            this.AnimationFromExt = new BoolChoiceField("From ext?", this.Editable, (extFlip) ? (!bitfield[(int)extInd]) : (bitfield[(int)extInd]));
            this.WhenAnyValue(_ => _.AnimationFromExt.Value).Subscribe(_ => bitfield[(int)extInd] = (extFlip) ? (!this.AnimationFromExt.Value) : (this.AnimationFromExt.Value));
        }

        if (frameBlendingInd is null)
            this.AnimationFrameBlendingEnabled = new BoolChoiceField("", false, false);
        else
        {
            this.AnimationFrameBlendingEnabled = new BoolChoiceField("Frame Blending Enabled?", this.Editable, (frameBlendingFlip) ? (!bitfield[(int)frameBlendingInd]) : (bitfield[(int)frameBlendingInd]));
            this.WhenAnyValue(_ => _.AnimationFrameBlendingEnabled.Value).Subscribe(_ => bitfield[(int)frameBlendingInd] = (frameBlendingFlip) ? (!this.AnimationFrameBlendingEnabled.Value) : (this.AnimationFrameBlendingEnabled.Value));
        }

        if (animation.IncludeLoopBool)
        {
            this.AnimationLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, (loopFlip) ? (animation.LoopBool == 0) : (animation.LoopBool != 0));
            this.WhenAnyValue(_ => _.AnimationLoopBool.Value).Subscribe(_ => animation.LoopBool = Convert.ToUInt32((loopFlip) ? (!this.AnimationLoopBool.Value) : (this.AnimationLoopBool.Value)));
        }
        else
            this.AnimationLoopBool = new BoolChoiceField("", false, true);

        if (animation.IncludeIndex)
        {
            this.AnimationID = new NumEntryField("Animation ID", this.Editable, animation.Index, 0, 59, 1);
            this.WhenAnyValue(_ => _.AnimationID.Value).Subscribe(_ => animation.Index = (uint)this.AnimationID.Value);
        }
        else
            this.AnimationID = new NumEntryField("", false, 0, 0, 0, 0);

        if (animation.IncludeStartingFrame)
        {
            this.AnimationStartingFrame = new NumEntryField("Starting Frame", this.Editable, animation.StartingFrame, 0, 99999, 1);
            this.WhenAnyValue(_ => _.AnimationStartingFrame.Value).Subscribe(_ => animation.StartingFrame = (uint)this.AnimationStartingFrame.Value);
        }
        else
            this.AnimationStartingFrame = new NumEntryField("", false, 0, 0, 0, 0);

        if (animation.IncludeEndingFrame)
        {
            this.AnimationEndingFrame = new NumEntryField("Ending Frame", this.Editable, animation.EndingFrame, 0, 99999, 1);
            this.WhenAnyValue(_ => _.AnimationEndingFrame.Value).Subscribe(_ => animation.EndingFrame = (uint)this.AnimationEndingFrame.Value);
        }
        else
            this.AnimationEndingFrame = new NumEntryField("", false, 0, 0, 0, 0);

        if (animation.IncludeInterpolatedFrames)
        {
            this.AnimationInterpolatedFrames = new NumEntryField("Interpolated Frames", this.Editable, animation.InterpolatedFrames, 0, 100, 1);
            this.WhenAnyValue(_ => _.AnimationInterpolatedFrames.Value).Subscribe(_ => animation.InterpolatedFrames = (uint)this.AnimationInterpolatedFrames.Value);
        }
        else
            this.AnimationInterpolatedFrames = new NumEntryField("", false, 0, 0, 0, 0);

        if (animation.IncludePlaybackSpeed)
        {
            this.AnimationPlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, animation.PlaybackSpeed, 0, 10, 0.1);
            this.WhenAnyValue(_ => _.AnimationPlaybackSpeed.Value).Subscribe(_ => animation.PlaybackSpeed = (float)this.AnimationPlaybackSpeed.Value);
        }
        else
            this.AnimationPlaybackSpeed = new NumEntryField("", false, 0, 0, 0, 0);

        if (animation.IncludeWeight)
        {
            this.AnimationWeight = new NumEntryField("Weight", this.Editable, animation.Weight, 0, 1, 0.01);
            this.WhenAnyValue(_ => _.AnimationWeight.Value).Subscribe(_ => animation.Weight = (float)this.AnimationWeight.Value);
        }
        else
            this.AnimationWeight = new NumEntryField("", false, 0, 0, 0, 0);

        this.AnimationPreviewVM = new GFDRenderingPanelViewModel(config);
        if (!(extInd is null) && animation.IncludeIndex)
        {
            this.WhenAnyValue(x => x.AnimationPreviewVM.ReadyToRender).Subscribe(x =>
            {
                if (x)
                {
                    this.AnimationPreviewVM.sceneManager.LoadObjects(config, new int[] {_objectID});
                    if (trackNum is null)
                        this.AnimationPreviewVM.sceneManager.LoadBaseAnimation(_objectID, this.AnimationFromExt.Value, (int)this.AnimationID.Value);
                    else
                        this.AnimationPreviewVM.sceneManager.LoadAddAnimationTrack(_objectID, this.AnimationFromExt.Value, (int)this.AnimationID.Value, (int)trackNum);
                }
                else 
                    this.AnimationPreviewVM.sceneManager.teardown();
            });

            this.WhenAnyValue(y => y.AnimationID.Value, y => y.AnimationFromExt.Value).Subscribe(y =>
            {
                if (this.AnimationPreviewVM.ReadyToRender)
                    if (trackNum is null)
                        this.AnimationPreviewVM.sceneManager.LoadBaseAnimation(_objectID, this.AnimationFromExt.Value, (int)this.AnimationID.Value);
                    else
                        this.AnimationPreviewVM.sceneManager.LoadAddAnimationTrack(_objectID, this.AnimationFromExt.Value, (int)this.AnimationID.Value, (int)trackNum);
            });
        }
    }

    public GFDRenderingPanelViewModel AnimationPreviewVM { get; set; }

    public BoolChoiceField  AnimationEnabled              { get; set; }
    public BoolChoiceField  AnimationFromExt              { get; set; }
    public BoolChoiceField  AnimationLoopBool             { get; set; }
    public BoolChoiceField  AnimationFrameBlendingEnabled { get; set; }

    public NumEntryField    AnimationID                   { get; set; }

    public NumEntryField    AnimationStartingFrame        { get; set; }
    public NumEntryField    AnimationEndingFrame          { get; set; }
    public NumEntryField    AnimationInterpolatedFrames   { get; set; }
    public NumEntryField    AnimationPlaybackSpeed        { get; set; }
    public NumEntryField    AnimationWeight               { get; set; }

    public bool   Editable { get; }
    public string Name     { get; }

    protected IntSelectionField _modelID;
    private int _objectID { get => (int)_modelID.Choice; }
}

public class ModelPreviewWidget : ViewModelBase
{
    public ModelPreviewWidget(DataManager config, IntSelectionField modelID)
    {
        _modelID = modelID;

        this.ModelPreviewVM = new GFDRenderingPanelViewModel(config);

        this.WhenAnyValue(x => x.ModelPreviewVM.ReadyToRender).Subscribe(x =>
        {
            if (x)
                this.ModelPreviewVM.sceneManager.LoadObjects(config, new int[] {_objectID});
            else
                this.ModelPreviewVM.sceneManager.teardown();
        });
    }

    public GFDRenderingPanelViewModel ModelPreviewVM { get; set; }

    protected IntSelectionField _modelID;
    private int _objectID { get => (int)_modelID.Choice; }
}

public class InterpolationParameters : ViewModelBase
{
    private uint Field;
    private bool Editable;

    public StringSelectionField InterpolationType { get; set; }
    public StringSelectionField SlopeInType       { get; set; }
    public StringSelectionField SlopeOutType      { get; set; }

    public InterpolationParameters(uint field, bool editable)
    {
        this.Field = field;
        this.Editable = editable;

        this.InterpolationType = new StringSelectionField("Interpolation Type", this.Editable, this.InterpolationTypes.Backward[(this.Field & 0xFF)], this.InterpolationTypes.Keys);
        this.SlopeInType = new StringSelectionField("Slope-In Type", this.Editable, this.SlopeTypes.Backward[((this.Field >> 8) & 0xF)], this.SlopeTypes.Keys);
        this.SlopeOutType = new StringSelectionField("Slope-Out Type", this.Editable, this.SlopeTypes.Backward[((this.Field >> 12) & 0xF)], this.SlopeTypes.Keys);
    }

    public uint Compose()
    {
        this.Field = 0;
        this.Field |= this.InterpolationTypes.Forward[this.InterpolationType.Choice];
        this.Field |= (this.SlopeTypes.Forward[this.SlopeInType.Choice] << 8);
        this.Field |= (this.SlopeTypes.Forward[this.SlopeOutType.Choice] << 12);
        return this.Field;
    }

    public BiDict<string, uint> InterpolationTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Linear",   0},
            {"Step",     1},
            {"Hermite",  2},
        }
    );

    public BiDict<string, uint> SlopeTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Normal", 0},
            {"Slow",   1},
            {"Fast",   2},
        }
    );
}

public class Position3D : ViewModelBase
{
    public Position3D(string name, bool editable, float[] position)
    {
        _name = name;
        this.Editable = editable;

        this.X = new NumRangeField("X", this.Editable, position[0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.X.Value).Subscribe(_ => position[0] = (float)this.X.Value);
        this.Y = new NumRangeField("Y", this.Editable, position[1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Y.Value).Subscribe(_ => position[1] = (float)this.Y.Value);
        this.Z = new NumRangeField("Z", this.Editable, position[2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Z.Value).Subscribe(_ => position[2] = (float)this.Z.Value);
    }

    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    public bool   Editable { get; }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            OnPropertyChanged(nameof(Name));
        }
    }
}

public class DegreeField : NumRangeField { public DegreeField(string name, bool editable, float val) : base(name, editable, val, -180, 180, 1) {} }
public class RotationWidget : ViewModelBase
{
    public RotationWidget(DataManager config, float[] rotation, BitfieldBase bitfield, int pitchInd = 0, int yawInd = 1, int rollInd = 2, int? enabledInd = null, int? pitchEnabledInd = null, int? yawEnabledInd = null, int? rollEnabledInd = null, bool enabledFlip = false, bool pitchEnabledFlip = false, bool yawEnabledFlip = false, bool rollEnabledFlip = false, string name = "Rotation (Degrees)")
    {
        this.Name = name;
        this.Editable = !config.ReadOnly;

        if (enabledInd is null)
            this.RotationEnabled = new BoolChoiceField("", false, true);
        else
        {
            this.RotationEnabled = new BoolChoiceField("Enabled?", this.Editable, enabledFlip ? !bitfield[(int)enabledInd] : bitfield[(int)enabledInd]);
            this.WhenAnyValue(_ => _.RotationEnabled.Value).Subscribe(_ => bitfield[(int)enabledInd] = enabledFlip ? !this.RotationEnabled.Value : this.RotationEnabled.Value);
        }

        if (pitchEnabledInd is null)
            this.PitchEnabled = new BoolChoiceField("", false, true);
        else
        {
            this.PitchEnabled = new BoolChoiceField("Pitch Enabled?", this.Editable, pitchEnabledFlip ? !bitfield[(int)pitchEnabledInd] : bitfield[(int)pitchEnabledInd]);
            this.WhenAnyValue(_ => _.PitchEnabled.Value).Subscribe(_ => bitfield[(int)pitchEnabledInd] = pitchEnabledFlip ? !this.PitchEnabled.Value : this.PitchEnabled.Value);
        }

        if (yawEnabledInd is null)
            this.YawEnabled = new BoolChoiceField("", false, true);
        else
        {
            this.YawEnabled = new BoolChoiceField("Yaw Enabled?", this.Editable, yawEnabledFlip ? !bitfield[(int)yawEnabledInd] : bitfield[(int)yawEnabledInd]);
            this.WhenAnyValue(_ => _.YawEnabled.Value).Subscribe(_ => bitfield[(int)yawEnabledInd] = yawEnabledFlip ? !this.YawEnabled.Value : this.YawEnabled.Value);
        }

        if (rollEnabledInd is null)
            this.RollEnabled = new BoolChoiceField("", false, true);
        else
        {
            this.RollEnabled = new BoolChoiceField("Roll Enabled?", this.Editable, rollEnabledFlip ? !bitfield[(int)rollEnabledInd] : bitfield[(int)rollEnabledInd]);
            this.WhenAnyValue(_ => _.RollEnabled.Value).Subscribe(_ => bitfield[(int)rollEnabledInd] = rollEnabledFlip ? !this.RollEnabled.Value : this.RollEnabled.Value);
        }

        this.PitchDegrees = new DegreeField("Pitch", this.Editable, rotation[pitchInd]);
        this.WhenAnyValue(_ => _.PitchDegrees.Value).Subscribe(_ => rotation[pitchInd] = (float)this.PitchDegrees.Value);

        this.YawDegrees = new DegreeField("Yaw", this.Editable, rotation[yawInd]);
        this.WhenAnyValue(_ => _.YawDegrees.Value).Subscribe(_ => rotation[yawInd] = (float)this.YawDegrees.Value);

        this.RollDegrees = new DegreeField("Roll", this.Editable, rotation[rollInd]);
        this.WhenAnyValue(_ => _.RollDegrees.Value).Subscribe(_ => rotation[rollInd] = (float)this.RollDegrees.Value);
    }

    public string Name     { get; }
    public bool   Editable { get; }

    public BoolChoiceField RotationEnabled { get; set; }
    public BoolChoiceField PitchEnabled    { get; set; }
    public BoolChoiceField YawEnabled      { get; set; }
    public BoolChoiceField RollEnabled     { get; set; }

    public DegreeField PitchDegrees { get; set; }
    public DegreeField YawDegrees   { get; set; }
    public DegreeField RollDegrees  { get; set; }
}
