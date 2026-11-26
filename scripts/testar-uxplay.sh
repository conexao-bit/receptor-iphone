#!/usr/bin/env bash
# Testes das correcoes nossas no UxPlay. Rodar num shell MSYS2 UCRT64 depois
# do compilar-uxplay.sh (usa a libairplay.a do build).
set -euo pipefail

mkdir -p uxplay/build/testes
gcc -o uxplay/build/testes/teste_digest.exe testes/teste_digest.c \
  -Iuxplay/lib uxplay/build/lib/libairplay.a \
  $(pkg-config --cflags --libs openssl libplist-2.0) -lws2_32 -liphlpapi
./uxplay/build/testes/teste_digest.exe
