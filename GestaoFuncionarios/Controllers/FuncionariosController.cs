using GestaoFuncionarios.Models;
using GestaoFuncionarios.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoFuncionarios.Controllers;

// Equivalente ao TIController do projeto original -> serve a tela (Index) e os 4 endpoints do CRUD.
public class FuncionariosController : Controller
{
    private readonly FuncionarioJsonRepository _repositorio;

    // Injeção de dependência: o ASP.NET Core cria o FuncionarioJsonRepository automaticamente
    // e entrega aqui no construtor -> não precisamos dar "new FuncionarioJsonRepository()" na mão
    // em cada Action, como seria feito no MVC clássico.
    public FuncionariosController(FuncionarioJsonRepository repositorio)
    {
        _repositorio = repositorio;
    }

    // GET /Funcionarios  (ou só "/", já que é a rota padrão -> ver Program.cs)
    public IActionResult Index()
    {
        return View();
    }

    // GET /Funcionarios/Listar
    [HttpGet]
    public IActionResult Listar()
    {
        var lista = _repositorio.Listar();
        return Json(lista);
        // Nota de estudo: no ASP.NET MVC clássico (o original), era OBRIGATÓRIO passar
        // JsonRequestBehavior.AllowGet aqui, senão o framework bloqueava JsonResult em GET
        // (proteção antiga contra JSON hijacking). No ASP.NET Core esse bloqueio não existe mais
        // -> Json(...) simples já funciona em GET, sem parâmetro extra nenhum.
    }

    // GET /Funcionarios/Pesquisar?termo=xxx
    // Substitui a chamada à API externa "Pesquisas/Empregados" do projeto original (que não existe
    // aqui, é um sistema interno da empresa real) por uma busca simples dentro dos próprios dados já
    // cadastrados -> mesmo papel (autocomplete ajuda a preencher o formulário), fonte diferente.
    [HttpGet]
    public IActionResult Pesquisar(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo))
            return Json(new List<object>());

        var resultado = _repositorio.Listar()
            .Where(f => f.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))
            .Select(f => new { nome = f.Nome, matricula = f.Matricula, departamento = f.Departamento })
            .Take(10)
            .ToList();

        return Json(resultado);
    }

    // POST /Funcionarios/Adicionar
    [HttpPost]
    public IActionResult Adicionar([FromForm] FuncionarioModel funcionario)
    {
        // [FromForm] = o Model Binder do ASP.NET Core casa os campos do POST (form-urlencoded,
        // do $.ajax com "data: {...}") com as propriedades do FuncionarioModel automaticamente,
        // pelo NOME -> não precisa listar parâmetro por parâmetro como no MVC clássico.
        var criado = _repositorio.Adicionar(funcionario);
        return Json(new { sucesso = true, id = criado.Id });
    }

    // POST /Funcionarios/Editar
    [HttpPost]
    public IActionResult Editar([FromForm] FuncionarioModel funcionario)
    {
        var encontrado = _repositorio.Editar(funcionario);

        if (!encontrado)
        {
            // Esse é o ponto que corrigimos no projeto original: em vez de deixar quebrar
            // (NullReferenceException) OU salvar silenciosamente sem avisar ninguém, aqui a gente
            // devolve uma resposta explícita -> o front consegue mostrar uma mensagem de erro real
            // se o registro não existir mais (por exemplo, foi excluído por outra pessoa nesse meio-tempo).
            return Json(new { sucesso = false, mensagem = "Funcionário não encontrado (id inexistente)." });
        }

        return Json(new { sucesso = true });
    }

    // POST /Funcionarios/Excluir
    [HttpPost]
    public IActionResult Excluir(int id)
    {
        _repositorio.Excluir(id);
        return Json(new { sucesso = true });
    }
}
