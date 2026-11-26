# Abre o ReceptorIphone.exe publicado (dist\pacote), espera a janela "Esperando o
# iPhone" e salva capturas para a pagina da Microsoft Store em dist\capturas: a
# janela como esta' e ela centralizada numa tela 1920x1080 (a Store pede
# capturas de computador de pelo menos 1366x768). Roda no CI (workflow capturas).
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System; using System.Runtime.InteropServices;
public static class Janela {
  [StructLayout(LayoutKind.Sequential)] public struct R { public int L, T, Rt, B; }
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr h, IntPtr dc, uint flags);
}
"@
$raiz = Split-Path -Parent $PSScriptRoot
$saida = Join-Path $raiz "dist\capturas"
New-Item -ItemType Directory -Force $saida | Out-Null

$p = Start-Process (Join-Path $raiz "dist\pacote\ReceptorIphone.exe") -PassThru
$h = [IntPtr]::Zero
for ($i = 0; $i -lt 60 -and $h -eq [IntPtr]::Zero; $i++) {
  Start-Sleep -Milliseconds 500
  $p.Refresh()
  $h = $p.MainWindowHandle
}
if ($h -eq [IntPtr]::Zero) { throw "a janela do receptor nao apareceu" }
Start-Sleep -Seconds 4
[void][Janela]::SetForegroundWindow($h)
Start-Sleep -Seconds 1

$r = New-Object Janela+R
[void][Janela]::GetWindowRect($h, [ref]$r)
$l = $r.Rt - $r.L; $a = $r.B - $r.T
$bmp = New-Object Drawing.Bitmap $l, $a
$g = [Drawing.Graphics]::FromImage($bmp)
$dc = $g.GetHdc()
# PW_RENDERFULLCONTENT (2): desenha a janela mesmo sem estar na frente.
[void][Janela]::PrintWindow($h, $dc, 2)
$g.ReleaseHdc($dc); $g.Dispose()
$bmp.Save((Join-Path $saida "janela.png"), [Drawing.Imaging.ImageFormat]::Png)

$tela = New-Object Drawing.Bitmap 1920, 1080
$g = [Drawing.Graphics]::FromImage($tela)
$g.Clear([Drawing.Color]::FromArgb(30, 31, 34))
$escala = [Math]::Min(1.0, 980.0 / $a)
$nl = [int]($l * $escala); $na = [int]($a * $escala)
$g.InterpolationMode = "HighQualityBicubic"
$g.DrawImage($bmp, [int]((1920 - $nl) / 2), [int]((1080 - $na) / 2), $nl, $na)
$g.Dispose()
$tela.Save((Join-Path $saida "loja-1920x1080.png"), [Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose(); $tela.Dispose()

Stop-Process -Id $p.Id -Force
Get-Process uxplay -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Output "capturas: janela ${l}x${a}, loja 1920x1080 em $saida"
