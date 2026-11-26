# Receptor iPhone

Programa para Windows que recebe o espelhamento de tela de um iPhone ou iPad
(AirPlay) numa janela, para essa janela ser compartilhada numa chamada da
plataforma da Conexão Tech.

O iPhone espelha no PC, o PC mostra a tela do iPhone na janela "iPhone —
Conexão Tech", e quem está na chamada compartilha essa janela pelo
compartilhamento de tela de sempre. Só funciona com o iPhone e o PC na mesma
rede.

## Origem e licença

O receptor é um fork do [UxPlay](https://github.com/FDH2/UxPlay), cravado na
versão 1.74 (commit `566f3460e3cbb16c2c33a7ac607d3d2d0dd09b14`), que mora em
`uxplay/` como subtree. As mudanças nossas estão no histórico deste
repositório.

Todo o repositório, inclusive a interface, é distribuído sob a
[GPLv3](LICENSE). As bibliotecas que vão junto no instalador, com as
licenças delas, estão em `THIRD_PARTY.md`.

Este projeto não é afiliado à Apple. AirPlay, iPhone e iPad são marcas da
Apple Inc. O suporte ao AirPlay vem do UxPlay, que o implementa por
engenharia reversa: uma versão nova do iOS pode quebrar o espelhamento até o
UxPlay acompanhar.

## Compilar

Num shell MSYS2 UCRT64 (instale o [MSYS2](https://www.msys2.org)):

```bash
./scripts/compilar-uxplay.sh
```

O resultado é `uxplay/build/uxplay.exe`.
