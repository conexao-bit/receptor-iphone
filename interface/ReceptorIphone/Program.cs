using System.Runtime.InteropServices;

namespace ReceptorIphone;

static partial class Program
{
    /// <summary>Uma instancia por usuario: duas brigariam pelas mesmas portas.</summary>
    private const string NomeDoMutex = "Local\\ReceptorIphone.ConexaoTech";

    [STAThread]
    static void Main()
    {
        using var unico = new Mutex(initiallyOwned: true, NomeDoMutex, out var primeiro);
        if (!primeiro)
        {
            TrazerParaAFrente();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new JanelaReceptor());
    }

    private static void TrazerParaAFrente()
    {
        var janela = FindWindow(null, JanelaReceptor.Titulo);
        if (janela == IntPtr.Zero) return;
        ShowWindow(janela, SW_RESTORE);
        SetForegroundWindow(janela);
    }

    private const int SW_RESTORE = 9;

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindow(string? classe, string titulo);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr janela, int comando);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr janela);
}
