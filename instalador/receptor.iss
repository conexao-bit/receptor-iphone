; Instalador do Receptor iPhone: por usuario, sem administrador, sem nada
; que inicie com o Windows. Compilar com o Inno Setup 6 (iscc) depois de
; montar dist\pacote (uxplay + licencas + ReceptorIphone.exe).

#ifndef Versao
  #define Versao "0.0.0"
#endif

[Setup]
AppId={{7B0D6F1A-4C2E-4B8B-9E36-0C5A2B7E1F42}
AppName=Receptor de Tela Conexão Tech
AppVersion={#Versao}
AppPublisher=Conexão Tech
AppPublisherURL=https://github.com/conexao-bit/receptor-iphone
DefaultDirName={localappdata}\Programs\Receptor iPhone
DisableDirPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=
OutputDir=..\dist
OutputBaseFilename=ReceptorIphone-Instalador
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName=Receptor de Tela Conexão Tech
UninstallDisplayIcon={app}\ReceptorIphone.exe
LicenseFile=..\LICENSE

[Languages]
Name: "ptbr"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "..\dist\pacote\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"
Source: "..\THIRD_PARTY.md"; DestDir: "{app}"

[Icons]
Name: "{userprograms}\Receptor de Tela Conexão Tech"; Filename: "{app}\ReceptorIphone.exe"

[Run]
Filename: "{app}\ReceptorIphone.exe"; Description: "Abrir o Receptor de Tela Conexão Tech"; Flags: nowait postinstall skipifsilent

[InstallDelete]
; Atalho com o nome antigo (ate' a v0.1.0-rc2). As pastas ficam com o nome
; antigo de proposito: atualizar por cima continua no mesmo lugar.
Type: files; Name: "{userprograms}\Receptor iPhone.lnk"

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\Receptor iPhone"
