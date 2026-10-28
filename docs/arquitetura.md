# Documento de Arquitetura

**Projeto:** meta-class-alpha  
**Cena de build:** `Assets/Scenes/TheRoom.unity`  
**Escopo:** funcionalidades implementadas e a forma como a cena The Room as compõe  
**Fora de escopo:** features descritas em [TDD-sala-observacao-controle.md](TDD-sala-observacao-controle.md), que ainda não estão implementadas

Este documento separa duas camadas. A primeira descreve capacidades genéricas: scripts e contratos que não conhecem a sala, os planetas nem o enredo da experiência. A segunda descreve como The Room instancia essas capacidades, qual é a hierarquia da cena e como os objetos se relacionam em runtime.

---

## 1. Ideia central

O projeto é uma experiência de realidade virtual para Google Cardboard, com presença compartilhada via Mirror. A interação do jogador é o olhar: um raycast sai da câmera, um tempo de permanência confirma a ação e o resultado depende da tag do objeto atingido.

A simulação do mundo (órbita, rotação, rótulos, luz) roda em cada cliente a partir dos mesmos parâmetros. Só a presença do jogador e a voz atravessam a rede. Isso mantém os sistemas de cena independentes do transporte e limita o que precisa ser autoritativo.

```mermaid
flowchart LR
    subgraph Generico["Capacidades genéricas"]
        VR[Sessão VR]
        Gaze[Olhar e teleporte]
        HUD[HUD local]
        Net[Presença em rede]
        Voice[Voz espacial]
        Orbit[Movimento orbital]
        Label[Rótulo billboard]
        Light[Luz do sol]
    end

    subgraph Cena["The Room"]
        Room[Sala e piso]
        System[Sistema solar]
        Player[Player.prefab]
    end

    VR --> Player
    Gaze --> Player
    Gaze --> Room
    Gaze --> System
    HUD --> Player
    Net --> Player
    Voice --> Player
    Orbit --> System
    Label --> System
    Light --> System
```

---

## 2. Funcionalidades genéricas

Cada bloco abaixo pode ser reutilizado em outra cena desde que os contratos (tags, nomes de âncora, prefabs) sejam respeitados. Nenhum deles referencia objetos exclusivos de The Room além de nomes de busca do Sol (`Sun Sphere`, `Sun`, `SUN`) e da raiz `SolarSystem`, usados só para localizar o centro orbital.

### 2.1 Sessão de realidade virtual

| Peça | Papel |
|------|--------|
| `InitCardboard` | Sobe o dispositivo Cardboard, aplica o skybox, impede o sono da tela e trata os botões do visor |

No `Start`, o script aplica o material de céu configurado e, se a câmera estiver em cor sólida, passa o clear flag para skybox. Em `Update` trata o ciclo do visor: escanear parâmetros do aparelho, recarregá-los quando mudam, recentralizar a visão com o gatilho e encerrar o aplicativo no botão de fechar. Fora do Editor, atualiza os parâmetros de tela a cada frame.

O script não conhece jogador, rede nem objetos interativos. Qualquer objeto da cena pode carregá-lo.

### 2.2 Presença multiplayer

A rede usa Mirror com transporte Telepathy. O contrato genérico é:

- um `NetworkManager` na cena, com um prefab de jogador e um transporte;
- o jogador é o único objeto spawnado pela rede;
- posição e rotação do dono seguem para o servidor (`NetworkTransform` com `SyncDirection.ClientToServer`) e dali para os demais clientes;
- câmera, canvas e listener existem no prefab, mas só o jogador local os liga.

Parâmetros atuais do manager, reutilizáveis como padrão do projeto: porta **7777**, no máximo **4** conexões, `dontDestroyOnLoad`, `autoStartServerBuild` ligado e `autoConnectClientBuild` desligado. O endereço padrão no Editor é `localhost`.

