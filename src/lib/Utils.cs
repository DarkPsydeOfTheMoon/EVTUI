using System.Security.Cryptography;
using System.Text;

namespace EVTUI;

public static class Utils
{
    // credit: https://www.techiedelight.com/generate-md5-hash-of-string-csharp/
    public static string Hashify(string s)
    {
        StringBuilder sb = new StringBuilder();
        using (MD5 md5 = MD5.Create())
        {
            foreach (byte b in md5.ComputeHash(Encoding.UTF8.GetBytes(s)))
                sb.Append($"{b:X2}");
        }
        return sb.ToString();
    }
}
