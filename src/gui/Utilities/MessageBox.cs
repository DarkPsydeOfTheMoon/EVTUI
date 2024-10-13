using System.Threading.Tasks;
using Avalonia.Controls;

namespace EVTUI.Views;

public partial class Utils
{

    // If we need to pop boxes like this from multiple places, we should add this to
    // a View base class or something.
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

}