`CameraPointerController.OnStartClient` posiciona o jogador local numa área aleatória próxima da origem (`x` e `z` entre 0 e 3, `y = 1`) e habilita câmera e canvas somente quando `isLocalPlayer` é verdadeiro.

### 2.3 Olhar, permanência e teleporte

`CameraPointerController` é um `NetworkBehaviour`, mas o raycast e o clique rodam só no jogador local. A rede entra depois, quando o `NetworkTransform` replica a nova pose.

Fluxo, independente do conteúdo da cena:

1. Um raio sai da posição da câmera na direção `forward`, sem limite de distância.
2. Se o alvo muda, o objeto anterior recebe `OnPointerExit` e o novo pode receber `OnPointerEnter`.
3. Objetos com tag `Environment` não entram em foco. O mesmo vale para um alvo de teleporte em cuja borda o observador já está, e para o alvo recém-usado (o ponteiro fica desarmado até o olhar sair dele).
4. Enquanto o foco permanece, um contador avança. O tempo padrão de confirmação é **3 segundos**. Um slider mostra a fração decorrida.
5. Ao completar o tempo, `handlePointerClick` dispara. Se a tag é `Teleportable`, o jogador é movido para um ponto na superfície do volume do alvo, com uma margem de 0,05 unidade para fora, e o ponteiro daquele alvo é desligado. Outras tags só registram o clique no HUD.

`OnPointerEnter` e `OnPointerExit` são enviados com `SendMessage` apenas para as tags `Interactive`, `Teleportable` e `Player`. O destaque visual fica a cargo de quem implementar esses métodos, em geral `ObjectBasicController`.

O nome exibido no HUD tem uma convenção: se o pai do alvo tem tag `Planet`, a mensagem é `Planeta: <nome do pai>`. Caso contrário, usa o nome do próprio objeto.

### 2.4 Feedback visual de foco

`ObjectBasicController` troca o material do `Renderer` do próprio objeto entre um material inativo e um material de olhar. As trocas usam instâncias estáveis, para que um shader com animação de tempo continue correndo entre entrar e sair do foco. Não decide o que o clique faz.

### 2.5 HUD local

`UILogger` escreve uma string num `TextMeshProUGUI`. Não formata mensagens e não fala com a rede. Quem define o texto é o controlador de olhar.

O indicador de progresso é um `Slider` filho do jogador, ativado só durante a permanência. O ícone do ponteiro troca de sprite entre ocioso e focado.

### 2.6 Olhar no Editor

`CameraEditorController` gira o transform do jogador local com o mouse e trava o cursor. Existe para testar a mesma câmera do gaze sem o visor. Em build, o `Start` retorna cedo; o `Update` ainda lê o mouse se houver um jogador local.

### 2.7 Voz espacial

`VoiceChatController` captura o microfone do jogador local e reproduz a voz dos outros com áudio espacial.

| Etapa | Comportamento |
|-------|----------------|
| Captura | Microfone em loop, 16 kHz, bloco de cerca de 100 ms |
| Envio | PCM16 num `[Command]` no canal unreliable do Mirror |
| Recepção | `[ClientRpc]` com `includeOwner = false`; o clipe é enfileirado no `AudioSource` de quem falou |
| Espacialização | `spatialBlend = 1`, rolloff linear, 1 m a 25 m |

No Android, o script pede permissão de microfone antes de gravar. O `AudioListener` do prefab começa desligado e só o jogador local o habilita, para não haver vários listeners na cena.

### 2.8 Simulação orbital

O movimento celeste está em `Assets/Scripts/PlanetaryMotion/` e não depende de rede.

| Tipo | Responsabilidade |
|------|------------------|
| `SolarPlanetId` | Identidade científica: Mercúrio a Netuno |
| `PlanetScientificCatalog` | Semi-eixo em UA, período orbital em dias, dia sideral em horas (negativo = retrógrado) e obliquidade |
| `PlanetOrbitCalculator` | Velocidade angular da translação e posição em volta de um centro |
| `PlanetSpinCalculator` | Velocidade angular da rotação e quaternion local, com sinal retrógrado |
| `SolarSystemLocator` | Acha o transform do Sol por nome |
| `PlanetOrbitMotion` | Aplica órbita ou rotação num transform |
| `SolarSystemMotionController` | Propaga tempo, escala de distância e visibilidade para os filhos |

