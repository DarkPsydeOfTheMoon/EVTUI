using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Avalonia.Media;

using ReactiveUI;

namespace EVTUI.ViewModels;

public class FieldBase : ViewModelBase
{
    public FieldBase(string name, bool editable)
    {
        this.Name     = name;
        this.Editable = editable;
    }

    public string Name     { get; }
    public bool   Editable { get; }
}

// ints/floats with open ranges
// (to be numerical entry)
public class NumEntryField : FieldBase
{
    public NumEntryField(string name, bool editable, dynamic val, dynamic? lowerLimit, dynamic? upperLimit, dynamic increment) : base(name, editable)
    {
        _value          = (decimal?)val;
        this.LowerLimit = (decimal?)lowerLimit;
        this.UpperLimit = (decimal?)upperLimit;
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
    public StringEntryField(string name, bool editable, string text, int? maxLength) : base(name, editable)
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
    public NumRangeField(string name, bool editable, dynamic val, dynamic lowerLimit, dynamic upperLimit, dynamic? increment) : base(name, editable)
    {
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
    public IntSelectionField(string name, bool editable, int choiceIndex, List<int> choices) : base(name, editable)
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
    public StringSelectionField(string name, bool editable, string choiceIndex, List<string> choices) : base(name, editable)
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
    public BoolChoiceField(string name, bool editable, bool val) : base(name, editable)
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
    public ColorSelectionField(string name, bool editable, UInt32 rgba) : base(name, editable)
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
        _config = config;
        _modelID = modelID;
        _bitfield = bitfield;
        _animation = animation;

        _enabledInd = enabledInd;
        _extInd = extInd;
        _frameBlendingInd = frameBlendingInd;

        _enabledFlip = enabledFlip;
        _extFlip = extFlip;
        _frameBlendingFlip = frameBlendingFlip;
        _loopFlip = loopFlip;

        this.Name = name;
        this.Editable = !config.ReadOnly;

        if (!(_enabledInd is null))
        {
            this.AnimationEnabled = new BoolChoiceField("Enabled?", this.Editable, (_enabledFlip) ? (!_bitfield[(int)_enabledInd]) : (_bitfield[(int)_enabledInd]));
            this.WhenAnyValue(_ => _.AnimationEnabled.Value).Subscribe(_ => _bitfield[(int)_enabledInd] = (_enabledFlip) ? (!this.AnimationEnabled.Value) : (this.AnimationEnabled.Value));
        }

        if (!(_extInd is null))
        {
            this.AnimationFromExt = new BoolChoiceField("From ext?", this.Editable, (_extFlip) ? (!_bitfield[(int)_extInd]) : (_bitfield[(int)_extInd]));
            this.WhenAnyValue(_ => _.AnimationFromExt.Value).Subscribe(_ => _bitfield[(int)_extInd] = (_extFlip) ? (!this.AnimationFromExt.Value) : (this.AnimationFromExt.Value));
        }

        if (!(_frameBlendingInd is null))
        {
            this.AnimationFrameBlendingEnabled = new BoolChoiceField("Frame Blending Enabled?", this.Editable, (_frameBlendingFlip) ? (!_bitfield[(int)_frameBlendingInd]) : (_bitfield[(int)_frameBlendingInd]));
            this.WhenAnyValue(_ => _.AnimationFrameBlendingEnabled.Value).Subscribe(_ => _bitfield[(int)_frameBlendingInd] = (_frameBlendingFlip) ? (!this.AnimationFrameBlendingEnabled.Value) : (this.AnimationFrameBlendingEnabled.Value));
        }

        if (_animation.IncludeLoopBool)
        {
            this.AnimationLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, (_loopFlip) ? (_animation.LoopBool == 0) : (_animation.LoopBool != 0));
            this.WhenAnyValue(_ => _.AnimationLoopBool.Value).Subscribe(_ => _animation.LoopBool = Convert.ToUInt32((_loopFlip) ? (!this.AnimationLoopBool.Value) : (this.AnimationLoopBool.Value)));
        }

        if (_animation.IncludeIndex)
        {
            this.AnimationID = new NumEntryField("Animation ID", this.Editable, _animation.Index, 0, 59, 1);
            this.WhenAnyValue(_ => _.AnimationID.Value).Subscribe(_ => _animation.Index = (uint)this.AnimationID.Value);
        }

        if (_animation.IncludeStartingFrame)
        {
            this.AnimationStartingFrame = new NumEntryField("Starting Frame", this.Editable, _animation.StartingFrame, 0, 99999, 1);
            this.WhenAnyValue(_ => _.AnimationStartingFrame.Value).Subscribe(_ => _animation.StartingFrame = (uint)this.AnimationStartingFrame.Value);
        }

        if (_animation.IncludeEndingFrame)
        {
            this.AnimationEndingFrame = new NumEntryField("Ending Frame", this.Editable, _animation.EndingFrame, 0, 99999, 1);
            this.WhenAnyValue(_ => _.AnimationEndingFrame.Value).Subscribe(_ => _animation.EndingFrame = (uint)this.AnimationEndingFrame.Value);
        }

        if (_animation.IncludeInterpolatedFrames)
        {
            this.AnimationInterpolatedFrames = new NumEntryField("Interpolated Frames", this.Editable, _animation.InterpolatedFrames, 0, 100, 1);
            this.WhenAnyValue(_ => _.AnimationInterpolatedFrames.Value).Subscribe(_ => _animation.InterpolatedFrames = (uint)this.AnimationInterpolatedFrames.Value);
        }

        if (_animation.IncludePlaybackSpeed)
        {
            this.AnimationPlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, _animation.PlaybackSpeed, 0, 10, 0.1);
            this.WhenAnyValue(_ => _.AnimationPlaybackSpeed.Value).Subscribe(_ => _animation.PlaybackSpeed = (float)this.AnimationPlaybackSpeed.Value);
        }

