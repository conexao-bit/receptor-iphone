# Monta dist\ReceptorDeTela.msix (sem assinatura: a Microsoft Store assina) a
# partir de dist\pacote, com o manifesto de instalador\msix. A identidade vem do
# Partner Center (no CI, variaveis do repositorio) e nunca e' inventada.
# Uso: scripts\montar-msix.ps1 -Identidade <Name> -Publicador "CN=..." -NomePublicador "Conexao Tech" -Versao 0.1.1.0
param(
  [string]$Identidade,
  [string]$Publicador,
  [string]$NomePublicador,
  [string]$Versao
)
$ErrorActionPreference = "Stop"

foreach ($p in "Identidade", "Publicador", "NomePublicador", "Versao") {
  if ([string]::IsNullOrWhiteSpace((Get-Variable $p -ValueOnly))) {
    Write-Output "montar-msix: falta -$p (no CI, as variaveis do repositorio RECEPTOR_* do Partner Center)"
    exit 1
  }
}
# A Store exige a quarta parte 0.
if ($Versao -notmatch '^\d+\.\d+\.\d+\.0$') {
  Write-Output "montar-msix: versao '$Versao' fora do formato N.N.N.0"
  exit 1
}

$raiz = Split-Path -Parent $PSScriptRoot
$pacote = Join-Path $raiz "dist\pacote"
if (-not (Test-Path (Join-Path $pacote "ReceptorIphone.exe"))) {
  Write-Output "montar-msix: falta dist\pacote\ReceptorIphone.exe (rodar o dotnet publish antes)"
  exit 1
}

$msix = Join-Path $raiz "dist\msix"
if (Test-Path $msix) { Remove-Item -Recurse -Force $msix }
Copy-Item -Recurse $pacote $msix
Get-ChildItem $msix -Filter *.pdb | Remove-Item
Copy-Item (Join-Path $raiz "LICENSE") $msix
Copy-Item (Join-Path $raiz "THIRD_PARTY.md") $msix
Copy-Item -Recurse (Join-Path $raiz "instalador\msix\Assets") (Join-Path $msix "Assets")

$esc = { param($v) [Security.SecurityElement]::Escape($v) }
$manifesto = [IO.File]::ReadAllText((Join-Path $raiz "instalador\msix\AppxManifest.xml"))
$manifesto = $manifesto.Replace("{{IDENTIDADE}}", (& $esc $Identidade)).
  Replace("{{PUBLICADOR}}", (& $esc $Publicador)).
  Replace("{{NOME_PUBLICADOR}}", (& $esc $NomePublicador)).
  Replace("{{VERSAO}}", $Versao)
[IO.File]::WriteAllText((Join-Path $msix "AppxManifest.xml"), $manifesto, (New-Object Text.UTF8Encoding $false))

$makeappx = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin\*\x64\makeappx.exe" -ErrorAction SilentlyContinue |
  Sort-Object FullName -Descending | Select-Object -First 1
if (-not $makeappx) {
  Write-Output "montar-msix: makeappx.exe nao encontrado (Windows SDK)"
  exit 1
}
$saida = Join-Path $raiz "dist\ReceptorDeTela.msix"
& $makeappx.FullName pack /o /d $msix /p $saida
if ($LASTEXITCODE -ne 0) { Write-Output "montar-msix: makeappx falhou ($LASTEXITCODE)"; exit 1 }
Write-Output "msix: $saida ($([Math]::Round((Get-Item $saida).Length / 1MB)) MB)"