Dois eixos de movimento ficam em objetos diferentes de propósito. Órbita e rotação no mesmo transform brigam com a pose local dos filhos. O padrão é:

- o **wrapper** orbita o Sol e não gira (`enableOrbit` ligado, `enableSpin` desligado);
- o **corpo** filho gira no lugar e não translada (`enableSpin` ligado, `enableOrbit` desligado).

`PlanetOrbitMotion` grava a pose local dos filhos cujo nome começa com `InteractionOrb` e a restaura no `LateUpdate`, para o marcador não derivar quando o pai é movido. Rótulos ficam de fora desse travamento: eles próprios ajustam escala e pose.

A escala de distância é a da órbita da Terra em unidades de cena. O catálogo fixa o máximo em **300**. Os outros corpos mantêm a proporção em unidades astronômicas (`semiMajorAxisAu * earthOrbitSceneUnits`). O `timeScale` padrão **4320** comprime um dia sideral da Terra para cerca de 20 segundos reais.

`SolarSystemMotionController` observa os próprios campos e só reaplica o que mudou: `timeScale`, `distanceScale`, `showInteractionOrbs` e `showPlanetLabels`. Orbes são objetos filhos cujo nome começa com `InteractionOrb`. Rótulos são componentes `PlanetLabel`. A ordem de execução é `-100`, antes do movimento (`1000`) e dos rótulos (`1100`).

O controlador não é `NetworkBehaviour`. Cada cliente simula o sistema com os mesmos números iniciais. Não há sincronização de fase orbital.

### 2.9 Rótulos que mantêm tamanho aparente

`PlanetLabel` escreve um nome num TextMesh Pro e escala o transform para que a altura aparente acompanhe o `fontSize`, independente da distância até a câmera local. A rotação de billboard fica no material (`LiberationSans SDF Billboard`); o script preserva a rotação autoral e só corrige posição local e escala no `LateUpdate`.

A câmera usada é a do `NetworkClient.localPlayer`. Sem rede, cai para a primeira câmera ativa. `SetVisible` liga ou desliga o objeto e é o gancho do controlador do sistema solar.

O texto vem de `overrideText`. Se estiver vazio, o script sobe nos pais, acha um `PlanetOrbitMotion` com órbita ligada e usa `PlanetProfile`.

### 2.10 Luz e substituição do Sol

`SunLightController` (`ExecuteAlways`) mantém uma luz pontual na posição do Sol. Cor quente, intensidade e alcance configuráveis, temperatura de cor 5778 K. A busca do Sol segue os mesmos nomes de `SolarSystemLocator`.

`SolarSystemSunReplacer` copia pose e escala de um Sol antigo (`SUN`, `Sun` ou `Sphere.009`) para um objeto `Sun Sphere` e remove o antigo. Serve para trocar a malha do modelo importado sem perder o centro que a órbita e a luz já usam.

### 2.11 Camada de nuvens

`PlanetCloudLayer` cria, se ainda não existir, um filho `Clouds`: a mesma malha da superfície, escala ligeiramente maior (`shellScale` 1,01) e um material de nuvem sem sombra. Se o material não estiver atribuído, não faz nada. É um complemento visual do corpo, sem collider próprio de interação.

### 2.12 Contrato de tags

Definidas em `ProjectSettings/TagManager.asset`. A lógica genérica depende delas, não do nome do modelo.

