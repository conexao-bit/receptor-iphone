using ReceptorIphone;

namespace ReceptorIphone.Testes;

public class ArgumentosTestes
{
    [Fact]
    public void NomeComOComputador() =>
        Assert.Equal("Conexão Tech (ALIENWARE-M15)", Argumentos.NomeNoIphone("ALIENWARE-M15"));

    [Fact]
    public void MacLocalEUnicast()
    {
        var mac = Argumentos.MacAleatorio(new Random(1));
        Assert.Matches("^([0-9a-f]{2}:){5}[0-9a-f]{2}$", mac);
        var primeiro = Convert.ToByte(mac[..2], 16);
        Assert.Equal(0x02, primeiro & 0x03); // administrado localmente, unicast
    }

    [Fact]
    public void MacMudaACadaAbertura() =>
        Assert.NotEqual(Argumentos.MacAleatorio(new Random(1)), Argumentos.MacAleatorio(new Random(2)));

    [Fact]
    public void MontaOsArgumentos() =>
        Assert.Equal(
            new[] { "-n", "Conexão Tech (PC)", "-nh", "-pin", "-m", "02:00:00:00:00:01",
                    "-as", "0", "-s", "1920x1920" },
            Argumentos.Montar("Conexão Tech (PC)", "02:00:00:00:00:01"));
}
