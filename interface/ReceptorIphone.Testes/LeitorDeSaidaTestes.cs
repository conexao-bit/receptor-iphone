using ReceptorIphone;

namespace ReceptorIphone.Testes;

// As linhas sao as que o uxplay 1.74 imprimiu no teste de 2026-11-26.
public class LeitorDeSaidaTestes
{
    [Fact]
    public void LeOCodigo() =>
        Assert.Equal(new CodigoPedido("6356"),
            LeitorDeSaida.Ler("*** CLIENT MUST NOW ENTER PIN = \"6356\" AS AIRPLAY PASSWORD"));

    [Fact]
    public void LeOAparelho() =>
        Assert.Equal(new Conectando("iPhone de Ana (iPhone14,8)"),
            LeitorDeSaida.Ler("connection request from iPhone de Ana (iPhone14,8) with deviceID = 96:D8:4E:72:C5:4D"));

    [Fact]
    public void LeOInicio() =>
        Assert.IsType<Espelhando>(LeitorDeSaida.Ler("raop_rtp_mirror starting mirroring"));

    [Fact]
    public void LeAGeometria() =>
        Assert.Equal(new Geometria(500, 1080),
            LeitorDeSaida.Ler("*** codec is h264: geometry is (w,h)=(500,1080) (source); (x,y)=(710,0); (w,h)=(500,1080)"));

    [Fact]
    public void LeAQueda() =>
        Assert.IsType<Desconectou>(LeitorDeSaida.Ler("Connection closed on socket 4824"));

    [Fact]
    public void IgnoraOResto() =>
        Assert.Null(LeitorDeSaida.Ler("Initialized server socket(s)"));
}
