# Diário

## 12 de novembro de 2026

Relato: ruído estridente emitido no client.

Com dois jogadores na sala, o client que ouve a voz do outro não recebe fala. Sai um som estridente, contínuo, no lugar da voz. O host captura o microfone; o barulho aparece em quem recebe.

O `Editor.log` dessa sessão registra o descarte do pacote de voz, no canal unreliable (canal 1), no componente de voz do jogador (índice 3, `functionHash` 47818):

- `NetworkConnectionToClient: discarded RpcMesage for netId=1 componentIndex=3 functionHash=47818 because it's larger than the rpc buffer limit of 16370 bytes for the channel: 1`
- `NetworkConnection.ValidatePacketSize: cannot send packet larger than 16384 bytes, was 30753 bytes`

30753 bytes é cerca de 1 segundo de PCM16 a 16 kHz. O Telepathy corta em 16384. O RPC inteiro é jogado fora. No client, `VoiceChatController.OnStreamRead` preenche o clipe com silêncio enquanto a fila está vazia e, quando um bloco seguinte entra, a reprodução salta. Esse salto é o som estridente.

O envio foi limitado no mesmo dia a uma janela de 100 ms (1600 amostras a 16 kHz, teto de 3000) em `MaxVoiceSamples`, descartando o áudio acumulado e ficando só com o trecho mais recente (`VoiceReadStart`). O relato fica aberto até o client ser ouvido de novo sem o estridente.

O `TutorialButton` abre `Tutorial.unity` em host + client e coloca o jogador no centro, acima do `Ground`.

## 4 de novembro de 2026

O projeto também pode ser uma plataforma aberta para o ensino de computação gráfica, modelagem 3D e programação. Pode ser um curso de Unity. Jovens iniciantes colaboram com novas salas usando o projeto como framework e caso de uso, e aprendem enquanto contribuem com um jogo real, já em produção e disponível para a comunidade.

## 31 de outubro de 2026

Hoje tive dificuldade para implementar um movimento sobre o espaço sideral, considerando o tamanho da sala de aula e a distância que eu queria a qualquer astro. Felizmente vi um vídeo sobre IAs, vibe coding e resolvi auditar vulnerabilidades e malware. Também pedi para reorganizar o código e refatorar usando os princípios do Clean Code e do SOLID. Graças a isso fui capaz de ler o código (e pedir para a IA me ajudar a entender), como também definir como o algoritmo de aproximação deveria agir. Li partes do código e fui mexendo aos poucos, hora manualmente, ora pedindo para o agente. Finalmente funcionou, mas antes pedi para esse agente corrigir umas cinco vezes. Como aprendi com meu amigo Erik Wendel (na verdade ele não me conhece, só eu que conheço ele), a IA vai sempre fazer algo e responder algo, mesmo que não saiba. Dito e certo: nas cinco tentativas o agente mudou o código para versões malucas, difíceis de entender. No final tive que confiar na minha capacidade de identificar a raiz do problema e resolver por conta própria (semi-vibe).

Mantenha seu código limpo, sério.
