using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Avalonia.Interactivity;

using System;

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
        var modalText = this.FindControl<TextBlock>("ModalText");
        if (modalText is null) throw new NullReferenceException();
        modalText.Text = this.text;
    }

    public void ClickHandler(object sender, RoutedEventArgs args)
    {
        var tl = TopLevel.GetTopLevel(this);
        if (tl is null) throw new NullReferenceException();
        var topLevel = (Window)tl;
        topLevel.Close(0);
    }
}
