namespace ReceptorIphone;

/// <summary>
/// A unica janela do receptor, do comeco ao fim: e' ela que a pessoa
/// compartilha na chamada. O uxplay desenha o video no painel _video (-hwnd);
/// por cima, enquanto nao ha' video, o passo a passo e o codigo. O iPhone
/// parar e voltar nao fecha a janela, entao o compartilhamento nao cai.
/// </summary>
public sealed class JanelaReceptor : Form
{
    public const string Titulo = "iPhone — Conexão Tech";

    private readonly Panel _video = new() { Dock = DockStyle.Fill, BackColor = Color.Black };
    private readonly Panel _aviso = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(17, 18, 20) };
    private readonly Label _titulo = Rotulo(16, FontStyle.Bold);
    private readonly Label _texto = Rotulo(12, FontStyle.Regular);
    private readonly Label _codigo = Rotulo(48, FontStyle.Bold);
    private readonly Button _ligar = new()
    {
        Text = "Ligar de novo",
        AutoSize = true,
        Visible = false,
        Font = new Font("Segoe UI", 12),
        ForeColor = Color.White,
        BackColor = Color.FromArgb(88, 101, 242),
        FlatStyle = FlatStyle.Flat,
        Padding = new Padding(12, 6, 12, 6),
    };

    private readonly string _nome = Argumentos.NomeNoIphone(Environment.MachineName);
    private ProcessoUxplay? _processo;
    private EstadoJanela _estado = new Esperando();
    private bool _parou;

    public JanelaReceptor()
    {
        Text = Titulo;
        BackColor = Color.Black;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = Medidas.AreaDoVideo(0, 0, AreaDeTrabalho());
        MinimumSize = new Size(240, 320);

        var coluna = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(24),
            AutoScroll = true,
        };
        coluna.Controls.AddRange([_titulo, _texto, _codigo, _ligar]);
        coluna.Resize += (_, _) =>
        {
            foreach (Control c in coluna.Controls)
                c.MaximumSize = new Size(Math.Max(100, coluna.ClientSize.Width - 48), 0);
        };
        _aviso.Controls.Add(coluna);

        Controls.Add(_video);
        Controls.Add(_aviso);
        _aviso.BringToFront();

        _ligar.Click += (_, _) => Ligar();
        Shown += (_, _) => Ligar();
        Mostrar();
    }

    private static Label Rotulo(float tamanho, FontStyle estilo) => new()
    {
        AutoSize = true,
        ForeColor = Color.White,
        Font = new Font("Segoe UI", tamanho, estilo),
        Margin = new Padding(0, 0, 0, 16),
    };

    private Size AreaDeTrabalho() => Screen.FromControl(this).WorkingArea.Size;

    private void Ligar()
    {
        _processo?.Dispose();
        _parou = false;
        _estado = new Esperando();
        Mostrar();

        var pasta = Path.GetDirectoryName(CaminhoDoUxplay())!;
        var plugins = Path.Combine(pasta, "lib", "gstreamer-1.0");
        var ambiente = new Dictionary<string, string>
        {
            // As DLLs moram ao lado do uxplay.exe.
            ["PATH"] = pasta + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"),
        };
        if (Directory.Exists(plugins))
        {
            // So' os plugins do pacote, nunca um GStreamer instalado na maquina.
            ambiente["GST_PLUGIN_PATH"] = plugins;
            ambiente["GST_PLUGIN_SYSTEM_PATH"] = "";
            ambiente["GST_REGISTRY"] = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Receptor iPhone", "registry.bin");
        }

        var args = Argumentos.Montar(_nome, Argumentos.MacAleatorio(Random.Shared), _video.Handle.ToInt64());
        var processo = new ProcessoUxplay(CaminhoDoUxplay(), args, ambiente);
        processo.AoEvento += e => NaJanela(() => Tratar(e));
        processo.AoSair += _ => NaJanela(() =>
        {
            if (!ReferenceEquals(processo, _processo)) return;
            _parou = true;
            Mostrar();
        });
        _processo = processo;
        try
        {
            processo.Iniciar();
        }
        catch (Exception)
        {
            _parou = true;
            Mostrar();
        }
    }

    /// <summary>uxplay\uxplay.exe ao lado do programa; RECEPTOR_UXPLAY no desenvolvimento.</summary>
    private static string CaminhoDoUxplay() =>
        Environment.GetEnvironmentVariable("RECEPTOR_UXPLAY")
        ?? Path.Combine(AppContext.BaseDirectory, "uxplay", "uxplay.exe");

    private void NaJanela(Action a)
    {
        if (IsDisposed) return;
        try { BeginInvoke(a); } catch (InvalidOperationException) { } // fechando
    }

    private void Tratar(Evento e)
    {
        if (e is Geometria g) AjustarTamanho(g.Largura, g.Altura);
        _estado = EstadoJanela.Proximo(_estado, e);
        Mostrar();
    }

    private void AjustarTamanho(int largura, int altura)
    {
        if (WindowState != FormWindowState.Normal) return;
        var centro = new Point(Left + Width / 2, Top + Height / 2);
        ClientSize = Medidas.AreaDoVideo(largura, altura, AreaDeTrabalho());
        Location = new Point(centro.X - Width / 2, centro.Y - Height / 2);
    }

    private void Mostrar()
    {
        if (_parou)
        {
            _titulo.Text = "O receptor parou.";
            _texto.Text = "Clique para ligar de novo. Se continuar parando, feche e abra o Receptor iPhone.";
            _codigo.Visible = false;
            _ligar.Visible = true;
            _aviso.Visible = true;
            return;
        }
        _ligar.Visible = false;
        switch (_estado)
        {
            case MostrandoCodigo c:
                _titulo.Text = "Digite no iPhone:";
                _texto.Text = "";
                _codigo.Text = c.Codigo;
                _codigo.Visible = true;
                _aviso.Visible = true;
                break;
            case Transmitindo:
                _aviso.Visible = false;
                break;
            default:
                _titulo.Text = "Esperando o iPhone";
                _texto.Text = $"No iPhone: Central de Controle > Espelhar Tela > {_nome}.\n\n" +
                              "O iPhone e este computador precisam estar na mesma rede.";
                _codigo.Visible = false;
                _aviso.Visible = true;
                break;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _processo?.Dispose();
        _processo = null;
        base.OnFormClosed(e);
    }
}
