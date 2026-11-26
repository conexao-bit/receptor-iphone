#!/usr/bin/env bash
# Compila o uxplay.exe. Rodar num shell MSYS2 UCRT64, na raiz do repositorio.
set -euo pipefail

pacman -S --needed --noconfirm \
  mingw-w64-ucrt-x86_64-cmake \
  mingw-w64-ucrt-x86_64-gcc \
  mingw-w64-ucrt-x86_64-ninja \
  mingw-w64-ucrt-x86_64-pkgconf \
  mingw-w64-ucrt-x86_64-libplist \
  mingw-w64-ucrt-x86_64-openssl \
  mingw-w64-ucrt-x86_64-gstreamer \
  mingw-w64-ucrt-x86_64-gst-plugins-base \
  mingw-w64-ucrt-x86_64-gst-plugins-good \
  mingw-w64-ucrt-x86_64-gst-plugins-bad \
  mingw-w64-ucrt-x86_64-gst-libav \
  mingw-w64-ucrt-x86_64-ntldd

cmake -G Ninja -S uxplay -B uxplay/build
ninja -C uxplay/build
