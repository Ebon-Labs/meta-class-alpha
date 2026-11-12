# Código limpo — consulta para agentes

**Projeto:** meta-class-alpha  
**Uso:** ler este arquivo antes de criar ou alterar código. É a referência de como produzir código neste repositório.  
**Palavras-chave:** clean code, SOLID, arquitetura, testes, NUnit, Edit Mode, MetaClass  
**Arquitetura vigente:** [arquitetura.md](arquitetura.md)

Três regras. As três valem ao mesmo tempo. Uma entrega que cumpre duas e fere a terceira não está pronta.

---

## Regra 1 — Não ferir a arquitetura

O código novo entra na arquitetura que já existe. Não cria um segundo caminho para a mesma responsabilidade.

Antes de editar, leia a seção correspondente de [arquitetura.md](arquitetura.md). A arquitetura tem duas camadas:

- **Genérica.** Scripts e contratos que não conhecem a sala, os planetas nem o enredo. Dependem de tags, nomes de âncora e prefabs.
- **Cena.** Menu e SolarSystem instanciam esses contratos. Objetos, textos e números de uma cena ficam na cena.

Um script genérico não procura um objeto exclusivo de SolarSystem para decidir o que fazer, salvo os nomes de busca já documentados para o Sol e a raiz `SolarSystem`. Uma cena não reimplementa gaze, órbita, voz ou presença em rede.

Contratos que a mudança tem de preservar:

| Contrato | O que permanece |
|----------|-----------------|
| Build | A primeira cena é Menu. SolarSystem é a cena online |
| Rede | Um `NetworkManager`, na cena Menu, com `dontDestroyOnLoad`. SolarSystem não ganha outro |
| Menu | Toque na tela. Não é `Physics.Raycast`. Criar Sala e Buscar Sala só giram o carrossel |
| Sala | O anfitrião nasce do toque em `SolarSystemThumb` ou do Host do HUD. Entrar numa sala da `RoomList` é só cliente |
| Giro da sala | 45° no sentido horário em torno da `HostArea`, em 3 s. Yaw horário é ângulo negativo em Y |
| Viagem | `MoveToEdge` e `PassengerHold`. O jogador acompanha o pai. A rotação do jogador não é escrita à parte |
| Simulação | Órbita, rótulo e luz rodam em cada cliente. A rede leva presença e voz |

Se a mudança precisa de um contrato novo, o contrato entra primeiro na arquitetura e o código o segue. Não o contrário.

---

## Regra 2 — Clean Code e SOLID

O estilo do repositório é uma função pura e estática mais um `MonoBehaviour` fino. A regra de negócio fica na função, que não precisa de cena nem de `Play`. O componente lê o Unity e chama a função.

### Responsabilidade única

Um tipo muda por um motivo. Calculadora, política e componente de cena não se misturam. `PlanetOrbitCalculator` calcula. `PlanetOrbitMotion` aplica. `GuestMutePolicy` decide quem silencia. O botão só alterna o estado e troca o ícone.

### Aberto para extensão, fechado para alteração

Comportamento novo entra por tipo novo, dado novo ou parâmetro do contrato. Não por um `if` com o nome de uma sala dentro de um script genérico. Não por copiar um componente e mudar um número.

### Substituição

Quem implementa um contrato existente cumpre o contrato inteiro. `IScreenPlayback` recebe `Play`, `Pause` e `Stop`. Um substituto não deixa um desses três sem efeito.

### Interface pequena

O chamador recebe só o que usa. Preferir uma função estática com os valores de que precisa a um componente que alcança a cena inteira por `Find`.

### Inversão de dependência

A regra depende de tag, interface ou valor já resolvido. A cena concreta fica na borda, no componente. Teste a regra sem subir a cena.

### Código que se lê de uma vez

- O nome diz o que a função devolve ou o que o tipo faz.
- A função faz uma coisa. Se o nome precisa de "e", separe.
- Sem código morto, sem parâmetro sem uso, sem comentário que repete a linha.
- Comentário só para um contrato que o nome não carrega, como o sentido do yaw ou a ordem de um clique.
- Uma mudança não reforma o arquivo ao lado. O diff fica no comportamento pedido.

---

## Regra 3 — Testes em toda modificação e em todo arquivo novo

Código de produção novo ou alterado só fica pronto com teste no mesmo conjunto de mudanças.

| Situação | Teste |
|----------|--------|
| Arquivo de script novo | Arquivo de teste novo cobrindo o comportamento público |
| Comportamento alterado | Teste novo ou teste existente atualizado para o contrato novo |
| Correção | Teste que falharia no código anterior e passa no corrigido |
| Só documentação, cena sem lógica, ou asset | Sem teste de código |

Onde colocar: `Assets/Scripts/<Área>/Editor/<Tipo>Tests.cs`, ao lado do script. A pasta `Editor` deixa o teste fora do build do player. O teste é NUnit, Edit Mode, método `[Test]`. Não precisa abrir cena nem dar Play.

O que afirmar:

- A função pura pública, com os valores do contrato (ângulos, tempos, nomes, sinais).
- O caso de borda que a mudança atravessa.
- O comportamento vizinho que não pode regredir.

O teste não sobe `NetworkManager`, não depende da hierarquia salva na cena e não afirma detalhe privado. Se a regra está presa dentro do `MonoBehaviour`, extraia a função pura e teste essa função.

Um teste que foi afrouxado para o código errado passar não conta. O teste descreve o contrato. O código se ajusta a ele.

---

## Ordem de trabalho

1. Ler a seção de [arquitetura.md](arquitetura.md) que a mudança toca.
2. Encaixar a regra numa função pura, sem ferir os contratos da Regra 1.
3. Ligar a função no componente, se a cena precisar chamá-la.
4. Escrever ou atualizar o teste da Regra 3 antes de dar a mudança por pronta.
5. Se a arquitetura mudou de fato, atualizar [arquitetura.md](arquitetura.md) no mesmo conjunto de mudanças.
