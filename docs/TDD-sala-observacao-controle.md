# TDD — Sala de observação e controle

**Projeto:** meta-class-alpha  
**Escopo:** features de sala de controle, retorno via portal e painel descritivo de UI  
**Status:** a fazer  
**Contexto técnico atual:** Unity + Mirror (multiplayer) + Cardboard XR; interação por gaze (`CameraPointerController`); teleporte via tag `Teleportable`; UI HUD no Canvas World Space do Player (`UILogger` / TMP).

---

## 1. Objetivo

Permitir que o jogador:

1. Observe e **controle o cenário** (ex.: sistema solar) a partir de uma sala dedicada.
2. **Volte à sala de controle** a qualquer momento por um objeto de interação sob os pés.
3. Veja um **painel descritivo na UI** ao entrar em áreas específicas do cenário.

---

## 2. Feature: Sala de observação e controle

### 2.1 Descrição

Espaço físico na cena (ou cena aditiva) onde o player nasce / retorna e de onde pode alterar parâmetros do mundo (tempo, escala de distância, visibilidade de orbs/labels, etc.).

### 2.2 Requisitos funcionais

| ID | Requisito |
|----|-----------|
| SC-01 | Existe uma área identificável “Sala de Controle” com spawn point do player. |
| SC-02 | Controles do cenário são acionáveis por gaze (mesmo padrão de `Interactive` / painéis). |
| SC-03 | Controles incluem, no mínimo, os já existentes em `SolarSystemMotionController`: `timeScale`, `distanceScale`, `showInteractionOrbs`, `showPlanetLabels`. |
| SC-04 | Alterações aplicadas em runtime refletem para todos os clientes (Mirror) ou, se single-player local, só ao host — **definir na implementação: sync via NetworkBehaviour / ServerRpc**. |
| SC-05 | A sala permanece acessível independentemente da posição orbital dos planetas. |

### 2.3 Requisitos não funcionais

- Layout legível em VR (elementos à altura do olhar / alcance do raycast).
- Não bloquear o fluxo de teleporte para planetas a partir da sala (se houver portais de ida).

### 2.4 Design técnico sugerido

```
ControlRoom (root)
 ├── SpawnPoint
 ├── ControlPanel (UI world / Interactables)
 │    ├── TimeScaleControl
 │    ├── DistanceScaleControl
 │    ├── ToggleOrbs
 │    └── ToggleLabels
 └── ExitPortals (opcional: ida ao sistema solar)
```

- Reutilizar `SolarSystemMotionController` como fonte de verdade dos parâmetros.
- Novos scripts finos (ex.: `ControlRoomPanel`) apenas leem/escrevem no controller; não duplicar lógica orbital.

### 2.5 Critérios de aceite (testes)

- [ ] Player spawna na sala de controle.
- [ ] Alterar `timeScale` / `distanceScale` na sala muda o movimento dos planetas.
- [ ] Toggles de Orb/Label escondem/mostram objetos corretos.
- [ ] Em multiplayer, estado do cenário fica consistente entre peers (conforme decisão de sync).

---

## 3. Feature: Portal para sala de controle (sob os pés)

### 3.1 Descrição

Objeto de interação posicionado **abaixo dos pés do player** que, ao ser ativado (gaze + hold atual), teleporta o player de volta à sala de controle.

### 3.2 Requisitos funcionais

| ID | Requisito |
|----|-----------|
| PT-01 | Existe um interactable “ReturnToControlRoom” ancorado ao player (filho do Player) ou no chão local sob o player. |
| PT-02 | Fica sempre sob os pés / no plano do chão relativo ao player, sem atrapalhar a visão frontal. |
| PT-03 | Usa o fluxo de interação existente (`Teleportable` ou extensão do `CameraPointerController`). |
| PT-04 | Ao completar o clique por gaze, o player é movido para o `SpawnPoint` / âncora da sala de controle. |
| PT-05 | Após o teleporte, o ponteiro é resetado (equivalente a `DisablePointerForTarget` / limpar gaze). |
| PT-06 | Funciona estando em qualquer planeta / área do sistema solar. |

### 3.3 Decisão de design (recomendada)

**Filho do Player** (não um portal estático no mundo):

- Segue o player automaticamente.
- Posição local tipica: `(0, -offsetY, 0)` ou ligeiramente à frente no chão.
- Tag: `Teleportable` **ou** tag/nova interface `ReturnPortal` tratada no `CameraPointerController`.
- Destino: `Transform` da sala de controle (referência serializada ou `ControlRoomLocator.FindSpawn()`).

> Hoje o teleporte `Teleportable` move o player para a **borda do alvo** (`GetTargetEdgePosition`). Para retorno à sala, o destino deve ser um **ponto fixo** (spawn), não a borda do objeto sob os pés — implementar ramo específico.

### 3.4 Design técnico sugerido

