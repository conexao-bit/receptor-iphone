using System.Drawing;
using ReceptorIphone;

namespace ReceptorIphone.Testes;

public class MedidasTestes
{
    private static readonly Size Area = new(1920, 1040);

    [Fact]
    public void EmPeOcupa85PorCentoDaAltura() =>
        Assert.Equal(new Size(409, 884), Medidas.AreaDoVideo(500, 1080, Area));

    [Fact]
    public void DeitadoOcupa85PorCentoDaLargura() =>
        Assert.Equal(new Size(1632, 748), Medidas.AreaDoVideo(1920, 880, Area));

    [Fact]
    public void SemTamanhoUsaAProporcaoDoIphone() =>
        Assert.Equal(new Size(408, 884), Medidas.AreaDoVideo(0, 0, Area));
}
