using System.Threading.Tasks;
using Avalonia.Controls;

namespace EVTUI.Views;

public partial class Utils
{

    public static async Task<int> RaiseModal(Window tl, string text)
    {
        Window sampleWindow =
            new Window 
            { 
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
        sampleWindow.Content = new MessageBox(text);

        // Launch window and get a return code to distinguish how the window
        // was closed.
        int? res = await sampleWindow.ShowDialog<int?>(tl);
        if (res is null)
            return 1;
        else
            return (int)res;
    }

    public static async Task<int> RaiseDoubleCheck(Window tl, string mainMsg="Are you sure?", string yesMsg="Yes", string noMsg="No")
    {
        Window sampleWindow =
            new Window 
            { 
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
        sampleWindow.Content = new SafetyBox(mainMsg, yesMsg, noMsg);

        // Launch window and get a return code to distinguish how the window
        // was closed.
        int? res = await sampleWindow.ShowDialog<int?>(tl);
        if (res is null)
            return 1;
        else
            return (int)res;
    }
}