```csharp
// Conceito
public class ReturnToControlRoomPortal : MonoBehaviour
{
    [SerializeField] Transform controlRoomSpawn;
    // OnActivate(player) => player.position = spawn.position (+ NetworkTransform sync)
}
```

Integração em `CameraPointerController.handlePointerClick`:

1. Se alvo tem `ReturnToControlRoomPortal` → teleportar para `controlRoomSpawn`.
2. Senão, manter comportamento atual de `Teleportable`.

### 3.5 Critérios de aceite (testes)

- [ ] Objeto visível/interagível sob os pés em play mode.
- [ ] Gaze + hold teleporta para a sala de controle.
- [ ] Posição final ≈ spawn da sala (tolerância definida, ex. 0.5 m).
- [ ] Não teleporta para a “borda” do próprio portal sob os pés.
- [ ] Em multiplayer, só o player local se move (NetworkTransform).

---

## 4. Feature: Painel descritivo na UI

### 4.1 Descrição

Quando o player entra em uma **área determinada** (trigger / volume), um painel na UI exibe informações contextuais (texto, título, opcionalmente imagem). Ao sair da área, o painel some ou volta ao estado vazio.

### 4.2 Requisitos funcionais

| ID | Requisito |
|----|-----------|
| UI-01 | Áreas do cenário possuem um volume (Collider `isTrigger`) com dados descritivos. |
| UI-02 | Ao entrar na área (player local), o painel HUD mostra título + corpo do texto. |
| UI-03 | Ao sair, o painel é ocultado ou limpo. |
| UI-04 | Se duas áreas se sobrepuserem, prevalece a última entrada ou a de maior prioridade (definir: **última entrada**). |
| UI-05 | Conteúdo é configurável por área (ScriptableObject ou campos no componente). |
| UI-06 | Apenas o player local vê o painel (UI do Canvas local). |

### 4.3 Design técnico sugerido

```
Player Canvas (já existente)
 └── InfoPanel (novo)
      ├── Title (TMP)
      └── Body (TMP)

InfoAreaTrigger (no mundo)
 ├── Collider (trigger)
 └── InfoAreaContent (title, body, optional sprite)
```

Scripts:

- `InfoPanelUI` — no Player; métodos `Show(InfoContent)`, `Hide()`.
- `InfoAreaTrigger` — `OnTriggerEnter/Exit` filtrando o player local (`isLocalPlayer`).

Reutilizar padrão visual do HUD atual (`UILogger`), sem misturar logs de gaze com o painel descritivo (painéis separados).

### 4.4 Dados de conteúdo (exemplo)

```csharp
[System.Serializable]
public struct InfoAreaContent
{
    public string title;
    [TextArea] public string body;
}
```

Casos de uso iniciais sugeridos:

- Próximo à Earth → “Terra — …”
- Sala de controle → “Sala de Controle — como usar os painéis”
- Próximo a um portal de planeta → dica de teleporte

### 4.5 Critérios de aceite (testes)

- [ ] Entrar no trigger mostra o painel com o texto correto.
- [ ] Sair esconde o painel.
- [ ] Outro player na área não afeta a UI local.
- [ ] Área sem conteúdo não quebra (no-op ou mensagem vazia).

---

## 5. Dependências e reuso

| Sistema existente | Uso |
|-------------------|-----|
| `CameraPointerController` | Gaze, click, teleporte |
| Tag `Teleportable` / `Interactive` | Interação |
| `SolarSystemMotionController` | Controles de cenário |
| `PlanetLabel` / InteractionOrb | Toggles da sala |
| Player Canvas + TMP | Painel descritivo |
| Mirror `NetworkTransform` | Sync de posição no retorno |

---

## 6. Ordem de implementação sugerida

1. **Sala de controle** (espaço + spawn + wiring dos controles ao `SolarSystemMotionController`).
2. **Portal de retorno** (objeto sob os pés + ramo de teleporte para spawn).
3. **Painel descritivo** (UI + triggers de área + conteúdos).

---

## 7. Fora de escopo (neste TDD)

- Novos planetas / conteúdo científico além do texto do painel.
- Inventário, missões ou progressão.
- Redesign completo do HUD.
- Troca de cena completa (pode ser a mesma `TheRoom` com região dedicada).

---

## 8. Riscos

| Risco | Mitigação |
|-------|-----------|
| Teleporte atual usa borda do alvo, inadequado para portal sob os pés | Ramo dedicado com destino fixo |
| Trigger 3D em VR com CharacterController / sem collider no player | Garantir collider no Player ou detector baseado em posição |
| Sync multiplayer dos controles da sala | `SyncVar` / mensagens Mirror no controller |
| Objeto sob os pés captura gaze acidentalmente | Ângulo mínimo / só ativo se olhar para baixo |

---

## 9. Definição de pronto (DoD)

- Features SC, PT e UI implementadas conforme critérios de aceite.
- Documentação de como configurar uma nova `InfoArea` e o spawn da sala.
- Sem regressão no teleporte atual para planetas/`Teleportable`.
