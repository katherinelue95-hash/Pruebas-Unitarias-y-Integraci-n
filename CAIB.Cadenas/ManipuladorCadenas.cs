using System;
using System.Linq;

namespace CAIB.Cadenas;

public class ManipuladorCadenas
{
    public string Invertir(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto;
        char[] arreglo = texto.ToCharArray();
        Array.Reverse(arreglo);
        return new string(arreglo);
    }

    public int ContarVocales(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return 0;
        return texto.Count(c => "aeiouAEIOU".Contains(c));
    }
}