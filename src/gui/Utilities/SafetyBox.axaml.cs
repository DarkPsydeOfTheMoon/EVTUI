using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EVTUI.Views;

public partial class SafetyBox : UserControl
{
    public string MainMsg = "Are you sure?";
    public string YesMsg = "Yes";
    public string NoMsg = "No";

    public SafetyBox(string mainMsg, string yesMsg, string noMsg)
    {
        this.Initialized += this.OnInitialized;
        InitializeComponent();
        this.MainMsg = mainMsg;
        this.YesMsg = yesMsg;
        this.NoMsg = noMsg;
    }

    public void OnInitialized(object? sender, EventArgs e)
    {
        this.ModalText.Text = this.MainMsg;
        this.YesText.Text = this.YesMsg;
        this.NoText.Text = this.NoMsg;
        this.NoButton.AttachedToVisualTree += (s, e) => this.NoButton.Focus();
    }

    public void YesHandler(object sender, RoutedEventArgs args)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;
        topLevel.Close(0);
    }

    public void NoHandler(object sender, RoutedEventArgs args)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;
        topLevel.Close(null);
    }
}
