using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        _value          = (decimal)val;
        this.LowerLimit = (decimal?)lowerLimit;
        this.UpperLimit = (decimal?)upperLimit;
        this.Increment  = (decimal)increment;
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
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
    public StringEntryField(string name, bool editable, string text) : base(name, editable)
    {
        _text = text;
    }

    private string _text;
    public string Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }
}

// ints/floats with definite ranges
// (to be sliders)
public class NumRangeField : FieldBase
{
    public NumRangeField(string name, bool editable, dynamic val, dynamic lowerLimit, dynamic upperLimit, dynamic? increment) : base(name, editable)
    {
        _value          = (decimal)val;
        this.LowerLimit = (decimal)lowerLimit;
        this.UpperLimit = (decimal)upperLimit;
        this.Increment  = (decimal?)increment;
    }

    private decimal _value;
    public decimal Value
    {
        get => _value;
        set
        {
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
    public ColorSelectionField(string name, bool editable, byte[] rgba) : base(name, editable)
    {
        _selectedColor = new Color(rgba[3], rgba[0], rgba[1], rgba[2]);
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
    public AnimationWidget(DataManager config, IntSelectionField modelID, AnimationStruct animation, Bitfield bitfield, string name, int? enabledInd = null, int? extInd = null, int? frameBlendingInd = null, bool enabledFlip = false, bool extFlip = false, bool frameBlendingFlip = false, bool loopFlip = false, int? trackNum = null)
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
            if (_enabledFlip)
                this.AnimationEnabled = new BoolChoiceField("Enabled?", this.Editable, !_bitfield[(int)_enabledInd]);
            else
                this.AnimationEnabled = new BoolChoiceField("Enabled?", this.Editable, _bitfield[(int)_enabledInd]);

        if (!(_extInd is null))
            if (_extFlip)
                this.AnimationFromExt = new BoolChoiceField("From ext?", this.Editable, !_bitfield[(int)_extInd]);
            else
                this.AnimationFromExt = new BoolChoiceField("From ext?", this.Editable, _bitfield[(int)_extInd]);

        if (!(_frameBlendingInd is null))
            if (_frameBlendingFlip)
                this.AnimationFrameBlendingEnabled = new BoolChoiceField("Frame Blending Enabled?", this.Editable, !_bitfield[(int)_frameBlendingInd]);
            else
                this.AnimationFrameBlendingEnabled = new BoolChoiceField("Frame Blending Enabled?", this.Editable, _bitfield[(int)_frameBlendingInd]);

        if (_animation.IncludeLoopBool)
            if (_loopFlip)
                this.AnimationLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, (_animation.LoopBool == 0));
            else
                this.AnimationLoopBool = new BoolChoiceField("Loop Playback?", this.Editable, (_animation.LoopBool != 0));

        if (_animation.IncludeIndex)
            this.AnimationID = new NumEntryField("Animation ID", this.Editable, _animation.Index, 0, 59, 1);

        if (_animation.IncludeStartingFrame)
            this.AnimationStartingFrame = new NumEntryField("Starting Frame", this.Editable, _animation.StartingFrame, 0, 99999, 1);
        if (_animation.IncludeEndingFrame)
            this.AnimationEndingFrame = new NumEntryField("Ending Frame", this.Editable, _animation.EndingFrame, 0, 99999, 1);
        if (_animation.IncludeInterpolatedFrames)
            this.AnimationInterpolatedFrames = new NumEntryField("Interpolated Frames", this.Editable, _animation.InterpolatedFrames, 0, 100, 1);
        if (_animation.IncludePlaybackSpeed)
            this.AnimationPlaybackSpeed = new NumEntryField("Playback Speed", this.Editable, _animation.PlaybackSpeed, 0, 10, 0.1);
        if (_animation.IncludeWeight)
            this.AnimationWeight = new NumEntryField("Weight", this.Editable, _animation.Weight, 0, 1, 0.01);

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
    protected Bitfield _bitfield;
    public AnimationStruct _animation;
    protected int? _enabledInd;
    protected int? _extInd;
    protected int? _frameBlendingInd;
    protected bool _enabledFlip;
    protected bool _extFlip;
    protected bool _frameBlendingFlip;
    protected bool _loopFlip;

    public void SaveChanges()
    {
        if (!(_enabledInd is null))
            if (_enabledFlip)
                _bitfield[(int)_enabledInd] = !this.AnimationEnabled.Value;
            else
                _bitfield[(int)_enabledInd] = this.AnimationEnabled.Value;

        if (!(_extInd is null))
            if (_extFlip)
                _bitfield[(int)_extInd] = !this.AnimationFromExt.Value;
            else
                _bitfield[(int)_extInd] = this.AnimationFromExt.Value;

        if (!(_frameBlendingInd is null))
            if (_frameBlendingFlip)
                _bitfield[(int)_frameBlendingInd] = !this.AnimationFrameBlendingEnabled.Value;
            else
                _bitfield[(int)_frameBlendingInd] = this.AnimationFrameBlendingEnabled.Value;

        if (_animation.IncludeLoopBool)
            if (_loopFlip)
                _animation.LoopBool = Convert.ToUInt32(!this.AnimationLoopBool.Value);
            else
                _animation.LoopBool = Convert.ToUInt32(this.AnimationLoopBool.Value);

        if (_animation.IncludeIndex)
            _animation.Index              = (uint)this.AnimationID.Value;

        if (_animation.IncludeStartingFrame)
            _animation.StartingFrame      = (uint)this.AnimationStartingFrame.Value;
        if (_animation.IncludeEndingFrame)
            _animation.EndingFrame        = (uint)this.AnimationEndingFrame.Value;
        if (_animation.IncludeInterpolatedFrames)
            _animation.InterpolatedFrames = (uint)this.AnimationInterpolatedFrames.Value;
        if (_animation.IncludePlaybackSpeed)
            _animation.PlaybackSpeed      = (float)this.AnimationPlaybackSpeed.Value;
        if (_animation.IncludeWeight)
            _animation.Weight             = (float)this.AnimationWeight.Value;
    }
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
