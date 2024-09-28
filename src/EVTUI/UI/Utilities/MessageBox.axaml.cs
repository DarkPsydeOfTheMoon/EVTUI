using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EVTUI.Views;

public partial class MessageBox : UserControl
{
    public string text = "";

    public MessageBox(string text)
    {
        this.Initialized += this.OnInitialized;
        InitializeComponent();
        this.text = text;
    }

    public void OnInitialized(object? sender, EventArgs e)
    {
        this.ModalText.Text = this.text;
        this.OKButton.AttachedToVisualTree += (s, e) => this.OKButton.Focus();
    }

    public void ClickHandler(object sender, RoutedEventArgs args)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;
        topLevel.Close(0);
    }
}
