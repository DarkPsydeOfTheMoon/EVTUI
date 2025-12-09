using System;
using System.Diagnostics;
using System.Globalization;

using ImageMagick;

using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace EVTUI.Views;

public class ToBitmap : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            MagickImage img = (MagickImage)value;
            WriteableBitmap bmp = img.ToWriteableBitmap();
            return bmp;
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.ToString());
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
