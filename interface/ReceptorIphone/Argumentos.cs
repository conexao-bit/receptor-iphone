using System.Globalization;

namespace ReceptorIphone;

/// <summary>Como o uxplay.exe e' iniciado.</summary>
public static class Argumentos
{
    /// <summary>Nome que aparece no "Espelhar Tela" do iPhone.</summary>
    public static string NomeNoIphone(string computador) => $"Conexão Tech ({computador})";

    /// <summary>
    /// MAC novo a cada abertura: o servidor ganha outra identidade e o iPhone
    /// tem de digitar o codigo de novo (nada de pareamento lembrado).
    /// Administrado localmente e unicast (primeiro byte xxxxxx10).
    /// </summary>
    public static string MacAleatorio(Random r)
    {
        var b = new byte[6];
        r.NextBytes(b);
        b[0] = (byte)((b[0] & 0xFC) | 0x02);
        return string.Join(":", b.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
    }

    /// <summary>Sem -hwnd: a janela de video e' do uxplay e a interface a encaixa (EncaixeDoVideo).</summary>
    public static IReadOnlyList<string> Montar(string nome, string mac) =>
    [
        "-n", nome, "-nh",
        "-pin",
        "-m", mac,
        "-as", "0",
        "-s", "1920x1920",
    ];
}
