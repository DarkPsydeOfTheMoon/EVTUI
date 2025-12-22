using System;
using System.Collections.Generic;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class Generic : ReactiveObject
{
    public Generic(DataManager config, CommonViewModels commonVMs, CommandPointer cmd)
    {
        this.Command     = cmd.Command;
        this.CommandData = cmd.CommandData;
        
        this.LongName    = cmd.Command.CommandCode;
        this.Size        = cmd.Command.DataSize.Value;
        this.Basics      = new Basics(config, cmd);
        this.Editable    = !config.ReadOnly;
    }

    protected SerialCommand Command;
    protected dynamic       CommandData;

    public string LongName { get; set; }
    public int    Size     { get; }
    public Basics Basics   { get; set; }
    public bool   Editable { get; }

    public BiDict<string, uint> BlurTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"5x5 Gaussian Filter",  0},
            {"2-Iteration Gaussian", 1},
            {"3-Iteration Gaussian", 2},
            {"5-Iteration Gaussian", 3},
            {"7-Iteration Gaussian", 4},
        }
    );

    public BiDict<string, uint> MessageCoordinateTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Top Left",      0},
            {"Top Center",    1},
            {"Top Right",     2},
            {"Bottom Left",   3},
            {"Bottom Center", 4},
            {"Bottom Right",  5},
        }
    );

    protected BiDict<string, uint> HelperNames;
    protected BiDict<string, int> ResourceHelperNames;
    protected void UpdateHelperNames(CommonViewModels commonVMs, int assetID, bool isField = false)
    {
        this.HelperNames = new BiDict<string, uint>();
        if (isField)
            this.ResourceHelperNames = new BiDict<string, int>();
            if (commonVMs.AssetsByID.ContainsKey(assetID))
            {
                AssetViewModel asset = commonVMs.AssetsByID[assetID];
                if (!String.IsNullOrEmpty(asset.ActiveModelPath))
                {
                    GFDLibrary.ModelPack model = GFDLibrary.Api.FlatApi.LoadModel(asset.ActiveModelPath);
                    foreach (GFDLibrary.Models.Node node in model.Model.Nodes)
                    {
                        if (node.Properties.ContainsKey("gfdHelperID"))
                        {
                            int id = (int)node.Properties["gfdHelperID"].GetValue();
                            this.HelperNames.Add($"{node.Name} ({id})", (uint)id);
                        }
                        if (isField && node.Properties.ContainsKey("fldLayoutOfModel_resId") && node.Properties.ContainsKey("fldLayoutOfModel_major") && node.Properties.ContainsKey("fldLayoutOfModel_minor"))
                        {
                            int resId = (int)node.Properties["fldLayoutOfModel_resId"].GetValue();
                            int majorId = (int)node.Properties["fldLayoutOfModel_major"].GetValue();
                            int minorId = (int)node.Properties["fldLayoutOfModel_minor"].GetValue();
                            this.ResourceHelperNames.Add($"M{majorId:000}_{minorId:000}", resId);
                        }
                    }
                }
            }
    }
}

public class Basics : ReactiveObject
{
    public Basics(DataManager config, CommandPointer cmd)
    {
        this.Command = cmd.Command;
        this.Editable = !config.ReadOnly;

        this.WaitOnCommand = new BoolChoiceField("Wait while frame is stopped?", this.Editable, this.Command.Flags[1]);
        this.WhenAnyValue(_ => _.WaitOnCommand.Value).Subscribe(_ => this.Command.Flags[1] = this.WaitOnCommand.Value);
        // feels like these two should only be allowed to go to 99999, but in the beta editor they can go up to 999999...
        this.StartingFrame = new NumEntryField("Starting Frame", this.Editable, this.Command.FrameStart, 0, 99999, 1);
        this.WhenAnyValue(_ => _.StartingFrame.Value).Subscribe(_ => this.Command.FrameStart = (int)this.StartingFrame.Value);
        this.FrameCount = new NumEntryField("Frame Duration", this.Editable, this.Command.FrameDuration, 1, 99999, 1);
        this.WhenAnyValue(_ => _.FrameCount.Value).Subscribe(_ => this.Command.FrameDuration = (int)this.FrameCount.Value);

        this.ForceSkipCommand = new BoolChoiceField("Force-skip command?", this.Editable, this.Command.Flags[0]);
        this.WhenAnyValue(_ => _.ForceSkipCommand.Value).Subscribe(_ => this.Command.Flags[0] = this.ForceSkipCommand.Value);
        this.ConditionalType = new StringSelectionField("Conditional Type", this.Editable, this.ConditionalTypes.Backward[this.Command.ConditionalType], this.ConditionalTypes.Keys);
        this.WhenAnyValue(_ => _.ConditionalType.Choice).Subscribe(_ => this.Command.ConditionalType = this.ConditionalTypes.Forward[this.ConditionalType.Choice]);
        this.ConditionalIndex = new NumEntryField("Conditional Index", this.Editable, this.Command.ConditionalIndex, 0, null, 1);
        this.WhenAnyValue(_ => _.ConditionalIndex.Value).Subscribe(_ => this.Command.ConditionalIndex = (uint)this.ConditionalIndex.Value);
        this.ComparisonType = new StringSelectionField("Comparison Type", this.Editable, this.ComparisonTypes.Backward[this.Command.ConditionalComparisonType], this.ComparisonTypes.Keys);
        this.WhenAnyValue(_ => _.ComparisonType.Choice).Subscribe(_ => this.Command.ConditionalComparisonType = this.ComparisonTypes.Forward[this.ComparisonType.Choice]);
        this.ConditionalValue = new NumEntryField("Conditional Value", this.Editable, this.Command.ConditionalValue, null, null, 1);
        this.WhenAnyValue(_ => _.ConditionalValue.Value).Subscribe(_ => this.Command.ConditionalValue = (int)this.ConditionalValue.Value);

        this.WhenAnyValue(x => x.ConditionalType.Choice).Subscribe(x =>
        {
            this.ConditionalIndex.UpperLimit = null;
            if (x == "Reference Global Count")
                this.ConditionalIndex.UpperLimit = 382;
            if (x == "Reference Local Data")
                this.ConditionalIndex.UpperLimit = 99;
            this.ConditionalValue.LowerLimit = null;
            this.ConditionalValue.UpperLimit = null;
            if (x == "Reference Global Bitflag")
            {
                this.ConditionalValue.LowerLimit = 0;
                this.ConditionalValue.UpperLimit = 1;
            }
        });
    }

    public BoolChoiceField      WaitOnCommand { get; set; }
    public NumEntryField        StartingFrame { get; set; }
    public NumEntryField        FrameCount    { get; set; }

    public BoolChoiceField      ForceSkipCommand { get; set; }
    public StringSelectionField ConditionalType  { get; set; }
    public NumEntryField        ConditionalIndex { get; set; }
    public StringSelectionField ComparisonType   { get; set; }
    public NumEntryField        ConditionalValue { get; set; }

    protected SerialCommand Command;
    public    bool          Editable { get; set; }

    public BiDict<string, uint> ConditionalTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"Always Run",                  0},
            {"Never Run",                   1},
            {"Reference Local Data",        2},
            {"Reference Global Bitflag",    3},
            {"Reference Global Counter",    4},
            {"Reference Animation Counter", 5},
        }
    );

    public BiDict<string, uint> ComparisonTypes = new BiDict<string, uint>
    (
        new Dictionary<string, uint>
        {
            {"==", 0},
            {"!=", 1},
            {"<",  2},
            {">",  3},
            {"<=", 4},
            {">=", 5},
        }
    );
}
