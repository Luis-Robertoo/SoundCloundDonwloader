using System.Text.RegularExpressions;

namespace SoundClound.API.Helpers;

public static class StringExtension
{
    public static string ToClean(this string valor)
    {
        valor = Regex.Replace(valor, "[^a-zA-Z0-9-]+", "-").Trim();

        return valor;
    }
}
