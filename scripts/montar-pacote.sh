#!/usr/bin/env bash
# Monta dist/pacote/uxplay: o uxplay.exe, os plugins do GStreamer que ele usa
# e todas as DLLs de que eles dependem. Rodar num shell MSYS2 UCRT64 depois
# do compilar-uxplay.sh. A interface (.NET) e' publicada em dist/pacote pelo
# workflow (dotnet publish), fora deste script.
set -euo pipefail

DESTINO=dist/pacote/uxplay
PLUGINS_DESTINO="$DESTINO/lib/gstreamer-1.0"
# appsrc, decodebin, h264parse, videoconvert/videoscale, o sink do Direct3D 11,
# o decodificador por hardware (d3d11) e o de reserva por software (openh264,
# BSD-2). O libav ficou de fora: puxava o ffmpeg e mais de 100 DLLs, entre
# elas o jbigkit (GPL-2.0 sem "or later", incompativel com a GPLv3).
PLUGINS="coreelements app typefindfunctions playback videoparsersbad videoconvertscale autodetect d3d11 openh264"

rm -rf "$DESTINO"
mkdir -p "$PLUGINS_DESTINO"
cp uxplay/build/uxplay.exe "$DESTINO/"
for p in $PLUGINS; do
  cp "/ucrt64/lib/gstreamer-1.0/libgst$p.dll" "$PLUGINS_DESTINO/"
done

# Dependencias, recursivas, so' as que vem do MSYS2 (as do Windows ficam).
# Qualquer raiz do MSYS2: no CI ele fica em D:\a\_temp\msys64, nao em C:\msys64.
for f in "$DESTINO/uxplay.exe" "$PLUGINS_DESTINO"/*.dll; do
  ntldd -R "$f"
done | grep -ioE '[a-z]:[\\/][^ ]*[\\/]ucrt64[\\/]bin[\\/][^ ]+\.dll' | sort -fu |
  while read -r dll; do
    cp "$(cygpath -u "$dll")" "$DESTINO/"
  done

echo "pacote: $(ls "$DESTINO"/*.dll | wc -l) DLLs, $(ls "$PLUGINS_DESTINO" | wc -l) plugins, $(du -sh "$DESTINO" | cut -f1)"
