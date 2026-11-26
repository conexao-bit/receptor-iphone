#!/usr/bin/env bash
# Gera THIRD_PARTY.md e copia as licencas para dist/pacote/licencas: para cada
# DLL e plugin do pacote, o pacote MSYS2 de origem, a versao e a licenca.
# Rodar num shell MSYS2 UCRT64 depois do montar-pacote.sh.
set -euo pipefail

PACOTE=dist/pacote/uxplay
LICENCAS=dist/pacote/licencas
rm -rf "$LICENCAS" && mkdir -p "$LICENCAS"

origens=()
for f in "$PACOTE"/*.dll; do origens+=("/ucrt64/bin/$(basename "$f")"); done
for f in "$PACOTE"/lib/gstreamer-1.0/*.dll; do origens+=("/ucrt64/lib/gstreamer-1.0/$(basename "$f")"); done

# Uma consulta so' (uma por arquivo levava minutos): "<arquivo> is owned by <pacote> <versao>".
declare -A dono
while read -r arquivo _ _ _ pkg _; do
  dono[$pkg]+="$(basename "$arquivo") "
done < <(LC_ALL=C pacman -Qo "${origens[@]}")

{
  echo "# Componentes de terceiros"
  echo
  echo "O instalador leva, além do código deste repositório (GPLv3), estas bibliotecas"
  echo "do [MSYS2](https://www.msys2.org) (UCRT64). As licenças completas vão no"
  echo "instalador, na pasta \`licencas\`."
  echo
  echo "Gerado por \`scripts/licencas.sh\`."
  echo
  echo "| Pacote MSYS2 | Versão | Licença | Arquivos |"
  echo "| --- | --- | --- | --- |"
  # Tambem uma consulta so': "nome|versao|licenca" por pacote.
  LC_ALL=C pacman -Qi $(printf '%s\n' "${!dono[@]}" | sort) | awk -F' *: ' '
    /^Name/ { n = $2 } /^Version/ { v = $2 } /^Licenses/ { l = $2 }
    /^$/ && n { print n "|" v "|" l; n = "" }
    END { if (n) print n "|" v "|" l }' |
  while IFS='|' read -r pkg versao licenca; do
    echo "| $pkg | $versao | $licenca | ${dono[$pkg]% } |"
    for d in "/ucrt64/share/licenses/$pkg" "/ucrt64/share/licenses/${pkg#mingw-w64-ucrt-x86_64-}"; do
      if [ -d "$d" ]; then mkdir -p "$LICENCAS/$pkg" && cp -r "$d"/. "$LICENCAS/$pkg/"; break; fi
    done
    # Pacote sem o arquivo no MSYS2 (GStreamer, libplist): o texto padrao,
    # versionado em licencas/.
    if [ ! -d "$LICENCAS/$pkg" ]; then
      mkdir -p "$LICENCAS/$pkg"
      [[ "$licenca" == *LGPL-2.1* ]] && cp licencas/LGPL-2.1.txt "$LICENCAS/$pkg/"
      [[ "$licenca" == *GPL-2.0* ]] && cp licencas/GPL-2.0.txt "$LICENCAS/$pkg/"
      true
    fi
  done
} > THIRD_PARTY.md

echo "licencas: $(printf '%s\n' "${!dono[@]}" | wc -l) pacotes"
sem=0
for pkg in "${!dono[@]}"; do
  if [ -z "$(ls -A "$LICENCAS/$pkg" 2>/dev/null)" ]; then echo "  sem texto de licenca: $pkg"; sem=1; fi
done
exit $sem
