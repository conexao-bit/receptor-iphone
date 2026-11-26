# Microsoft Store: textos do envio

Produto no Partner Center: **Receptor de Tela Conexão Tech**, Store ID
`9NSJ2S8H9BZB`. Identidade do pacote nas variáveis do repositório
(`RECEPTOR_IDENTIDADE`, `RECEPTOR_PUBLICADOR`, `RECEPTOR_NOME_PUBLICADOR`). O
MSIX sai como artefato `msix` do workflow `release`.

## Preços e disponibilidade

- Preço: gratuito.
- Visibilidade no primeiro envio: **público privado** (só as contas listadas
  num grupo de clientes), para testar instalado antes de abrir a todos.

## Propriedades

- Categoria: Utilitários e ferramentas (ou Produtividade).
- Política de privacidade:
  https://github.com/conexao-bit/receptor-iphone/blob/main/PRIVACIDADE.md
- Site: https://conexaotech.com/
- Contato de suporte: conexao@conexaotech.com

## Listagem (Português, Brasil)

**Descrição**

Mostre a tela do iPhone ou do iPad no computador, sem cabo, para compartilhar
numa chamada de vídeo.

O Receptor de Tela Conexão Tech recebe o espelhamento de tela do iPhone e do
iPad e o mostra numa janela do Windows. Na chamada, você compartilha essa janela
como compartilharia qualquer outra.

Como usar:
1. Abra o Receptor de Tela Conexão Tech.
2. No iPhone ou iPad: Central de Controle > Espelhar Tela > Conexão Tech (nome
   do computador).
3. Digite no iPhone o código que aparece na janela.
4. Na chamada, compartilhe a janela "iPhone — Conexão Tech".

O iPhone e o computador precisam estar na mesma rede. A tela vem sem áudio. O
código vale só enquanto o app está aberto: cada vez que ele abre, o iPhone
digita um código novo.

Código aberto (GPLv3), baseado no UxPlay:
https://github.com/conexao-bit/receptor-iphone

iPhone, iPad e AirPlay são marcas da Apple Inc. Este app não é afiliado à
Apple.

**Recursos do produto** (um por linha)

- Espelhamento da tela do iPhone e do iPad numa janela do PC
- Código de acesso mostrado na janela a cada abertura
- Janela pronta para compartilhar em chamadas de vídeo
- Gira junto com o iPhone
- Sem conta e sem coleta de dados

**Palavras-chave:** espelhar tela, iPhone, iPad, AirPlay, chamada,
compartilhar tela

**Capturas:** `loja-1920x1080.png` do artefato `capturas` (workflow
`capturas`).

## Notas para a certificação (em inglês, para os revisores)

This is a Win32 desktop app (.NET WinForms) packaged as MSIX. It launches a
bundled open-source AirPlay receiver (uxplay.exe, GPLv3, source in the
repository above) to receive screen mirroring from an iPhone or iPad on the same
local network and shows it in the app window, so the user can share that window
in a video call.

- runFullTrust: required because it is a desktop (Win32) app that starts the
  bundled receiver process.
- Firewall rules (desktop2:FirewallRules) for uxplay.exe: inbound TCP/UDP on the
  local network, needed for mDNS discovery and the mirroring stream.
- No account, no data collection, no audio.

How to test: put an iPhone or iPad on the same network as the PC, open the app,
then on the device go to Control Center > Screen Mirroring > "Conexão Tech
(<PC name>)" and enter the 4-digit code shown in the app window. The device
screen appears in the window. Without an iOS device, the app shows its waiting
screen ("Esperando o iPhone") with the instructions.
