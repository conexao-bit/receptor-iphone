using System.Runtime.InteropServices;
using ReceptorIphone;

namespace ReceptorIphone.Testes;

/// <summary>
/// O encaixe e' entre processos na vida real (a janela de video e' do uxplay);
/// aqui as duas janelas sao deste processo, o que basta para SetParent, estilo
/// e tamanho. O caso entre processos foi conferido com o iPhone.
/// </summary>
public class EncaixeDoVideoTestes
{
    private const int GWL_STYLE = -16;
    private const long WS_CHILD = 0x40000000L;
    private const long WS_CAPTION = 0x00C00000L;

    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr h);
    [DllImport("user32.dll")] private static extern IntPtr GetWindowLongPtrW(IntPtr h, int i);
    [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr h, out Rect r);
    [StructLayout(LayoutKind.Sequential)] private struct Rect { public int L, T, R, B; }

    /// <summary>Janela de topo com titulo e borda, como a do GStreamer, numa classe so' dela.</summary>
    private static NativeWindow JanelaSolta()
    {
        var j = new NativeWindow();
        j.CreateHandle(new CreateParams
        {
            ClassName = "STATIC",
            Caption = "Direct3D11 renderer",
            Style = unchecked((int)0x00CF0000), // WS_OVERLAPPEDWINDOW
            Width = 300,
            Height = 200,
        });
        return j;
    }

    [Fact]
    public void ProcuraPeloProcessoEPelaClasse()
    {
        var j = JanelaSolta();
        try
        {
            Assert.Equal(j.Handle, EncaixeDoVideo.Procurar(Environment.ProcessId, "Static"));
            Assert.Equal(IntPtr.Zero, EncaixeDoVideo.Procurar(Environment.ProcessId, EncaixeDoVideo.ClasseDoGStreamer));
        }
        finally { j.DestroyHandle(); }
    }

    [Fact]
    public void EncaixaComoFilhaDoPainelEOcupaTudo()
    {
        using var form = new Form { ClientSize = new Size(400, 700) };
        var painel = new Panel { Dock = DockStyle.Fill };
        form.Controls.Add(painel);
        _ = form.Handle;
        var j = JanelaSolta();
        try
        {
            EncaixeDoVideo.Encaixar(j.Handle, painel.Handle, painel.ClientSize);

            Assert.Equal(painel.Handle, GetParent(j.Handle));
            var estilo = GetWindowLongPtrW(j.Handle, GWL_STYLE).ToInt64();
            Assert.NotEqual(0, estilo & WS_CHILD);
            Assert.Equal(0, estilo & WS_CAPTION);
            GetClientRect(j.Handle, out var r);
            Assert.Equal(painel.ClientSize, new Size(r.R - r.L, r.B - r.T));
        }
        finally { j.DestroyHandle(); }
    }

    [Fact]
    public void AcompanhaOTamanhoDoPainel()
    {
        using var form = new Form { ClientSize = new Size(400, 700) };
        var painel = new Panel { Dock = DockStyle.Fill };
        form.Controls.Add(painel);
        _ = form.Handle;
        var j = JanelaSolta();
        try
        {
            EncaixeDoVideo.Encaixar(j.Handle, painel.Handle, painel.ClientSize);
            form.ClientSize = new Size(700, 400);
            EncaixeDoVideo.Ajustar(j.Handle, painel.ClientSize);

            GetClientRect(j.Handle, out var r);
            Assert.Equal(new Size(700, 400), new Size(r.R - r.L, r.B - r.T));
        }
        finally { j.DestroyHandle(); }
    }
}
