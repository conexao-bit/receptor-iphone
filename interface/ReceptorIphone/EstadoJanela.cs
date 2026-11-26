namespace ReceptorIphone;

/// <summary>O que a janela mostra por cima do video.</summary>
public abstract record EstadoJanela
{
    /// <summary>Estado seguinte depois de um evento do uxplay.</summary>
    public static EstadoJanela Proximo(EstadoJanela atual, Evento evento) => evento switch
    {
        CodigoPedido c => new MostrandoCodigo(c.Codigo),
        Espelhando => new Transmitindo(),
        // Depois do PIN o iPhone fecha e reabre a conexao para mandar o
        // codigo: so' a queda de quem ja' espelhava volta a esperar.
        Desconectou when atual is Transmitindo => new Esperando(),
        _ => atual,
    };
}

/// <summary>Esperando o iPhone: o nome e o caminho no iPhone.</summary>
public sealed record Esperando : EstadoJanela;

/// <summary>O iPhone pediu o codigo: ele em letra grande.</summary>
public sealed record MostrandoCodigo(string Codigo) : EstadoJanela;

/// <summary>O video do iPhone na janela, sem nada por cima.</summary>
public sealed record Transmitindo : EstadoJanela;
