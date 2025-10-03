using System;

namespace EVTUI.ViewModels;

public class Clipboard
{
    // TODO: split this into different types of things you can copy
    // (once there are multiple such things)

    public CommandPointer         CopiedCommand  { get; set; }
    public TimelinePanelViewModel SourceVM       { get; set; }

    public bool HasCopiedCommand { get; set; } = false;
    public bool DeleteOriginal   { get; set; } = false;

    public void CopyCommand(TimelinePanelViewModel sourceVM, CommandPointer cmd, bool deleteOriginal)
    {
        this.SourceVM = sourceVM;
        this.CopiedCommand  = cmd;
        this.DeleteOriginal = deleteOriginal;
        this.HasCopiedCommand = true;
    }

    public void DeleteCommand()
    {
        // if the source VM has been closed...? does this class keep it from garbage collection??
        if (this.CopiedCommand is null || this.SourceVM is null)
            return;

        bool success = this.SourceVM.DeleteCommand(this.CopiedCommand);
        if (success)
            this.DeleteOriginal = false;
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
