# 🗂️ Gestão de Funcionários — Vertex Solutions

CRUD completo de funcionários com geração de relatórios em **PDF**, **Excel** e **JSON**, construído em ASP.NET Core MVC.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![License](https://img.shields.io/badge/license-MIT-blue)

> **Contexto do projeto**: esta é uma recriação própria, do zero, com empresa e dados fictícios ("Vertex Solutions"), inspirada em um desafio técnico real que resolvi no trabalho. Nenhum código, dado ou nome do sistema original foi reaproveitado — a estrutura (CRUD com JSON como storage, grid, exportação em 3 formatos) foi reconstruída por mim, aplicando o que aprendi no processo, incluindo bugs reais que encontrei e corrigi ao longo do caminho.

## ✨ Funcionalidades

- CRUD completo (criar, listar, editar, excluir) com um único formulário reutilizado pra criar/editar
- Autocomplete de busca de funcionário já cadastrado
- Grid paginada e responsiva ([bootstrap-table](https://bootstrap-table.com/))
- Exportação da lista atual da grid para **Excel**, **PDF** ou **JSON**, com download automático no navegador
- Validações de formulário: máscara de data (`dd/mm/aaaa`), filtro de digitação numérica no campo de salário

## 🛠️ Stack

| Camada | Tecnologia |
|---|---|
| Back-end | ASP.NET Core 8 MVC (C#) |
| Front-end | Razor Views + jQuery + Bootstrap 4 + bootstrap-table |
| "Banco de dados" | Arquivo JSON (proposital — ver [Decisões técnicas](#-decisões-técnicas)) |
| Geração de Excel | [ClosedXML](https://github.com/ClosedXML/ClosedXML) |
| Geração de PDF | [QuestPDF](https://www.questpdf.com/) (licença Community, gratuita) |

## 🚀 Como rodar localmente

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/<seu-usuario>/gestao-funcionarios.git
cd gestao-funcionarios/GestaoFuncionarios
dotnet restore
dotnet run
```

Acesse `http://localhost:5000` no navegador. A tela de Cadastro de Funcionários já abre direto na rota padrão, com 4 funcionários fictícios pré-cadastrados.

## 📁 Estrutura

```
Controllers/
  FuncionariosController.cs   -> CRUD + serve a tela
  RelatoriosController.cs     -> geração de PDF/Excel/JSON
Models/
  FuncionarioModel.cs
  FiltroExportacaoModel.cs
Services/
  FuncionarioJsonRepository.cs -> toda a lógica de ler/escrever o "banco" JSON
Views/Funcionarios/Index.cshtml -> tela (formulário + grid + JS)
Data/funcionarios.json          -> dados fictícios de seed
```

## 💡 Decisões técnicas

- **Sem banco de dados de propósito**: o `FuncionarioJsonRepository` lê/escreve um `Data/funcionarios.json` inteiro a cada operação — simples de entender e rodar sem infraestrutura nenhuma. É um trade-off consciente (não é como eu faria em produção com volume real de dados ou usuários simultâneos), documentado direto no código.
- **API de exportação com status HTTP corretos**: `RelatoriosController` sempre devolve o status certo (`200` em sucesso, `400` em erro), então o front só precisa checar `response.ok` — sem precisar inspecionar o `Content-Type` da resposta pra descobrir se algo deu errado.
- **Bugs reais que encontrei e corrigi durante o desenvolvimento**, documentados no código onde aconteceram:
  - Duplo-clique no botão de excluir de uma linha da grid não deve também acionar o modo de edição da linha (conflito de eventos — usei `e.target.closest(...)` pra distinguir).
  - O campo de data, ao ser preenchido automaticamente na edição, precisa ser formatado (`moment().format(...)`) antes de entrar no campo — colocar o valor "cru" vindo do backend quebrava o salvamento.
  - Editar um registro cujo `id` não existe mais não deve derrubar a aplicação (`NullReferenceException`) — deve avisar quem chamou de forma controlada.

## 🖼️ Preview

*(adicione aqui um print ou GIF da tela funcionando — vale muito num README de portfólio)*

## 📄 Licença

MIT — sinta-se livre pra usar como referência de estudo.
