using ReceptorIphone;

namespace ReceptorIphone.Testes;

public class EstadoJanelaTestes
{
    [Fact]
    public void CodigoPedidoMostraOCodigo() =>
        Assert.Equal(new MostrandoCodigo("1405"),
            EstadoJanela.Proximo(new Esperando(), new CodigoPedido("1405")));

    // No teste de 2026-11-26 o uxplay imprimiu "Connection closed on socket"
    // logo depois do PIN: o iPhone reconecta para mandar o codigo. O codigo
    // nao pode sumir enquanto a pessoa digita.
    [Fact]
    public void QuedaDuranteOCodigoMantemOCodigo() =>
        Assert.Equal(new MostrandoCodigo("1405"),
            EstadoJanela.Proximo(new MostrandoCodigo("1405"), new Desconectou()));

    [Fact]
    public void EspelhandoEscondeOCodigo() =>
        Assert.IsType<Transmitindo>(
            EstadoJanela.Proximo(new MostrandoCodigo("1405"), new Espelhando()));

    [Fact]
    public void QuedaDepoisDeEspelharVoltaAEsperar() =>
        Assert.IsType<Esperando>(
            EstadoJanela.Proximo(new Transmitindo(), new Desconectou()));

    [Fact]
    public void ConectandoEGeometriaNaoMudamOEstado()
    {
        Assert.Equal(new MostrandoCodigo("1"),
            EstadoJanela.Proximo(new MostrandoCodigo("1"), new Conectando("iPhone")));
        Assert.IsType<Transmitindo>(
            EstadoJanela.Proximo(new Transmitindo(), new Geometria(500, 1080)));
    }
}
