using RazorSlices;
using RazorSlices.Samples.WebApp;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseStatusCodePages();

app.MapGet("/", () => RazorSlice.CreateHttpResult<Todo[]>("/Slices/Todos.cshtml", Todos.AllTodos));

app.Run();