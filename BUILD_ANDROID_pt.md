# Build Android (APK)

> **Idiomas:** [English](BUILD_ANDROID.md) | Português (BR)

Guia para gerar um APK do **meta-class-alpha** para testes em dispositivo com Google Cardboard.

## Configuração atual do projeto

| Item | Valor |
|------|-------|
| Unity | 6000.3.6f1 (Unity 6.3) |
| Cena de build | `Assets/Scenes/SolarSystem.unity` |
| Package Name | `com.EbonLabs.metaclassalpha` |
| Versão | 0.1 (versionCode: 1) |
| Min API Level | 25 (Android 7.1) |
| Scripting Backend | IL2CPP |
| Arquiteturas | ARMv7 + ARM64 |
| XR | Google Cardboard (Android) |
| Keystore | Não configurado (usa debug keystore em builds de teste) |

## Pré-requisitos

No **Unity Hub**, instale o editor **6000.3.6f1** com o módulo:

- **Android Build Support**
  - Android SDK & NDK Tools
  - OpenJDK

Se algo estiver faltando, o Unity costuma solicitar a instalação ao trocar a plataforma para Android.

## Gerar APK pelo Editor

1. Abra o projeto no Unity.
2. Vá em **File → Build Profiles** (Unity 6) ou **File → Build Settings**.
3. Selecione **Android** e clique em **Switch Platform** (aguarde a reimportação).
4. Confirme que a cena `Assets/Scenes/SolarSystem.unity` está incluída no build.
5. Revise **Edit → Project Settings → Player → Android**:
   - **Package Name**: `com.EbonLabs.metaclassalpha`
   - **Minimum API Level**: 25
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: ARMv7 e ARM64
6. Para gerar **APK** (e não AAB):
   - No perfil de build, desmarque **Build App Bundle (Google Play)** se estiver ativo.
7. Clique em **Build** (ou **Build And Run** com o celular conectado via USB).

O APK será salvo no caminho escolhido, por exemplo:

```
Builds/meta-class-alpha.apk
```

## Assinatura do app

O projeto não possui keystore de release configurado. Para testes locais, o Unity usa automaticamente o **debug keystore**.

Para publicar na Google Play Store:

1. Abra **Project Settings → Player → Android → Publishing Settings**.
2. Crie ou selecione um **keystore**.
3. Defina o **alias** e a senha.
4. Guarde o arquivo `.keystore` em local seguro — sem ele não é possível publicar atualizações do mesmo app.

## Build por linha de comando (opcional)

Requer um script de build em `Assets/Editor/`. Exemplo de invocação:

```bash
/path/to/Unity \
  -quit -batchmode -nographics \
  -projectPath "/caminho/para/meta-class-alpha" \
  -buildTarget Android \
  -executeMethod AndroidBuild.BuildApk
```

Substitua `/path/to/Unity` pelo caminho real do executável do Unity 6.3 no seu sistema.

## Testar no celular

1. Ative **Opções do desenvolvedor** e **Depuração USB** no aparelho.
2. Conecte o celular ao computador ou transfira o APK manualmente.
3. Instale e abra o app com o Google Cardboard acoplado ao telefone.

> O Cardboard deve ser testado em dispositivo físico. O modo VR não funciona de forma completa no Editor.

## Problemas comuns

| Problema | Solução |
|----------|---------|
| SDK ou NDK não encontrado | Unity Hub → Installs → engrenagem → Add Modules |
| Build gera `.aab` em vez de `.apk` | Desative **Build App Bundle** no perfil de build |
| App não instala no celular | Ative depuração USB e aceite a autorização no aparelho |
| Cardboard não responde | Teste em dispositivo físico; calibre o visor pelo app |