| Tag | Efeito |
|-----|--------|
| `Interactive` | Recebe enter/exit de olhar. O clique só atualiza o HUD |
| `Teleportable` | Igual à anterior e, ao completar a permanência, move o jogador para a borda do volume |
| `Planet` | Não é alvo de teleporte. Serve para agrupar o corpo e nomear o HUD quando o filho focado tem essa tag no pai |
| `Player` | Recebe enter/exit. Identifica o avatar em rede |
| `Environment` | O raycast acerta, mas o foco não começa |

---

## 3. Aplicação na cena The Room

The Room é a única cena do build. Ela não adiciona regras novas de interação: compõe as capacidades da seção 2 em três grupos — infraestrutura de sessão, sala próxima ao jogador e sistema solar em escala.

### 3.1 O que a cena é

Uma sala com piso, alguns objetos ao alcance do olhar e, ao redor, o sistema solar em órbita. O jogador nasce perto da origem, olha os objetos da sala ou os portais presos aos corpos celestes e, ao sustentar o olhar num portal, é colocado na borda desse portal. Outros jogadores aparecem como avatares com voz espacial. O céu estrelado e a esfera de fundo fecham o ambiente.

### 3.2 Hierarquia

Raízes da cena:

```
TheRoom
├── NetworkManager          sessão Mirror (não é geometria)
├── EventSystem             UI
├── SpaceSphere             esfera de fundo, escala 50000
├── Interactive             objetos da sala
│   ├── Treasure
│   ├── Chair               instância de Cylinder Portal
│   └── Sphere Portal
└── Environment
    ├── Plane               piso, tag Environment; carrega InitCardboard
    ├── Sun Light           luz pontual, tag Environment
    └── SolarSystem.fbx     modelo importado
        ├── Sun Sphere      prefab do Sol, com um Portal
        └── um wrapper por planeta
```

Cada planeta segue o mesmo molde. O nome `JupterWrapper` / `Jupter` é o nome autoral da cena para Júpiter.

```
MercuryWrapper | VenusWrapper | EarthWrapper | MarsWrapper
JupterWrapper | SaturnWrapper | UranusWrapper | NeptuneWrapper
├── <Planeta>                 malha do FBX, tag Planet, só rotação
│   ├── Portal                Sphere Portal, tag Teleportable
│   └── Clouds                só Terra e Vênus
├── PlanetLabel               texto com o nome do corpo
└── InteractionOrb            partículas; pose local travada pelo wrapper
```

O jogador não está na hierarquia salva. O `NetworkManager` instancia `Assets/Prefabs/Player.prefab` ao conectar.

```
Player                        tag Player
├── câmera, listener, gaze, editor, NetworkIdentity, NetworkTransform, voz
├── Canvas                    World Space, desligado até ser o jogador local
│   ├── Text (TMP)            mensagem do UILogger
│   ├── Loader / Slider       barra dos 3 segundos
│   └── Icon                  sprite ocioso ou focado
└── Male_002                  avatar visual
```

### 3.3 Como cada funcionalidade aparece na cena

| Funcionalidade genérica | Onde vive em The Room |
|-------------------------|------------------------|
| Sessão VR | `InitCardboard` no `Plane`, com o material de skybox estrelado |
| Presença em rede | `NetworkManager` na raiz; jogadores spawnados a partir de `Player.prefab` |
| Olhar e teleporte | `CameraPointerController` no Player. Alvos reais de teleporte são os portais, não as malhas dos planetas |
| Feedback de foco | `ObjectBasicController` em `Sphere Portal`, `Cylinder Portal` e `Treasure` |
| HUD | Canvas do Player, à frente da câmera em World Space |
| Olhar no Editor | `CameraEditorController` no Player, sensibilidade cerca de 10 |
| Voz | `VoiceChatController` e `AudioSource` no Player; os outros ouvem na posição do avatar |
| Órbita | `PlanetOrbitMotion` em cada `*Wrapper`, centro no Sol, `SolarSystemMotionController` na raiz do FBX |
| Rotação | `PlanetOrbitMotion` na malha de cada planeta, com o perfil científico correspondente |
| Rótulos | Oito instâncias de `PlanetLabel.prefab`, uma por wrapper, com `overrideText` igual ao nome do corpo |
| Luz | `Sun Light` segue `Sun Sphere` |
| Nuvens | Filhos `Clouds` já presentes em Terra e Vênus. `PlanetCloudLayer` existe como script e não está atribuído na cena |
| Marcadores | Oito `InteractionOrb`, visíveis porque `showInteractionOrbs` está ligado |

