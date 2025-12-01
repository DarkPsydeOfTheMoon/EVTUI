using System;

using ReactiveUI;

namespace EVTUI.ViewModels.TimelineCommands;

public class ESD_ : Generic
{
    public ESD_(DataManager config, CommonViewModels commonVMs, CommandPointer cmd) : base(config, commonVMs, cmd)
    {
        this.LongName = "Effect: Placement (Coordinates)";
        this.AssetID = new IntSelectionField("Asset ID", this.Editable, this.Command.ObjectId, config.EventManager.AssetIDsOfType(0x01000002));
        this.WhenAnyValue(_ => _.AssetID.Choice).Subscribe(_ => this.Command.ObjectId = this.AssetID.Choice);

        this.Position = new Position3D("Position", this.Editable, this.CommandData.Position);
        this.Rotation = new RotationWidget(config, this.CommandData.Rotation, null, pitchInd: 0, yawInd: 1);
    }

    public IntSelectionField AssetID { get; set; }

    public Position3D     Position { get; set; }
    public RotationWidget Rotation { get; set; }
}
