using System.Diagnostics;
using ReceptorIphone;

namespace ReceptorIphone.Testes;

public class ProcessoUxplayTestes
{
    private static readonly string Cmd =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");

    private static readonly Dictionary<string, string> SemAmbiente = new();

    [Fact]
    public void FecharMataOFilho()
    {
        var p = new ProcessoUxplay(Cmd, ["/c", "ping", "-n", "30", "127.0.0.1"], SemAmbiente);
        p.Iniciar();
        var pid = p.Pid;
        p.Dispose();

        try
        {
            using var filho = Process.GetProcessById(pid);
            Assert.True(filho.WaitForExit(2000), "o filho continuou vivo");
        }
        catch (ArgumentException) { } // ja' saiu
    }

    [Fact]
    public void FecharMataOsNetos()
    {
        // O ping e' neto (filho do cmd): o Job Object leva todos.
        var p = new ProcessoUxplay(Cmd, ["/c", "ping", "-n", "30", "127.0.0.1"], SemAmbiente);
        p.Iniciar();
        Thread.Sleep(1000);
        var netos = FilhosDe(p.Pid);
        Assert.NotEmpty(netos);

        p.Dispose();

        foreach (var neto in netos)
        {
            try
            {
                using var vivo = Process.GetProcessById(neto);
                Assert.True(vivo.WaitForExit(2000), $"neto {neto} continuou vivo");
            }
            catch (ArgumentException) { } // ja' saiu
        }
    }

    [Fact]
    public void RepassaEventos()
    {
        var recebidos = new List<Evento>();
        using var saiu = new ManualResetEventSlim();
        using var p = new ProcessoUxplay(Cmd, ["/c", "echo raop_rtp_mirror starting mirroring"], SemAmbiente);
        p.AoEvento += e => { lock (recebidos) recebidos.Add(e); };
        p.AoSair += _ => saiu.Set();
        p.Iniciar();

        Assert.True(saiu.Wait(5000));
        lock (recebidos) Assert.Contains(recebidos, e => e is Espelhando);
    }

    [Fact]
    public void AvisaQuandoSai()
    {
        var codigo = -1;
        using var saiu = new ManualResetEventSlim();
        using var p = new ProcessoUxplay(Cmd, ["/c", "exit 3"], SemAmbiente);
        p.AoSair += c => { codigo = c; saiu.Set(); };
        p.Iniciar();

        Assert.True(saiu.Wait(5000));
        Assert.Equal(3, codigo);
    }

    private static List<int> FilhosDe(int pai)
    {
        using var q = new System.Management.ManagementObjectSearcher(
            $"SELECT ProcessId FROM Win32_Process WHERE ParentProcessId = {pai}");
        return q.Get().Cast<System.Management.ManagementObject>()
            .Select(o => Convert.ToInt32(o["ProcessId"]))
            .ToList();
    }
}
