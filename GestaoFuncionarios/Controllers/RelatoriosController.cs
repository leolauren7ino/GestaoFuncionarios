using ClosedXML.Excel;
using GestaoFuncionarios.Models;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Globalization;
using System.Text.Json;

namespace GestaoFuncionarios.Controllers;

// Equivalente ao CadastroUsuarioRelModel (ApiReports/LEO) do projeto original -> só que aqui,
// como é tudo um projeto ASP.NET Core só (sem a separação em dois projetos/duas portas que o
// original tinha por ser um sistema maior com várias APIs internas), isso vira só mais um
// Controller do mesmo projeto, em vez de uma API separada.
[ApiController]
[Route("api/relatorios")]
public class RelatoriosController : ControllerBase
{
    // POST /api/relatorios/funcionarios
    [HttpPost("funcionarios")]
    public IActionResult GerarRelatorio([FromBody] FiltroExportacaoModel filtro)
    {
        if (filtro.Linhas is null || filtro.Linhas.Count == 0)
        {
            // Melhoria em relação ao original: lá, "sem dados" caía numa página HTML disfarçada
            // de sucesso (status 200), e o front tinha que detectar isso lendo o Content-Type da
            // resposta. Aqui devolvemos um status HTTP de erro de verdade (400) com um JSON simples
            // -> o front só precisa checar "response.ok", sem aquele sniffing manual de content-type.
            return BadRequest(new { mensagem = "Não há registros para exportar." });
        }

        return filtro.Tipo?.ToUpperInvariant() switch
        {
            "EXCEL" => GerarExcel(filtro),
            "PDF" => GerarPdf(filtro),
            "JSON" => GerarJson(filtro),
            _ => BadRequest(new { mensagem = $"Tipo de arquivo inválido: '{filtro.Tipo}'." })
        };
    }

    private IActionResult GerarExcel(FiltroExportacaoModel filtro)
    {
        using var workbook = new XLWorkbook(); // o "arquivo" Excel em memória
        var planilha = workbook.Worksheets.Add("Funcionarios"); // cria a aba

        // cabeçalho -> linha 1. Indexação do ClosedXML começa em 1, não em 0.
        string[] cabecalhos = { "Id", "Nome", "Matrícula", "Departamento", "Sexo", "Cargo", "Salário", "Dt. Nascimento" };
        for (int coluna = 1; coluna <= cabecalhos.Length; coluna++)
            planilha.Cell(1, coluna).Value = cabecalhos[coluna - 1];

        planilha.Range(1, 1, 1, cabecalhos.Length).Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightGray);
        // Range aplica o estilo num bloco de células de uma vez, sem precisar percorrer célula por célula

        int linha = 2; // linha 1 é o cabeçalho, os dados começam na 2
        foreach (var f in filtro.Linhas)
        {
            planilha.Cell(linha, 1).SetValue(f.Id);
            planilha.Cell(linha, 2).SetValue(f.Nome);
            planilha.Cell(linha, 3).SetValue(f.Matricula); // string -> preserva zero à esquerda, se tiver
            planilha.Cell(linha, 4).SetValue(f.Departamento);
            planilha.Cell(linha, 5).SetValue(f.Sexo);
            planilha.Cell(linha, 6).SetValue(f.Cargo);

            planilha.Cell(linha, 7).SetValue(f.Salario);
            planilha.Cell(linha, 7).Style.NumberFormat.Format = "\"R$\" #,##0.00";
            // NumberFormat é só formatação VISUAL -> o valor guardado na célula continua sendo o número puro

            planilha.Cell(linha, 8).SetValue(f.DataNascimento);
            planilha.Cell(linha, 8).Style.NumberFormat.Format = "dd/mm/yyyy";

            linha++;
        }

        planilha.Columns().AdjustToContents(); // ajusta a largura das colunas ao conteúdo

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        // depois de escrever, o ponteiro do stream fica no final -> sem resetar pra 0,
        // quem ler esse stream em seguida (o File(...) abaixo) vai ler "nada" (arquivo vazio)

        var nomeArquivo = $"RelatorioFuncionarios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nomeArquivo);
        // File(bytes, contentType, nomeArquivo) já monta o header Content-Disposition sozinho ->
        // no projeto original isso era feito manualmente (RespostaArquivo), aqui o framework resolve
    }

    private IActionResult GerarPdf(FiltroExportacaoModel filtro)
    {
        var cultura = new CultureInfo("pt-BR");

        var bytesPdf = Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(30);

                pagina.Header()
                    .Text("Relatório de Funcionários")
                    .FontSize(18).Bold();

                pagina.Content().Table(tabela =>
                {
                    tabela.ColumnsDefinition(colunas =>
                    {
                        colunas.RelativeColumn(2); // Nome
                        colunas.RelativeColumn(1); // Matrícula
                        colunas.RelativeColumn(1); // Departamento
                        colunas.RelativeColumn(2); // Cargo
                        colunas.RelativeColumn(1); // Salário
                        colunas.RelativeColumn(1); // Dt. Nascimento
                    });

                    tabela.Header(cabecalho =>
                    {
                        foreach (var titulo in new[] { "Nome", "Matrícula", "Depto.", "Cargo", "Salário", "Nascimento" })
                            cabecalho.Cell().Text(titulo).Bold();
                    });

                    foreach (var f in filtro.Linhas)
                    {
                        tabela.Cell().Text(f.Nome);
                        tabela.Cell().Text(f.Matricula);
                        tabela.Cell().Text(f.Departamento);
                        tabela.Cell().Text(f.Cargo);
                        tabela.Cell().Text(f.Salario.ToString("C", cultura));
                        // CultureInfo("pt-BR") explícito -> garante "R$ 8.000,00" independente
                        // da configuração regional do servidor onde isso rodar
                        tabela.Cell().Text(f.DataNascimento.ToString("dd/MM/yyyy", cultura));
                    }
                });

                pagina.Footer().AlignRight().Text(texto =>
                {
                    texto.Span("Gerado por: ").SemiBold();
                    texto.Span(string.IsNullOrWhiteSpace(filtro.GeradoPor) ? "-" : filtro.GeradoPor);
                    texto.Span($"  |  {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });
        }).GeneratePdf();
        // QuestPDF monta o PDF inteiro com essa API fluente (Fluent = métodos encadeados,
        // cada um configurando uma parte do layout) e devolve os bytes prontos de uma vez só
        // -> diferente do iTextSharp original, não precisa lidar com MemoryStream/pdf.Close() na mão

        var nomeArquivo = $"RelatorioFuncionarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(bytesPdf, "application/pdf", nomeArquivo);
    }

    private IActionResult GerarJson(FiltroExportacaoModel filtro)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(filtro.Linhas, new JsonSerializerOptions { WriteIndented = true });
        var nomeArquivo = $"RelatorioFuncionarios_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        return File(json, "application/json", nomeArquivo);
    }
}
