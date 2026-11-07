# meta-class-alpha

> **Idiomas:** [English](README.md) | Português (BR)

Experiência multiplayer em realidade virtual para **Google Cardboard**, onde jogadores exploram uma sala imersiva com sistema solar, planetas e portais. O foco é a interação por **olhar** (gaze) e a presença compartilhada entre participantes conectados em rede.

## Sobre o jogo

O build abre na cena **Menu**. Dali o anfitrião cria a sala e o Mirror carrega **The Room**. No visor, o jogador pode:

- Observar e interagir com objetos no ambiente
- Girar a sala 45° no sentido horário em torno da área do anfitrião
- Viajar com a sala até um corpo celeste e voltar
- Teleportar-se para a superfície de corpos celestes marcados como teleportáveis
- Conversar por voz com outros jogadores em tempo real
- Navegar por portais esféricos e cilíndricos no cenário

A experiência combina exploração espacial, interação sem mãos (apenas com o olhar) e multiplayer síncrono.

## Conceitos e tecnologias

| Conceito | Implementação |
|----------|---------------|
| **VR mobile** | [Google Cardboard XR Plugin](https://github.com/googlevr/cardboard-xr-plugin) (`InitCardboard`) |
| **Multiplayer** | [Mirror](https://mirror-networking.com/) com transporte **Telepathy** (porta `7777`) |
| **Interação por gaze** | `CameraPointerController` — raycast a partir da câmera, clique por permanência (~3 s) |
| **Voice chat** | `VoiceChatController` — captura de microfone, streaming PCM16 via Mirror (canal unreliable) |
| **Teste local multiplayer** | [ParrelSync](https://github.com/VeriorPies/ParrelSync) — clones do projeto no Editor |
| **UI** | TextMesh Pro + slider de progresso do gaze. O menu usa **Montserrat Bold** |
| **Menu** | `CarouselMenu` — toque na tela gira o menu 90° em 1,5 s |

### Tags importantes

Definidas em `ProjectSettings/TagManager.asset` e usadas pela lógica de interação:

| Tag | Uso |
|-----|-----|
| `Interactive` | Objetos que respondem a `OnPointerEnter` / `OnPointerExit` |
| `Teleportable` | Superfícies para onde o jogador pode se teleportar ao completar o gaze |
| `Planet` | Agrupamento de objetos planetários (ex.: nome exibido na UI) |
| `Environment` | Cenário estático, ignorado na interação |

### Fluxo de interação (gaze)

```
Câmera (direção do olhar)
    → Raycast
    → Objeto com tag Interactive / Teleportable / Player
    → OnPointerEnter (destaque visual via ObjectBasicController)
    → Permanência no alvo (~3 s, barra de progresso)
    → Clique: ação (teleporte se Teleportable) ou feedback na UI
    → OnPointerExit ao sair do foco
```

## Arquitetura do projeto

```
meta-class-alpha/
├── Assets/
│   ├── Scenes/Menu.unity         # Primeira cena do build
│   ├── Scenes/TheRoom.unity      # Sala de aula, carregada como cena online
│   ├── Scripts/                  # Lógica do jogo
│   ├── Prefabs/                  # Player, portais, tesouros
│   ├── Models/                   # SolarSystem.fbx, avatar (Male_002)
│   ├── Materials/                # Skybox, sol, portais, UI
│   ├── XR/                       # Loaders e settings do Cardboard (Android)
│   ├── Mirror/                   # Biblioteca de networking
│   ├── ParrelSync/               # Teste multiplayer no Editor
│   └── Plugins/Android/          # Gradle e manifest para build Android
├── Packages/manifest.json        # Dependências UPM (Cardboard, etc.)
├── ProjectSettings/              # Player, XR, build, tags
├── BUILD_ANDROID_pt.md           # Guia de build APK
└── extensions.list               # Extensões recomendadas do VS Code
```

### Componentes principais

| Script | Responsabilidade |
|--------|------------------|
| `InitCardboard` | Inicialização do Cardboard, skybox, recentralização e saída do app |
| `CameraPointerController` | Gaze, teleporte, UI de foco e clique por permanência (`NetworkBehaviour`) |
| `CameraEditorController` | Controle por mouse no Editor (apenas `isLocalPlayer`) |
| `ObjectBasicController` | Feedback visual de gaze (troca de material) |
| `VoiceChatController` | Captura e reprodução espacial de voz (`NetworkBehaviour`) |
| `SunLightController` | Luz pontual seguindo o Sol no sistema solar |
| `UILogger` | Mensagens de interação na tela (TMP) |
| `CarouselMenu` | Faces do menu: Criar Sala, Buscar Sala e Voltar, por toque na tela |
| `MenuBackdrop` | Ajusta o fundo do menu à câmera ortográfica |
| `ClassController` | Gira a sala 45° em torno da `HostArea` em 3 s e posiciona os jogadores |
| `RotateClassButton` | Clique de olhar que inicia essa rotação |
| `SolarSystemThumbHost` | Toque na miniatura cria a sala, como o Host do HUD |
| `RoomList` | Lista salas abertas na rede local; o toque entra só como cliente |
| `LanRoomBeacon` | Anuncia a sala enquanto há servidor e busca salas enquanto o menu está ocioso |
| `LogoBounce` | Oscilação leve da logo enquanto o menu está ativo |

### Rede (Mirror)

- **NetworkManager** na cena `Menu`, porta **7777**, máximo **4** conexões. Permanece com `dontDestroyOnLoad`
- **Cena offline** `Menu`, **cena online** `TheRoom`. The Room não contém um segundo manager
- O HUD do Mirror no canto continua criando a sala. Os rótulos Criar Sala e Buscar Sala só giram o carrossel. O toque em SolarSystemThumb chama a mesma ação Host (Server + Client). RoomList mostra as salas abertas na rede local, e o toque numa delas entra como Client nessa sala
- **Player prefab** (`Assets/Prefabs/Player.prefab`): câmera, canvas, gaze, voz e identidade de rede
- Servidor inicia automaticamente em builds (`autoStartServerBuild`)
- Cliente conecta manualmente (host `localhost` por padrão no Editor)

### Diagrama simplificado

```mermaid
flowchart TB
    subgraph Client["Cliente (Cardboard)"]
        Cam[CameraPointerController]
        Voice[VoiceChatController]
        Cardboard[InitCardboard]
    end

    subgraph Server["Servidor (Mirror)"]
        NM[NetworkManager]
        TP[Telepathy :7777]
    end

    subgraph Menu["Menu.unity"]
        Carousel[CarouselMenu]
        HUD[NetworkManagerHUD]
    end

    subgraph Scene["TheRoom.unity"]
        SS[SolarSystem]
        Class[Class]
        Portals[Portais]
    end

    HUD -->|Host / Client| NM
    Carousel -->|giro de 90°| Menu
    Cam -->|gaze / teleporte / giro da sala| Scene
    Voice -->|PCM16 unreliable| NM
    NM --> TP
    NM -->|cena online| Scene
    Cardboard --> Cam
```

## Requisitos

| Ferramenta | Versão |
|------------|--------|
| Unity | **6000.3.6f1** (Unity 6.3) |
| Plataforma alvo | Android (Cardboard) + Editor para desenvolvimento |
| IDE | VS Code, Rider ou Visual Studio |

Módulos Unity recomendados no Hub: **Android Build Support** (SDK, NDK, OpenJDK).

## Começando (desenvolvedores)

### 1. Clonar e abrir

```bash
git clone <url-do-repositorio>
cd meta-class-alpha
```

Abra a pasta no **Unity Hub** com o editor **6000.3.6f1**.

### 2. Cena de trabalho

Abra `Assets/Scenes/Menu.unity` e dê Play a partir dela. É a primeira cena habilitada no build. `TheRoom.unity` entra quando a sala fica online. O Play usa a cena aberta, então começar o Play dentro de The Room pula o menu.

### 3. Testar no Editor

1. Pressione **Play**
2. O servidor Mirror sobe automaticamente na porta `7777`
3. No Editor, `CameraEditorController` permite olhar ao redor com o mouse (somente jogador local)
4. Para testar **dois jogadores** no mesmo PC, use **ParrelSync → Clones Manager** e abra um clone do projeto

### 4. Testes unitários

A suíte é de **Edit Mode**, no assembly `MetaClass.EditModeTests`. Os arquivos ficam em pastas `Editor/` ao lado de cada script em `Assets/Scripts/` e não entram no build do player. Não é preciso abrir nem dar Play na cena.

**No Editor**

1. Abra o projeto no Unity **6000.3.6f1**.
2. Vá em **Window → General → Test Runner**.
3. Na aba **EditMode**, selecione `MetaClass.EditModeTests` e clique em **Run All**.

**Pelo terminal**

Feche o Editor antes. Uma segunda instância do Unity no mesmo projeto é recusada enquanto ele estiver aberto.

```bash
/home/ive/Unity/Hub/Editor/6000.3.6f1/Editor/Unity \
  -batchmode -nographics \
  -projectPath "$(pwd)" \
  -runTests -testPlatform EditMode \
  -assemblyNames MetaClass.EditModeTests \
  -testResults /tmp/meta-class-editmode-results.xml \
  -logFile /tmp/meta-class-editmode.log
```

O caminho do executável é o do Unity Hub para a versão **6000.3.6f1**. O resumo da execução fica em `/tmp/meta-class-editmode-results.xml`; o log do Editor, em `/tmp/meta-class-editmode.log`. O processo termina sozinho ao final dos testes.

### 5. Testar em dispositivo Android

Siga o guia completo em **[BUILD_ANDROID_pt.md](BUILD_ANDROID_pt.md)**.

Resumo: troque a plataforma para Android, desative App Bundle se quiser APK, e faça **Build** ou **Build And Run**.

### 6. Multiplayer em rede local

1. Os dois aparelhos ficam na mesma rede local, com a cena Menu aberta na face Buscar Sala
2. Quem cria a sala (miniatura ou Host do HUD) anuncia em broadcast. A lista do outro aparelho mostra `endereço:porta` em até cerca de 3 s
3. O toque no item entra como cliente nessa sala. O anfitrião não vê a própria sala, e um item some depois de cerca de 7 s sem anúncio novo
4. A porta **7777** precisa estar liberada no firewall. Dois processos na mesma máquina podem não se ver, porque o anúncio não volta para o remetente

## Estrutura de prefabs

| Prefab | Descrição |
|--------|-----------|
| `Player.prefab` | Jogador em rede: câmera VR, gaze, voz, HUD |
| `Sphere Portal.prefab` | Portal esférico interativo |
| `Cylinder Portal.prefab` | Portal cilíndrico interativo |
| `Treasure.prefab` | Objeto colecionável/interativo |

## Extensões VS Code

Lista recomendada em `extensions.list`. Para instalar em uma máquina nova:

```bash
cat extensions.list | xargs -I {} code --install-extension {}
```

Para atualizar a lista após instalar novas extensões:

```bash
code --list-extensions > extensions.list
```

Extensões atuais: C#, Unity Tools, Unity Debug, DocComment.

## Configuração do player (Android)

| Campo | Valor |
|-------|-------|
| Package Name | `com.EbonLabs.metaclassalpha` |
| Versão | `0.1` (versionCode: 1) |
| Min API Level | 25 |
| Scripting Backend | IL2CPP |
| Arquiteturas | ARMv7 + ARM64 |

## Documentação adicional

- [docs/arquitetura.md](docs/arquitetura.md) — arquitetura: funcionalidades genéricas, a cena Menu e a aplicação em The Room
- [docs/codigo-limpo.md](docs/codigo-limpo.md) — consulta para agentes: arquitetura, Clean Code, SOLID e testes
- [docs/seguranca.md](docs/seguranca.md) — riscos de rede, voz e manifesto Android, com a mitigação de cada um
- [docs/TDD-sala-observacao-controle.md](docs/TDD-sala-observacao-controle.md) — TDD da sala de observação e controle (painel descritivo feito; sala e portal de retorno a fazer)
- [BUILD_ANDROID_pt.md](BUILD_ANDROID_pt.md) — gerar e instalar APK ([EN](BUILD_ANDROID.md))
- [Mirror Documentation](https://mirror-networking.com/docs/)
- [Cardboard XR Plugin](https://github.com/googlevr/cardboard-xr-plugin)
- [ParrelSync Wiki](https://github.com/VeriorPies/ParrelSync/wiki)

## Licença e créditos

- **Empresa:** EbonLabs
- **Produto:** meta-class-alpha
- Bibliotecas de terceiros: Mirror, Google Cardboard XR, ParrelSync, TextMesh Pro
- Fonte do menu: [Montserrat](https://github.com/JulietaUla/Montserrat) Bold, SIL Open Font License (`Assets/Fonts/OFL.txt`)
