using System;
using DeepCopy;

namespace EVTUI.ViewModels;

public class Clipboard
{
    // TODO: split this into different types of things you can copy
    // (once there are multiple such things)

    public CommandPointer CopiedCommand  { get; set; }
    public Position3D     CopiedPosition { get; set; }
    public RotationWidget CopiedRotation { get; set; }

    public void CopyCommand(CommandPointer cmd)
    {
        this.CopiedCommand = DeepCopier.Copy(cmd);
    }

    public void CopyPosition(Position3D pos)
    {
        this.CopiedPosition = DeepCopier.Copy(pos);
    }

    public void CopyRotation(RotationWidget rot)
    {
        this.CopiedRotation = DeepCopier.Copy(rot);
    }
}

public class LandingPageViewModel : ViewModelBase
{
 
    ////////////////////////////
    // *** PUBLIC MEMBERS *** //
    ////////////////////////////
    public User UserData;

    ////////////////////////////
    // *** PUBLIC METHODS *** //
    ////////////////////////////
    public LandingPageViewModel(User userData)
    {
        this.UserData = userData;
    }

}