Parâmetros do controlador na cena: `timeScale = 4320`, `distanceScale = 300`, orbes e rótulos visíveis. Com a Terra em 300 unidades, os raios de órbita gravados nos wrappers são as proporções do catálogo:

| Corpo | Perfil | Raio de órbita | Período (dias) | Dia sideral (h) | Obliquidade |
|-------|--------|----------------|----------------|-----------------|-------------|
| Mercúrio | 0 | 116,1 | 87,969 | 1407,6 | 0,034° |
| Vênus | 1 | 216,9 | 224,701 | −5832,6 | 177,36° |
| Terra | 2 | 300 | 365,256 | 23,934 | 23,44° |
| Marte | 3 | 457,2 | 686,98 | 24,623 | 25,19° |
| Júpiter | 4 | 1560,9 | 4332,59 | 9,925 | 3,13° |
| Saturno | 5 | 2861,1 | 10759,22 | 10,656 | 26,73° |
| Urano | 6 | 5757,3 | 30688,5 | −17,24 | 97,77° |
| Netuno | 7 | 9020,7 | 60182 | 16,11 | 28,32° |

Vênus e Urano usam dia sideral negativo: a rotação do corpo é retrógrada. Os wrappers não giram; só transladam em torno do eixo Y do Sol.

### 3.4 Sala, portais e sistema solar

`Interactive` agrupa o que está na sala, perto do spawn:

- `Treasure` — prefab com feedback de olhar, tag padrão `Untagged`. O clique completa a barra e escreve o nome no HUD; não teleporta.
- `Chair` — instância de `Cylinder Portal`. O prefab nasce com tag `Teleportable` e `ObjectBasicController`, então o olhar sustentado move o jogador para a borda do cilindro.
- `Sphere Portal` — o mesmo contrato, em forma de esfera, solto na sala.

No sistema solar o mesmo prefab `Sphere Portal` é reutilizado com o nome `Portal`. Há um portal filho de cada malha de planeta e mais um no `Sun Sphere`. Como o pai tem tag `Planet`, o HUD mostra `Planeta: <nome>` enquanto o olhar está no portal do corpo. O teleporte coloca o jogador na superfície do volume do portal, não no centro da malha do planeta. Em seguida o ponteiro ignora esse portal até o olhar sair, para o clique não se repetir no mesmo alvo.

`InteractionOrb` é visual. O prefab não tem collider de jogo nem tag de interação; o travamento de pose no wrapper existe para o marcador continuar no lugar autoral enquanto o wrapper orbita. `SolarSystemMotionController` liga e desliga todos eles juntos.

`Plane` tem tag `Environment` e um `MeshCollider`. O chão pode ser atingido pelo raio e não inicia foco. `SpaceSphere` é uma esfera de escala 50000 na origem, com collider, e funciona como casca de fundo atrás do sistema solar. `Sun Light` também está em `Environment`, para a luz não virar alvo de olhar se o raycast a encontrar.

### 3.5 Runtime da cena

```mermaid
sequenceDiagram
    participant Boot as Cena
    participant Card as InitCardboard
    participant Net as NetworkManager
    participant Player as Player local
    participant Gaze as CameraPointerController
    participant World as Sistema solar

    Boot->>Card: Start no Plane
    Card->>Card: skybox, brilho, parâmetros do visor
    Boot->>World: Awake dos wrappers e dos corpos
    World->>World: grava pose dos orbes e o offset até o Sol
    Boot->>Net: servidor sobe na build (porta 7777)
    Net->>Player: spawn do prefab
    Player->>Player: posição aleatória, câmera e canvas locais
    Player->>Gaze: Update só se isLocalPlayer
    Gaze->>World: raycast em portal, sala ou ambiente
    Note over Gaze,Player: teleporte altera a pose local
    Player->>Net: NetworkTransform envia pose ao servidor
    loop Cada frame
        World->>World: órbita no wrapper, rotação no corpo, rótulo no LateUpdate
    end
```

