using System;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;

using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

using ReactiveUI;

using EVTUI.ViewModels;

namespace EVTUI.Views;

public partial class ScriptPanel : ReactiveUserControl<ScriptPanelViewModel>
{
    private Window topLevel;
    private static RegistryOptions _registryOptions = new RegistryOptions(ThemeName.Monokai);

    private TextEditor            _msgTextEditor;
    private TextMate.Installation _msgTextMateInstallation;
    private static string _jsonGrammar = _registryOptions.GetScopeByLanguageId("json");

    private TextEditor            _flowTextEditor;
    private TextMate.Installation _flowTextMateInstallation;
    private static string _cppGrammar = _registryOptions.GetScopeByLanguageId("cpp");

    private IDisposable _subscription;

    public ScriptPanel()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            var tl = TopLevel.GetTopLevel(this);
            if (tl is null) throw new NullReferenceException();
            this.topLevel = (Window)tl;

            if (_msgTextEditor is null || _flowTextEditor is null)
            {
                this.InitializeTextEditor();
                this.UpdateTextEditor();
            }
        });
    }

    public async void NameSelectionChanged(object source, SelectionChangedEventArgs e)
    {
        try
        {
            if (!(EditorContainer.Content is null))
            {
                ViewModel!.SelectedCompiledScriptName = (string)(CompiledName.SelectedItem);
                ViewModel!.UpdateSubfiles();
                if (!ViewModel!.HasDecompiledFiles)
                    this.UpdateTextEditor();
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to load script due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void ExtSelectionChanged(object source, SelectionChangedEventArgs e)
    {
        try
        {
            if (!(EditorContainer.Content is null))
                this.UpdateTextEditor();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to load file related to script due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public async void Compile(object source, RoutedEventArgs e)
    {
        try
        {
            ViewModel!.Compile();
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            await Utils.RaiseModal(this.topLevel, $"Failed to compile script due to unhandled exception:\n{ex.ToString()}");
        }
    }

    public void InitializeTextEditor()
    {
        _msgTextEditor = new TextEditor();
        // very goofy to have to do this here but I can't figure out the right XAML for it lolz
        _msgTextEditor.TextArea.TextView.Margin = new Thickness(0, 0, 20, 0);
        _msgTextEditor.IsReadOnly = !(ViewModel!.Editable);
        if (!(ViewModel!.Editable))
            _msgTextEditor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        _msgTextMateInstallation = _msgTextEditor.InstallTextMate(_registryOptions);
        _msgTextMateInstallation.SetGrammar(_jsonGrammar);
        _msgTextEditor.Document = new TextDocument("");

        _flowTextEditor = new TextEditor();
        // very goofy to have to do this here but I can't figure out the right XAML for it lolz
        _flowTextEditor.TextArea.TextView.Margin = new Thickness(0, 0, 20, 0);
        _flowTextEditor.IsReadOnly = !(ViewModel!.Editable);
        if (!(ViewModel!.Editable))
            _flowTextEditor.TextArea.Caret.CaretBrush = Brushes.Transparent;
        _flowTextMateInstallation = _flowTextEditor.InstallTextMate(_registryOptions);
        _flowTextMateInstallation.SetGrammar(_cppGrammar);
        _flowTextEditor.Document = new TextDocument("");
    }

    public void UpdateTextEditor()
    {
        if (!(_subscription is null))
            _subscription.Dispose();

        if (ViewModel!.SelectedDecompiledScriptName.EndsWith(".msg"))
        {
            _msgTextEditor.IsEnabled = ViewModel!.HasDecompiledFiles;
            _msgTextEditor.Document.Text = ViewModel!.SelectedScriptContent;
            EditorContainer.Content = _msgTextEditor;
            _subscription = this.WhenAnyValue(x => x._msgTextEditor.Document.Text).Subscribe(x => ViewModel!.SelectedScriptContent = x );
        }
        else
        {
            _flowTextEditor.IsEnabled = ViewModel!.HasDecompiledFiles;
            _flowTextEditor.Document.Text = ViewModel!.SelectedScriptContent;
            EditorContainer.Content = _flowTextEditor;
            _subscription = this.WhenAnyValue(x => x._flowTextEditor.Document.Text).Subscribe(x => ViewModel!.SelectedScriptContent = x );
        }
    }
}
