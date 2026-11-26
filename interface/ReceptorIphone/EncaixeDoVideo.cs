using System.Runtime.InteropServices;
using System.Text;

namespace ReceptorIphone;

/// <summary>
/// Poe a janela de video do uxplay dentro da nossa. O -hwnd nao serve: para
/// desenhar numa janela dada, o d3d11videosink precisa trocar o procedimento
/// dela, e o Windows nao deixa um processo fazer isso com a janela de outro
/// (o log mostra "original window procedure 0000000000000000" e o video fica
/// preto). Entao o uxplay cria a janela dele (classe GSTD3D11) e a interface a
/// adota com SetParent, como filha do painel de video, e acompanha o tamanho.
/// </summary>
public static class EncaixeDoVideo
{
    public const string ClasseDoGStreamer = "GSTD3D11";

    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    private const long WS_CHILD = 0x40000000L;
    private const long WS_VISIBLE = 0x10000000L;
    private const long WS_POPUP = 0x80000000L;
    private const long WS_CAPTION = 0x00C00000L;
    private const long WS_THICKFRAME = 0x00040000L;
    private const long WS_SYSMENU = 0x00080000L;
    private const long WS_MINIMIZEBOX = 0x00020000L;
    private const long WS_MAXIMIZEBOX = 0x00010000L;
    private const long WS_EX_APPWINDOW = 0x00040000L;
    private const long WS_EX_WINDOWEDGE = 0x00000100L;
    private const long WS_EX_DLGMODALFRAME = 0x00000001L;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_SHOWWINDOW = 0x0040;

    private delegate bool EnumWindowsProc(IntPtr janela, IntPtr dado);

    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc cb, IntPtr dado);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr janela, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassNameW(IntPtr janela, StringBuilder nome, int tamanho);
    [DllImport("user32.dll")] private static extern IntPtr GetWindowLongPtrW(IntPtr janela, int indice);
    [DllImport("user32.dll")] private static extern IntPtr SetWindowLongPtrW(IntPtr janela, int indice, IntPtr valor);
    [DllImport("user32.dll")] private static extern IntPtr SetParent(IntPtr janela, IntPtr pai);
    [DllImport("user32.dll")] private static extern IntPtr GetParent(IntPtr janela);
    [DllImport("user32.dll")] private static extern bool IsWindow(IntPtr janela);
    [DllImport("user32.dll")] private static extern bool SetWindowPos(IntPtr janela, IntPtr depois, int x, int y, int l, int a, uint flags);

    /// <summary>A primeira janela de topo do processo com essa classe, ou Zero.</summary>
    public static IntPtr Procurar(int pid, string classe)
    {
        var achada = IntPtr.Zero;
        var nome = new StringBuilder(256);
        EnumWindows((j, _) =>
        {
            GetWindowThreadProcessId(j, out var dono);
            if (dono != pid) return true;
            nome.Clear();
            GetClassNameW(j, nome, nome.Capacity);
            if (!string.Equals(nome.ToString(), classe, StringComparison.OrdinalIgnoreCase)) return true;
            achada = j;
            return false;
        }, IntPtr.Zero);
        return achada;
    }

    /// <summary>A janela continua existindo e ja' e' filha desse pai.</summary>
    public static bool Encaixada(IntPtr janela, IntPtr pai) =>
        janela != IntPtr.Zero && IsWindow(janela) && GetParent(janela) == pai;

    /// <summary>Tira titulo e borda, torna filha do pai e ocupa o tamanho dado.</summary>
    public static void Encaixar(IntPtr janela, IntPtr pai, Size tamanho)
    {
        var estilo = GetWindowLongPtrW(janela, GWL_STYLE).ToInt64();
        estilo &= ~(WS_POPUP | WS_CAPTION | WS_THICKFRAME | WS_SYSMENU | WS_MINIMIZEBOX | WS_MAXIMIZEBOX);
        estilo |= WS_CHILD | WS_VISIBLE;
        SetWindowLongPtrW(janela, GWL_STYLE, new IntPtr(estilo));
        var extra = GetWindowLongPtrW(janela, GWL_EXSTYLE).ToInt64();
        extra &= ~(WS_EX_APPWINDOW | WS_EX_WINDOWEDGE | WS_EX_DLGMODALFRAME);
        SetWindowLongPtrW(janela, GWL_EXSTYLE, new IntPtr(extra));
        SetParent(janela, pai);
        SetWindowPos(janela, IntPtr.Zero, 0, 0, tamanho.Width, tamanho.Height,
            SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED | SWP_SHOWWINDOW);
    }

    public static void Ajustar(IntPtr janela, Size tamanho) =>
        SetWindowPos(janela, IntPtr.Zero, 0, 0, tamanho.Width, tamanho.Height, SWP_NOZORDER | SWP_NOACTIVATE);
}