        if (_animation.IncludeWeight)
        {
            this.AnimationWeight = new NumEntryField("Weight", this.Editable, _animation.Weight, 0, 1, 0.01);
            this.WhenAnyValue(_ => _.AnimationWeight.Value).Subscribe(_ => _animation.Weight = (float)this.AnimationWeight.Value);
        }

        this.AnimationPreviewVM = new GFDRenderingPanelViewModel();
        this.WhenAnyValue(x => x.AnimationPreviewVM.ReadyToRender).Subscribe(x =>
        {
            if (x)
            {
                this.AnimationPreviewVM.sceneManager.LoadObjects(_config, new int[] {_objectID});
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

    protected DataManager _config;
    protected BitfieldBase _bitfield;
    public AnimationStruct _animation;
    protected int? _enabledInd;
    protected int? _extInd;
    protected int? _frameBlendingInd;
    protected bool _enabledFlip;
    protected bool _extFlip;
    protected bool _frameBlendingFlip;
    protected bool _loopFlip;
}

public class ModelPreviewWidget : ViewModelBase
{
    public ModelPreviewWidget(DataManager config, IntSelectionField modelID)
    {
        _config = config;
        _modelID = modelID;

        this.ModelPreviewVM = new GFDRenderingPanelViewModel();

        this.WhenAnyValue(x => x.ModelPreviewVM.ReadyToRender).Subscribe(x =>
        {
            if (x)
                this.ModelPreviewVM.sceneManager.LoadObjects(_config, new int[] {_objectID});
            else
                this.ModelPreviewVM.sceneManager.teardown();
        });
    }

    public GFDRenderingPanelViewModel ModelPreviewVM { get; set; }

    protected IntSelectionField _modelID;
    private int _objectID { get => (int)_modelID.Choice; }

    protected DataManager _config;
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

public class Target : ViewModelBase
{
    public Target(DataManager config, object commandData, int groupInd, bool isActive)
    {
        this.CommandData = commandData;

        this.Editable = !config.ReadOnly;
        this.Idx = groupInd;
        _isActive = isActive;

        this.X = new NumRangeField("X", this.Editable, this.CommandData.Targets[this.Idx,0], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.X.Value).Subscribe(_ => this.CommandData.Targets[this.Idx,0] = (float)this.X.Value);
        this.Y = new NumRangeField("Y", this.Editable, this.CommandData.Targets[this.Idx,1], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Y.Value).Subscribe(_ => this.CommandData.Targets[this.Idx,1] = (float)this.Y.Value);
        this.Z = new NumRangeField("Z", this.Editable, this.CommandData.Targets[this.Idx,2], -99999, 99999, 1);
        this.WhenAnyValue(_ => _.Z.Value).Subscribe(_ => this.CommandData.Targets[this.Idx,2] = (float)this.Z.Value);
    }

    public NumRangeField X { get; set; }
    public NumRangeField Y { get; set; }
    public NumRangeField Z { get; set; }

    public bool   Editable { get; }
    public int    Idx      { get; }
    public string Name     { get => $"Position #{this.Idx+1}"; }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            this.RaiseAndSetIfChanged(ref _isActive, value);
            OnPropertyChanged(nameof(IsActive));
        }
    }

    protected dynamic CommandData;
}
