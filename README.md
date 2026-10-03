# JevApplicationTeste

Laboratório em C# / ASP.NET Core (.NET 10) para aprender integração HTTP com o Jev da TypeSafe AI. Classifica a descrição de um chamado e sugere equipe e prioridade. Não encaminha chamados de verdade.

## 1. Rodar primeiro, sem chave

Instale o SDK .NET 10. No terminal:

```bash
git clone https://github.com/GabrielMorais99/JevApplicationTeste.git
cd JevApplicationTeste
dotnet run --project src/JevApplicationTeste.Api
```

Abra http://localhost:5080/health. O resultado esperado é `{"status":"ok"}`.
Abra `JevApplicationTeste.http` no Visual Studio com suporte a .NET 10, ou envie pelo Insomnia:

- Método: POST
- URL: http://localhost:5080/chamados/classificar
- Corpo: JSON

```json
{
  "descricao": "A integração de funcionários parou e nenhum cadastro chega ao sistema desde ontem. Toda a operação está bloqueada."
}
```

O modo inicial é **simulado**: sempre retorna uma fixture fixa, com categoria `integracao`, prioridade `indefinida` e revisão humana. Ele serve para aprender o caminho da requisição e não analisa o texto. O campo `modo` torna essa distinção explícita. Não há interface Swagger nesta versão; use o arquivo HTTP ou Insomnia.

## 2. Entender o caminho do código

1. `Program.cs` registra dependências e configura o cliente HTTP. O container de DI entrega os serviços aos construtores.
2. `ChamadosController` recebe e valida a descrição (10 a 5.000 caracteres).
3. `ClassificacaoService` define as opções de categoria e prioridade. No modo real, envia ambas em uma única chamada.
4. `JevClient` autentica e chama a API. `await` permite esperar a resposta sem manter a thread bloqueada durante a espera de rede; não torna a API externa mais rápida.
5. O serviço valida a resposta e compara as confianças com o limite configurado. O mapeamento de categoria para equipe é uma regra C# determinística.

O Jev é o modelo externo; ASP.NET Core é o framework deste projeto. `state` contém o relato; `questions` contém as perguntas e opções definidas por nós. `Choice` escolhe entre opções, `Score` avalia níveis ordenados e `Noul` avalia uma pergunta sim/não. Este laboratório começa apenas com `Choice`.

## 3. Ativar a integração real

Obtenha sua chave no console oficial: https://console.typesafe.ai/ . Confira acesso e cobrança na sua conta. Não envie a chave no chat nem faça commit dela.

Na raiz do repositório, configure localmente:

```bash
dotnet user-secrets set "Jev:ApiKey" "SUA_CHAVE_LOCAL" --project src/JevApplicationTeste.Api
dotnet user-secrets set "Jev:UseMock" "false" --project src/JevApplicationTeste.Api
dotnet run --project src/JevApplicationTeste.Api
```

O perfil de execução usa Development para carregar user-secrets. Alternativa: variáveis `Jev__ApiKey` e `Jev__UseMock=false`. Reinicie a aplicação após alterar a configuração. Sem chave, o modo real falha na inicialização com mensagem clara.

A integração usa `POST https://api.typesafe.ai/v1/systemone`, Bearer e modelo configurável `jev-latest`. A resposta inclui o modelo efetivamente usado. Fonte: https://docs.typesafe.ai/api . O alias pode mudar; para comparações reproduzíveis, configure uma versão disponível na sua conta.

## 4. Ler o resultado e experimentar

- `categoria` e `prioridade`: opção, confiança e probabilidades recebidas.
- `equipeSugerida`: regra local (`integracao` → backend, `infraestrutura` → infraestrutura, demais → suporte).
- `requerRevisaoHumana`: verdadeiro se alguma confiança for menor que 0,8, categoria for `outros` ou prioridade for `indefinida`.
- O limiar de 0,8 é didático e precisa ser avaliado com exemplos rotulados. Confiança não é garantia de acerto nem sinônimo de probabilidade da opção. Referência: https://docs.typesafe.ai/confidence .

Compare no modo real: um incidente de integração bloqueada; uma solicitação de redefinição de senha; uma sugestão de melhoria; um relato sem detalhes. Anote o resultado esperado antes de chamar a API. Observe também erros com alta confiança.

Exercício seguinte: adicionar a categoria `banco_de_dados`, descrever seus critérios e ajustar o mapeamento de equipe. Depois compare os mesmos exemplos antes e depois da alteração.

## 5. Validação e limites

```bash
dotnet build src/JevApplicationTeste.Api -c Release
python tests/smoke.py
```

O workflow do GitHub compila e executa testes HTTP do modo simulado, incluindo entradas inválidas. Eles não provam a acurácia do Jev. A integração real precisa ser testada com uma chave válida; ela não foi executada na criação do laboratório.

Timeout externo retorna 504; falha de rede, resposta incompatível ou status de erro externo retornam 502. Não há retry automático nesta versão: chamadas extras podem consumir cota. Não há banco, autenticação de usuários ou publicação. Execute localmente; antes de expor o endpoint, implemente autenticação e limites de uso para proteger sua cota.
