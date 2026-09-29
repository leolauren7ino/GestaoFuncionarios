namespace GestaoFuncionarios.Models;

// Corpo que o front manda no POST de exportação (equivalente ao FiltroCadastroUsuario do original)
public class FiltroExportacaoModel
{
    public string Tipo { get; set; } = string.Empty; // "EXCEL" | "PDF" | "JSON"
    public string GeradoPor { get; set; } = string.Empty; // nome de quem gerou o relatório (mostrado no rodapé do PDF)
    public List<FuncionarioModel> Linhas { get; set; } = new();
}
