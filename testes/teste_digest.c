/* Regressao da queda do -pin no Windows (2026-11-26): com -pin, o passwd()
 * do uxplay devolve len = -2 e senha NULL, e o SETUP com "Authorization:
 * Digest" chegava em pairing_digest_verify com password NULL (strlen(NULL),
 * 0xC0000005). O cabecalho e' o que o iPhone mandou no teste (o response foi
 * cortado no log pela queda; o resto do valor nao importa aqui). */
#include <stdbool.h>
#include <stdio.h>
#include "pairing.h"

static int falhas = 0;

static void conferir(bool ok, const char *nome) {
    printf("%s %s\n", ok ? "ok  " : "FALHOU", nome);
    if (!ok) falhas++;
}

int main(void) {
    const char *iphone =
        "Digest username=\"iTunes\", realm=\"raop\", nonce=\"913b69f1e80abba1905f00dbe4bbbb90\", "
        "uri=\"rtsp://fd2a:ade1:7d23:e84d:a648:8984:9053:910b/8680103844065461139\", "
        "response=\"87144115fa537bf73b06a192f0000000\"";

    conferir(!pairing_digest_verify("SETUP", iphone, NULL), "senha NULL recusa, sem cair");
    conferir(!pairing_digest_verify("SETUP", "Digest username=\"iTunes\", realm=\"raop\"", "1234"),
             "cabecalho sem nonce/uri/response recusa, sem cair");
    conferir(!pairing_digest_verify("SETUP", iphone, "1234"), "resposta errada recusa");
    return falhas ? 1 : 0;
}
