using System.Drawing;

namespace ReceptorIphone;

public static class Medidas
{
    /// <summary>Fracao da area de trabalho que a janela pode ocupar em cada lado.</summary>
    private const double Teto = 0.85;

    /// <summary>
    /// Area do video na janela: a proporcao do que o iPhone manda, sem passar
    /// de 85% da area de trabalho em nenhum lado. Sem tamanho ainda, a
    /// proporcao de um iPhone em pe' (9 x 19,5).
    /// </summary>
    public static Size AreaDoVideo(int largura, int altura, Size areaDeTrabalho)
    {
        double l = largura > 0 ? largura : 9;
        double a = altura > 0 ? altura : 19.5;
        var escala = Math.Min(Teto * areaDeTrabalho.Width / l, Teto * areaDeTrabalho.Height / a);
        return new Size((int)Math.Round(l * escala), (int)Math.Round(a * escala));
    }
}
