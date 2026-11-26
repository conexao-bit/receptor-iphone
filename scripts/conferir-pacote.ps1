# Confere que dist\pacote\uxplay se basta: decodifica H.264 pelo mesmo caminho
# do uxplay (h264parse ! decodebin ! videoconvert ! videoscale) usando so' as
# DLLs e os plugins do pacote, sem o MSYS2 no PATH.
# Uso: powershell -File scripts\conferir-pacote.ps1 [-Msys2 C:\msys64]
param([string]$Msys2 = "C:\msys64")
$ErrorActionPreference = "Stop"

$raiz = Split-Path -Parent $PSScriptRoot
$teste = Join-Path $raiz "dist\teste-pacote"
if (Test-Path $teste) { Remove-Item -Recurse -Force $teste }
Copy-Item -Recurse (Join-Path $raiz "dist\pacote\uxplay") $teste
# O gst-launch so' serve ao teste; nao vai no instalador.
Copy-Item "$Msys2\ucrt64\bin\gst-launch-1.0.exe" $teste

$amostra = Join-Path $teste "amostra.h264"
& "$Msys2\ucrt64\bin\ffmpeg.exe" -loglevel error -y -f lavfi -i "testsrc=size=590x1280:rate=30" -t 2 `
  -c:v libx264 -profile:v high -pix_fmt yuv420p $amostra
if ($LASTEXITCODE -ne 0) { throw "ffmpeg falhou" }

function Testar([string]$decodificador) {
  $inicio = New-Object System.Diagnostics.ProcessStartInfo (Join-Path $teste "gst-launch-1.0.exe")
  $inicio.UseShellExecute = $false
  $inicio.RedirectStandardError = $true
  $inicio.Environment["PATH"] = "$teste;$env:SystemRoot\System32;$env:SystemRoot"
  $inicio.Environment["GST_PLUGIN_SYSTEM_PATH"] = ""
  $inicio.Environment["GST_PLUGIN_PATH"] = Join-Path $teste "lib\gstreamer-1.0"
  $inicio.Environment["GST_REGISTRY"] = Join-Path $teste "registry.bin"
  $inicio.Environment["GST_DEBUG"] = "2"
  # Arguments (string) e nao ArgumentList: o PowerShell 5.1 roda no .NET Framework.
  # Barras normais: o gst-launch trata a invertida como escape.
  $inicio.Arguments = "-q filesrc location=`"$($amostra -replace '\\', '/')`" ! h264parse ! $decodificador ! videoconvert ! videoscale ! fakesink sync=false"
  $p = [System.Diagnostics.Process]::Start($inicio)
  $erros = $p.StandardError.ReadToEnd()
  $p.WaitForExit()
  $faltando = $erros -split "`n" | Where-Object { $_ -match "no element|missing|not found|could not" }
  if ($p.ExitCode -ne 0 -or $faltando) {
    Write-Output $erros
    throw "o pacote nao se basta com $decodificador (saida $($p.ExitCode))"
  }
  Write-Output "ok: $decodificador decodificou H.264 (High) so' com o pacote"
}

# O caminho do uxplay (decodebin escolhe o de hardware se houver) e a reserva
# por software, sozinha: maquina sem decodificacao por hardware (maquina virtual,
# o CI) depende dela.
Testar "decodebin"
Testar "openh264dec"
