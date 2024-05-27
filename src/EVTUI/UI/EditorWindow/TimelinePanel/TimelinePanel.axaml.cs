using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using EVTUI.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EVTUI.Views;

public partial class TimelinePanel : ReactiveUserControl<TimelinePanelViewModel>
{

    public TimelinePanel()
    {
        InitializeComponent();

        this.WhenActivated(d =>
        {
            d(ViewModel!.UpdateTimelineInView.RegisterHandler(PopulateTimeline));
            ViewModel!.CallUpdateTimelineInView();
        });
    }

    public ICommand DoAMenu { get { return ReactiveCommand.CreateFromTask(async (Button parameter) =>
    //private void DoAMenu(Button parameter)
    {
        Console.WriteLine(parameter);
    //}
    });}}

    private async Task PopulateTimeline(InteractionContext<ViewableEvent, bool> interaction)
    {
        timeline.Children.Clear();
        StackPanel header = new StackPanel();
        header.Name = "headerrow";
        StackPanel content = new StackPanel();
        content.Name = "contentrow";
        for (int i=0; i<interaction.Input.Duration; i++)
        {
            ////StackPanel column = new StackPanel();
            //ListBox column = new ListBox();
            ItemsControl column = new ItemsControl();
            column.Classes.Add("column");
            TextBlock text = new TextBlock();
            text.Text = $"{i}";
            //column.Children.Add(text);
            column.ItemsSource = new List<object>([text]);
            header.Children.Add(column);

            //ListBox column2 = new ListBox();
            ItemsControl column2 = new ItemsControl();
            List<object> elems = new List<object>();
            ////StackPanel column2 = new StackPanel();
            column2.Classes.Add("column");
            //column2.AddHandler(StackPanel.PointerReleasedEvent, TryDrop, handledEventsToo: true);
            // sometimes commands have frames outside of the defined duration...
            // idk how the game handles that (worth checking later), but. we ignore those lol
            if (interaction.Input.Commands.ContainsKey(i))
            {
                foreach (ViewableCommand command in interaction.Input.Commands[i])
                {
                    Button spacer = new Button();
                    spacer.Classes.Add("space");
                    spacer.Content = new PathIcon();
                    //column2.Children.Add(spacer);
                    elems.Add(spacer);

                    Button button = new Button();
                    //Panel button = new Panel();
                    button.Classes.Add("command");

                    Flyout flyout = new Flyout();
                    StackPanel flyoutContent = new StackPanel();
                    flyoutContent.Classes.Add("form");
                    TextBlock flyoutTitle = new TextBlock();
                    flyoutTitle.Classes.Add("formtitle");
                    flyoutTitle.Text = command.LongName;
                    flyoutContent.Children.Add(flyoutTitle);
                    flyoutContent.Children.Add(new Separator());
                    if (command.StringSelectionFields.Keys.Count == 0 && command.IntSelectionFields.Keys.Count == 0 && command.BoolChoiceFields.Keys.Count == 0)
                    {
                        TextBlock placeholder = new TextBlock();
                        placeholder.Classes.Add("placeholder");
                        placeholder.Text = "(Not yet implemented.)";
                        flyoutContent.Children.Add(placeholder);
                    }
                    else
                    {
                        foreach (string fieldName in command.StringSelectionFields.Keys)
                        {
                            StackPanel field = new StackPanel();
                            field.Classes.Add("field");

                            TextBlock fieldTitle = new TextBlock();
                            fieldTitle.Classes.Add("fieldtitle");
                            fieldTitle.Text = fieldName;
                            field.Children.Add(fieldTitle);

                            ComboBox dropdown = new ComboBox();
                            dropdown.ItemsSource = command.StringSelectionFields[fieldName].Choices;
                            dropdown.SelectedItem = command.StringSelectionFields[fieldName].Choice;
                            field.Children.Add(dropdown);

                            flyoutContent.Children.Add(field);
                        }
                        foreach (string fieldName in command.IntSelectionFields.Keys)
                        {
                            StackPanel field = new StackPanel();
                            field.Classes.Add("field");

                            TextBlock fieldTitle = new TextBlock();
                            fieldTitle.Classes.Add("fieldtitle");
                            fieldTitle.Text = fieldName;
                            field.Children.Add(fieldTitle);

                            ComboBox dropdown = new ComboBox();
                            dropdown.ItemsSource = command.IntSelectionFields[fieldName].Choices;
                            dropdown.SelectedItem = command.IntSelectionFields[fieldName].Choice;
                            field.Children.Add(dropdown);

                            flyoutContent.Children.Add(field);
                        }
                        foreach (string fieldName in command.BoolChoiceFields.Keys)
                        {
                            StackPanel field = new StackPanel();
                            field.Classes.Add("field");

                            TextBlock fieldTitle = new TextBlock();
                            fieldTitle.Classes.Add("fieldtitle");
                            fieldTitle.Text = fieldName;
                            field.Children.Add(fieldTitle);

                            CheckBox check = new CheckBox();
                            check.IsChecked = command.BoolChoiceFields[fieldName].Value;
                            field.Children.Add(check);

                            flyoutContent.Children.Add(field);
                        }
                    }
                    flyoutContent.Children.Add(new Separator());
                    flyout.Content = flyoutContent;
                    button.Flyout = flyout;
                    //FlyoutBase.SetAttachedFlyout(button, flyout);

                    MenuItem copy = new MenuItem();
                    copy.Header = "Copy";
                    copy.Command = DoAMenu;
                    copy.CommandParameter = button;
                    //menu.Children.Add(copy);
                    //////////
                    MenuItem paste = new MenuItem();
                    paste.Header = "Paste";
                    paste.Command = DoAMenu;
                    paste.CommandParameter = button;
                    paste.IsEnabled = false;
                    //menu.Children.Add(paste);
                    //////////
                    MenuItem delete = new MenuItem();
                    delete.Header = "Delete";
                    delete.Command = DoAMenu;
                    delete.CommandParameter = button;
                    //menu.LogicalChildren.Add(delete);
                    //////////
                    ContextMenu menu = new ContextMenu();
                    menu.ItemsSource = new[] { copy, paste, delete };
                    button.ContextMenu = menu;

                    TextBlock text2 = new TextBlock();
                    text2.Text = command.Code;
                    button.Content = text2;
                    //button.Children.Add(text2);
                    //column2.Children.Add(button);
                    elems.Add(button);
                }
            }
            Button finalspacer = new Button();
            finalspacer.Classes.Add("space");
            finalspacer.Content = new PathIcon();
            //column2.Children.Add(finalspacer);
            elems.Add(finalspacer);
            column2.ItemsSource = elems;

            content.Children.Add(column2);
        }
        timeline.Children.Add(header);
        timeline.Children.Add(content);
        interaction.SetOutput(true);
    }

}
