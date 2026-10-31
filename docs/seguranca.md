# Riscos de segurança

**Projeto:** meta-class-alpha  
**Cena:** `Assets/Scenes/TheRoom.unity`  
**Escopo:** revisão do código do projeto em outubro de 2026. Não houve indício de malware: os scripts próprios não baixam código, não decodificam payload e não abrem processos. As DLLs em `Assets/Mirror/Plugins/Mono.Cecil/` pertencem ao Weaver do Mirror. As URLs do repositório apontam para Unity, Mirror, Cardboard e ParrelSync.

Estes riscos são aceitáveis numa aula em rede local fechada. Ficam graves se a porta do servidor estiver acessível fora dessa rede.

---

## 1. Servidor sem autenticação e sem cifra

**Gravidade:** alta  
**Onde:** `NetworkManager` em `Assets/Scenes/TheRoom.unity` (porta **7777**, `authenticator` vazio, `autoStartServerBuild` ligado) e transporte Telepathy no mesmo objeto.

O servidor sobe sozinho na build Android. Não há `NetworkAuthenticator`. Quem alcançar a porta entra como jogador, até o limite de 4 conexões. O Telepathy envia o tráfego em texto puro: pose do jogador e áudio da voz podem ser lidos no caminho.

**Mitigação**

- Manter a porta 7777 só na rede da sala. Não encaminhar essa porta no roteador e não publicar o endereço do aparelho-host.
- Antes de qualquer uso fora dessa rede, atribuir um `NetworkAuthenticator` no `NetworkManager` e recusar a conexão de quem não apresentar a credencial da aula.
- Para tráfego que saia da rede local, trocar o Telepathy por um transporte com TLS (por exemplo SimpleWeb com certificado) ou colocar o host atrás de uma VPN. O Telepathy não cifra a sessão.

---

## 2. Voz retransmitida sem controle no servidor

**Gravidade:** média  
**Onde:** `CmdSendVoice` em `Assets/Scripts/VoiceChatController.cs`.

O mudo dos convidados é decidido no cliente, em `CaptureAndSend`, lendo o `ClassController` local. O comando no servidor só descarta pacote nulo ou vazio e chama `RpcReceiveVoice` para os outros clientes. Um cliente conectado pode continuar enviando áudio mesmo com o mudo ligado na cena do host. O teto prático é o `serverMaxMessageSize` de 16384 bytes do Telepathy.

**Mitigação**

- No `CmdSendVoice`, recusar o pacote quando o remetente não for o host e `ClassController.MuteGuests` estiver ligado. Essa checagem tem de ocorrer no servidor, não só antes do envio.
- Recusar pacotes maiores do que um bloco legítimo de voz (taxa de 16 kHz, cerca de 100 ms, PCM16 mono) e impor um intervalo mínimo entre comandos do mesmo jogador.
- Manter o canal unreliable. Não passar a voz para um canal confiável: um fluxo contínuo nesse canal aumenta a fila do servidor.

---

## 3. Acesso amplo ao armazenamento Android

**Gravidade:** baixa  
**Onde:** `android:requestLegacyExternalStorage="true"` em `Assets/Plugins/Android/AndroidManifest.xml`.

O manifesto pede o modo antigo de armazenamento externo em Android que ainda o honra. O restante do arquivo é o modelo gerado pela Unity. O jogo não precisa desse acesso para o Cardboard, o Mirror ou o microfone. O comentário no topo avisa que a Unity pode regravar o arquivo na exportação.

**Mitigação**

- Remover `requestLegacyExternalStorage` se nenhuma função do jogo lê ou grava arquivo fora do armazenamento interno.
- Depois de exportar um APK, conferir o manifesto gerado. Se a Unity repor o atributo, fixá-lo num manifesto customizado marcado para não ser sobrescrito, em vez de editar só a cópia gerada.

---

## 4. Mensagens de olhar no cliente local

**Gravidade:** baixa  
**Onde:** `sendMessage` em `Assets/Scripts/CameraPointerController.cs`.

`SendMessage` dispara `OnPointerEnter`, `OnPointerExit` e `OnPointerClick` no objeto olhado. A lista está restrita às tags `Interactive`, `Teleportable` e `Player`, e a chamada não atravessa a rede. O risco aparece se essa lista passar a aceitar um nome de método vindo de outro jogador.

**Mitigação**

- Manter a lista de tags e os nomes fixos dos métodos. Não encaminhar uma string recebida pela rede para `SendMessage`.
- Objetos com essas tags devem tratar só `OnPointerEnter`, `OnPointerExit` e `OnPointerClick`.

---

## Quando revisar de novo

Revisar este documento ao adicionar um autenticador, ao trocar o transporte, ao mudar `CmdSendVoice` ou ao exportar um manifesto Android diferente.
