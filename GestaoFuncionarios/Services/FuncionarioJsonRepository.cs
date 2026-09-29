using System.Text.Json;
using GestaoFuncionarios.Models;

namespace GestaoFuncionarios.Services;

// No projeto original, cada Action do TIController repetia a mesma lógica de abrir/ler/escrever
// o arquivo JSON na mão. Aqui isso foi extraído pra uma classe só (repositório), registrada via
// injeção de dependência -- assim o Controller só chama _repositorio.Listar()/.Adicionar(...)/etc,
// sem se preocupar com caminho de arquivo ou serialização. É uma organização mais "profissional"
// pra mostrar em portfólio, mas o CONCEITO por trás (ler tudo, alterar em memória, salvar tudo de
// novo -> sem banco de dados de verdade) é exatamente o mesmo do projeto original.
public class FuncionarioJsonRepository
{
    private readonly string _caminhoArquivo;
    private static readonly object _travaDeEscrita = new();
    // lock simples -> evita que duas requisições escrevam no arquivo ao mesmo tempo e corrompam o JSON
    // (o projeto original não tinha essa proteção; é uma melhoria que vale citar no README)

    public FuncionarioJsonRepository(IWebHostEnvironment ambiente)
    {
        // IWebHostEnvironment.ContentRootPath é o equivalente moderno do HttpContext.Server.MapPath
        // do ASP.NET MVC clássico -> resolve caminho físico real no disco
        _caminhoArquivo = Path.Combine(ambiente.ContentRootPath, "Data", "funcionarios.json");
    }

    public List<FuncionarioModel> Listar()
    {
        if (!File.Exists(_caminhoArquivo))
            return new List<FuncionarioModel>();

        var texto = File.ReadAllText(_caminhoArquivo);
        if (string.IsNullOrWhiteSpace(texto))
            return new List<FuncionarioModel>();

        return JsonSerializer.Deserialize<List<FuncionarioModel>>(texto,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<FuncionarioModel>();
    }

    private void Salvar(List<FuncionarioModel> lista)
    {
        lock (_travaDeEscrita)
        {
            var json = JsonSerializer.Serialize(lista, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_caminhoArquivo, json);
        }
    }

    public FuncionarioModel Adicionar(FuncionarioModel novo)
    {
        var lista = Listar();
        novo.Id = lista.Any() ? lista.Max(x => x.Id) + 1 : 1;
        // mesmo "auto incremento na mão" do original -> maior id existente + 1, ou 1 se a lista estiver vazia
        lista.Add(novo);
        Salvar(lista);
        return novo;
    }

    // Retorna false se o id não foi encontrado -> quem chama decide o que fazer (mensagem pro front, etc),
    // em vez do método simplesmente quebrar (NullReferenceException) ou falhar silenciosamente.
    // Essa é a correção que a gente aplicou no projeto original, já nascendo certa aqui.
    public bool Editar(FuncionarioModel dados)
    {
        var lista = Listar();
        var existente = lista.FirstOrDefault(x => x.Id == dados.Id);
        // FirstOrDefault percorre a lista procurando o item com esse Id; devolve o objeto (por REFERÊNCIA,
        // já que FuncionarioModel é uma classe) se achar, ou null se não achar.

        if (existente is null)
            return false;

        // como "existente" é o mesmo objeto que já está dentro de "lista" (tipo referência),
        // alterar as propriedades aqui já reflete direto na lista -> não precisa de lista.Add(...) de novo
        existente.Nome = dados.Nome;
        existente.Matricula = dados.Matricula;
        existente.Departamento = dados.Departamento;
        existente.Sexo = dados.Sexo;
        existente.Cargo = dados.Cargo;
        existente.Salario = dados.Salario;
        existente.DataNascimento = dados.DataNascimento;

        Salvar(lista);
        return true;
    }

    public void Excluir(int id)
    {
        var lista = Listar();
        lista.RemoveAll(x => x.Id == id); // não dá erro se o id não existir, simplesmente não remove nada
        Salvar(lista);
    }
}