O que cada cliente calcula sozinho: órbitas, rotações, escala dos rótulos, posição da luz e visibilidade de orbes e rótulos. O que a rede replica: transform do jogador e pacotes de voz. Um teleporte é uma mudança de `transform.position` no dono; os outros clientes veem o avatar chegar à borda do portal porque o `NetworkTransform` está em `ClientToServer`.

A fase orbital não é reconciliada. Dois clientes que entram em momentos diferentes, ou que alterassem `timeScale` só de um lado, veriam os planetas em lugares diferentes. Na cena os valores estão iguais e fixos no componente, então a divergência prática vem do instante em que cada processo começou a simular.

### 3.6 Prefabs usados pela cena

| Prefab | Uso em The Room |
|--------|-----------------|
| `Player.prefab` | Spawn de rede. Não está colocado na cena |
| `Sphere Portal.prefab` | Um na sala, um por planeta e um no Sol. Na órbita o nome vira `Portal` |
| `Cylinder Portal.prefab` | Um na sala, renomeado `Chair` |
| `Treasure.prefab` | Um sob `Interactive` |
| `PlanetLabel.prefab` | Um por wrapper, texto fixo do corpo |
| `InteractionOrb.prefab` | Um por wrapper |
| `Sun Sphere` | Substitui o Sol do FBX; pai do portal solar |
| `SolarSystem.fbx` | Malhas dos corpos, raiz sob `Environment` |
| `Male_002.fbx` | Avatar filho do Player |

`InteractionMarker.prefab` existe no projeto e não é instanciado nesta cena.

### 3.7 Acoplamentos da cena que não são funcionalidade genérica

Dois componentes estão no objeto `Environment` e os scripts correspondentes não estão no repositório: `SolarSystemPortalAligner` e `PlanetWrapperSetup`. A hierarquia descrita acima é a que está salva na cena (wrappers, portais e rótulos já posicionados). Esses componentes não participam do comportamento em runtime enquanto os scripts estiverem ausentes.

`PlanetCloudLayer` e `SolarSystemSunReplacer` estão implementados e não aparecem como componentes desta cena. As nuvens da Terra e de Vênus são objetos já colocados na hierarquia. O Sol da cena é a instância de `Sun Sphere`.

---

## 4. Mapa de código

```
Assets/Scripts/
├── InitCardboard.cs                 sessão VR
├── CameraPointerController.cs       olhar, permanência, teleporte
├── CameraEditorController.cs        mouse no Editor
├── ObjectBasicController.cs         material de foco
├── UILogger.cs                      texto do HUD
├── VoiceChatController.cs           voz PCM16
├── SunLightController.cs            luz pontual no Sol
├── SolarSystemSunReplacer.cs        troca da malha do Sol
├── PlanetCloudLayer.cs              casca de nuvens
├── PlanetLabel.cs                   rótulo com tamanho aparente
└── PlanetaryMotion/
    ├── SolarPlanetId.cs
    ├── PlanetScientificData.cs      catálogo
    ├── PlanetOrbitCalculator.cs
    ├── PlanetSpinCalculator.cs
    ├── SolarSystemLocator.cs
    ├── PlanetOrbitMotion.cs
    └── SolarSystemMotionController.cs
```

Bibliotecas que a arquitetura assume e não reimplementa: Mirror (identidade, spawn, transform, comandos de voz), Google Cardboard XR Plugin (parâmetros do visor e pose rastreada no Player) e TextMesh Pro (HUD e rótulos).
