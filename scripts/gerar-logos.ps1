# Gera os logos do pacote MSIX em instalador\msix\Assets: um quadrado arredondado
# na cor da marca com um celular desenhado. Rodar de novo so' se o desenho mudar;
# os PNG ficam versionados.
Add-Type -AssemblyName System.Drawing
$destino = Join-Path (Split-Path -Parent $PSScriptRoot) "instalador\msix\Assets"
New-Item -ItemType Directory -Force $destino | Out-Null

function Desenhar([int]$lado, [string]$nome) {
  $bmp = New-Object Drawing.Bitmap $lado, $lado
  $g = [Drawing.Graphics]::FromImage($bmp)
  $g.SmoothingMode = "AntiAlias"
  $g.Clear([Drawing.Color]::Transparent)
  $fundo = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(88, 101, 242))
  $r = [Math]::Max(2, [int]($lado * 0.18))
  $caminho = New-Object Drawing.Drawing2D.GraphicsPath
  $caminho.AddArc(0, 0, 2 * $r, 2 * $r, 180, 90)
  $caminho.AddArc($lado - 2 * $r - 1, 0, 2 * $r, 2 * $r, 270, 90)
  $caminho.AddArc($lado - 2 * $r - 1, $lado - 2 * $r - 1, 2 * $r, 2 * $r, 0, 90)
  $caminho.AddArc(0, $lado - 2 * $r - 1, 2 * $r, 2 * $r, 90, 90)
  $caminho.CloseFigure()
  $g.FillPath($fundo, $caminho)
  # O celular: retangulo arredondado branco, em pe', com a tela escura.
  $l = $lado * 0.36; $a = $lado * 0.62
  $x = ($lado - $l) / 2; $y = ($lado - $a) / 2
  $branco = New-Object Drawing.SolidBrush ([Drawing.Color]::White)
  $g.FillRectangle($branco, [single]$x, [single]$y, [single]$l, [single]$a)
  $tela = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(17, 18, 20))
  $m = [Math]::Max(1, $lado * 0.035)
  $g.FillRectangle($tela, [single]($x + $m), [single]($y + 2 * $m), [single]($l - 2 * $m), [single]($a - 4 * $m))
  $g.Dispose()
  $bmp.Save((Join-Path $destino $nome), [Drawing.Imaging.ImageFormat]::Png)
  $bmp.Dispose()
}

Desenhar 50 "StoreLogo.png"
Desenhar 44 "Square44x44Logo.png"
Desenhar 150 "Square150x150Logo.png"
Write-Output "logos em $destino"
