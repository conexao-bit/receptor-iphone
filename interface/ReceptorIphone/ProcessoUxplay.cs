using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ReceptorIphone;

/// <summary>
/// O uxplay.exe rodando oculto. Fica num Job Object com KILL_ON_JOB_CLOSE:
/// se a interface fechar, cair ou for encerrada pelo Gerenciador de Tarefas,
/// o Windows fecha o handle do job e leva o uxplay (e o que ele abriu) junto,
/// para nada ficar escutando na rede sem janela.
/// </summary>
public sealed partial class ProcessoUxplay : IDisposable
{
    private readonly ProcessStartInfo _inicio;
    private Process? _processo;
    private IntPtr _job;
    private bool _encerrado;

    public event Action<Evento>? AoEvento;
    public event Action<string>? AoLinha;
    public event Action<int>? AoSair;

    public int Pid => _processo?.Id ?? 0;

    public ProcessoUxplay(string exe, IReadOnlyList<string> args, IDictionary<string, string> ambiente)
    {
        _inicio = new ProcessStartInfo(exe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        foreach (var a in args) _inicio.ArgumentList.Add(a);
        foreach (var (k, v) in ambiente) _inicio.Environment[k] = v;
    }

    public void Iniciar()
    {
        _job = CriarJob();
        var p = new Process { StartInfo = _inicio, EnableRaisingEvents = true };
        p.OutputDataReceived += (_, e) => Tratar(e.Data);
        p.ErrorDataReceived += (_, e) => Tratar(e.Data);
        p.Exited += (_, _) =>
        {
            // Espera o fim das leituras antes de avisar a saida.
            p.WaitForExit();
            if (!_encerrado) AoSair?.Invoke(p.ExitCode);
        };
        p.Start();
        if (!AssignProcessToJobObject(_job, p.Handle))
            throw new Win32Exception(Marshal.GetLastWin32Error());
        _processo = p;
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
    }

    private void Tratar(string? linha)
    {
        if (linha is null) return;
        AoLinha?.Invoke(linha);
        if (LeitorDeSaida.Ler(linha) is { } e) AoEvento?.Invoke(e);
    }

    public void Dispose()
    {
        _encerrado = true;
        try
        {
            if (_processo is { HasExited: false } p) p.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) { } // saiu no meio
        if (_job != IntPtr.Zero)
        {
            CloseHandle(_job);
            _job = IntPtr.Zero;
        }
        _processo?.Dispose();
    }

    private static IntPtr CriarJob()
    {
        var job = CreateJobObject(IntPtr.Zero, null);
        if (job == IntPtr.Zero) throw new Win32Exception(Marshal.GetLastWin32Error());
        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = { LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE },
        };
        var tamanho = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
        var ptr = Marshal.AllocHGlobal(tamanho);
        try
        {
            Marshal.StructureToPtr(info, ptr, false);
            if (!SetInformationJobObject(job, JobObjectExtendedLimitInformation, ptr, (uint)tamanho))
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return job;
    }

    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;
    private const int JobObjectExtendedLimitInformation = 9;

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public UIntPtr Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount;
        public ulong ReadTransferCount, WriteTransferCount, OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "CreateJobObjectW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr CreateJobObject(IntPtr atributos, string? nome);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetInformationJobObject(IntPtr job, int classe, IntPtr info, uint tamanho);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AssignProcessToJobObject(IntPtr job, IntPtr processo);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr handle);
}
