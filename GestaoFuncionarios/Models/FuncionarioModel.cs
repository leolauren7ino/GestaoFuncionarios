namespace GestaoFuncionarios.Models;

// Molde dos dados -> equivalente ao CadastroUsuarioModel do projeto original.
// Toda propriedade pode ser "get; set;" normal (não precisa de [JsonProperty] ou nada disso,
// o System.Text.Json do ASP.NET Core já serializa/desserializa automaticamente por reflexão,
// desde que as propriedades sejam públicas -- mesma regra do JavaScriptSerializer original).
public class FuncionarioModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty; // equivalente ao "RE" do original -> string, não int, pra não perder zero à esquerda
    public string Departamento { get; set; } = string.Empty; // equivalente à "Planta"
    public string Sexo { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public decimal Salario { get; set; } // decimal, não double -> precisão exata pra dinheiro (mesma razão do original)
    public DateTime DataNascimento { get; set; }
}
