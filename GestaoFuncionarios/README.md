# Gestão de Funcionários — Vertex Solutions (projeto de portfólio)

CRUD completo de funcionários com geração de relatórios em **PDF**, **Excel** e **JSON**, construído em ASP.NET Core MVC.

> **Contexto**: este projeto é uma recriação própria, do zero, com dados e empresa fictícios ("Vertex Solutions"), inspirada em um desafio técnico real que resolvi no trabalho. Nenhum código, dado ou nome do sistema original foi reaproveitado — a estrutura (CRUD com JSON como storage, grid, exportação em 3 formatos) foi reconstruída por mim, aplicando o que aprendi no processo, incluindo bugs reais que encontrei e corrigi.

## Stack
- **Back-end**: ASP.NET Core 8 MVC (C#)
- **Front-end**: Razor Views + jQuery + Bootstrap 4 + [bootstrap-table](https://bootstrap-table.com/)
- **"Banco de dados"**: arquivo JSON (proposital — simula persistência sem precisar de um banco de verdade pra rodar o projeto)
- **Geração de Excel**: [ClosedXML](https://github.com/ClosedXML/ClosedXML)
- **Geração de PDF**: [QuestPDF](https://www.questpdf.com/) (licença Community, gratuita)

## Funcionalidades
- CRUD completo (criar, listar, editar, excluir) com um único formulário reutilizado pra criar/editar
- Autocomplete de busca de funcionário já cadastrado
- Grid paginada (bootstrap-table)
- Exportação da lista atual da grid para **Excel**, **PDF** ou **JSON**, com download automático no navegador
- Validações de formulário: máscara de data (`dd/mm/aaaa`), filtro de digitação numérica no salário

## Como rodar localmente
Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
cd GestaoFuncionarios
dotnet restore
dotnet run
```

Acesse `https://localhost:5001` (ou a porta que o console mostrar). A tela de Cadastro de Funcionários já abre na rota padrão.

## Decisões técnicas que valem destacar
- **Sem banco de dados de propósito**: o "repositório" (`FuncionarioJsonRepository`) lê/escreve um `Data/funcionarios.json` inteiro a cada operação — simples de entender e rodar sem infraestrutura, mas documentado no código como um trade-off consciente (não é como eu faria em produção com volume real de dados/usuários simultâneos).
- **Sem `Content-Type` sniffing na exportação**: a API de relatórios sempre devolve o status HTTP correto (200 em sucesso, 400 em erro), então o front só precisa checar `response.ok` — uma simplificação que só foi possível porque aqui é tudo um projeto ASP.NET Core moderno (no desafio original, uma camada legada obrigava a checar o `Content-Type` da resposta pra detectar erros "disfarçados de sucesso").
- **Bugs que encontrei e corrigi durante o desenvolvimento original, já nascendo corrigidos aqui**:
  - Duplo-clique no botão de excluir de uma linha da grid não deve também acionar o modo de edição da linha (conflito de eventos).
  - O campo de data, ao ser preenchido automaticamente (edição), precisa ser formatado antes de entrar no campo — colocar o valor "cru" quebra o salvamento.
  - Editar um registro cujo id não existe mais não deve derrubar a aplicação (`NullReferenceException`) — deve avisar quem chamou de forma controlada.

## Estrutura
```
Controllers/
  FuncionariosController.cs   -> CRUD + tela
  RelatoriosController.cs     -> geração de PDF/Excel/JSON
Models/
  FuncionarioModel.cs
  FiltroExportacaoModel.cs
Services/
  FuncionarioJsonRepository.cs -> toda a lógica de ler/escrever o "banco" JSON
Views/Funcionarios/Index.cshtml -> tela (form + grid + JS)
Data/funcionarios.json          -> dados fictícios de seed
```
