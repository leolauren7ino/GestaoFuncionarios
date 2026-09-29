using System.Globalization;
using GestaoFuncionarios.Services;
using Microsoft.AspNetCore.Localization;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// MVC clássico (Controller + View), igual ao padrão do projeto original (TIController + .cshtml)
builder.Services.AddControllersWithViews();

// Registra o repositório como Singleton -> uma instância só, compartilhada entre todas as
// requisições (faz sentido aqui porque ele só guarda o caminho do arquivo, sem estado por usuário;
// o lock de escrita dentro dele já cuida de concorrência entre requisições simultâneas)
builder.Services.AddSingleton<FuncionarioJsonRepository>();

// QuestPDF exige declarar o tipo de licença -> Community é grátis para uso pessoal/portfólio
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // serve tudo que está em wwwroot/ (css, js, libs)

// Força a cultura pt-BR pra requisição inteira -> importante pra que o Model Binder do ASP.NET Core
// consiga converter a string "01/05/1999" (formato dd/mm/aaaa que o front manda) pra DateTime
// corretamente. Sem isso, dependendo do ambiente/SO onde o projeto rodar, o binder pode assumir
// cultura invariante (formato mm/dd/aaaa) e falhar/inverter dia e mês silenciosamente.
var culturaPadrao = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaPadrao),
    SupportedCultures = new[] { culturaPadrao },
    SupportedUICultures = new[] { culturaPadrao }
});

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Funcionarios}/{action=Index}/{id?}");
// rota padrão já cai direto na tela de Funcionários -> não precisamos de uma Home separada

app.Run();
