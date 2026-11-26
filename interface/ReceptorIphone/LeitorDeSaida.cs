using System.Globalization;
using System.Text.RegularExpressions;

namespace ReceptorIphone;

/// <summary>O que a interface precisa saber do que o uxplay.exe imprime.</summary>
public abstract record Evento;

/// <summary>O iPhone pediu o codigo; mostrar na janela.</summary>
public sealed record CodigoPedido(string Codigo) : Evento;

/// <summary>Um aparelho comecou a conectar (nome e modelo).</summary>
public sealed record Conectando(string Aparelho) : Evento;

/// <summary>O video comecou a chegar.</summary>
public sealed record Espelhando : Evento;

/// <summary>Tamanho do video que o iPhone manda (em pe' ou deitado).</summary>
public sealed record Geometria(int Largura, int Altura) : Evento;

/// <summary>A conexao com o iPhone fechou.</summary>
public sealed record Desconectou : Evento;

/// <summary>
/// Le uma linha da saida do uxplay.exe. As expressoes seguem o texto do
/// UxPlay 1.74; mudar a versao do UxPlay exige conferir estas linhas.
/// </summary>
public static partial class LeitorDeSaida
{
    [GeneratedRegex("CLIENT MUST NOW ENTER PIN = \"(\\d+)\"")]
    private static partial Regex Codigo();

    [GeneratedRegex("^connection request from (.+) with deviceID = ")]
    private static partial Regex Aparelho();

    [GeneratedRegex("^raop_rtp_mirror starting mirroring")]
    private static partial Regex Inicio();

    [GeneratedRegex("geometry is \\(w,h\\)=\\((\\d+),(\\d+)\\)")]
    private static partial Regex Tamanho();

    [GeneratedRegex("^Connection closed on socket")]
    private static partial Regex Fechou();

    public static Evento? Ler(string linha)
    {
        if (Codigo().Match(linha) is { Success: true } c)
            return new CodigoPedido(c.Groups[1].Value);
        if (Aparelho().Match(linha) is { Success: true } a)
            return new Conectando(a.Groups[1].Value);
        if (Inicio().IsMatch(linha))
            return new Espelhando();
        if (Tamanho().Match(linha) is { Success: true } t)
            return new Geometria(
                int.Parse(t.Groups[1].Value, CultureInfo.InvariantCulture),
                int.Parse(t.Groups[2].Value, CultureInfo.InvariantCulture));
        if (Fechou().IsMatch(linha))
            return new Desconectou();
        return null;
    }
}
