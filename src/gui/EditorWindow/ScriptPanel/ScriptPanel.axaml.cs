using System;

using Avalonia.Controls;
using Avalonia.Interactivity;
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
    private ContentControl _editorContainer;
    private TextEditor _textEditor;
    private RegistryOptions _registryOptions;
    private TextMate.Installation _textMateInstallation;

    private ComboBox _nameBox;
    private ComboBox _extBox;

    public ScriptPanel()
    {
        InitializeComponent();
        this.WhenActivated(d =>
        {
            _editorContainer = this.FindControl<ContentControl>("EditorContainer");
            _registryOptions = new RegistryOptions(ThemeName.Monokai);
            _nameBox = this.FindControl<ComboBox>("CompiledName");
            _extBox = this.FindControl<ComboBox>("DecompiledExt");
            _nameBox.SelectionChanged += NameSelectionChanged;
            _extBox.SelectionChanged += ExtSelectionChanged;
            this.UpdateTextEditor();
        });
    }

    public void NameSelectionChanged(object source, SelectionChangedEventArgs e)
    {
        ViewModel!.SelectedCompiledScriptName = (string)(_nameBox.SelectedItem);
        ViewModel!.UpdateSubfiles();
        if (!ViewModel!.HasDecompiledFiles)
            this.UpdateTextEditor();
    }

    public void ExtSelectionChanged(object source, SelectionChangedEventArgs e)
    {
        this.UpdateTextEditor();
    }

    public void Compile(object source, RoutedEventArgs e)
    {
        ViewModel!.Compile();
    }

    public void UpdateTextEditor()
    {
        _textEditor = new TextEditor();
        _textEditor.IsReadOnly = ViewModel!.ReadOnly;
        _textEditor.IsEnabled = ViewModel!.HasDecompiledFiles;
        _textEditor.Document = new TextDocument(ViewModel!.SelectedScriptContent);
        this.WhenAnyValue(x => x._textEditor.Document.Text).Subscribe(x => ViewModel!.SelectedScriptContent = x );
        _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);
        if (ViewModel!.HasDecompiledFiles)
        {
            if (ViewModel!.SelectedDecompiledScriptName.EndsWith(".msg"))
                _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("json"));
            else
                _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("cpp"));
        }
        _editorContainer.Content = _textEditor;
    }
}
