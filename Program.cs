using Microsoft.EntityFrameworkCore;
using TodoApi;
// using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ToDoDbContext>(/*options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"))*/);
builder.Services.AddScoped<ToDoDbContext>();


var app = builder.Build();


app.UseCors(builder =>
{
    builder.WithOrigins("http://localhost:5082/")
           .AllowAnyHeader()
           .AllowAnyMethod();
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/", () => "malki goelman! 😉");

var tasks = new List<Items>();
var id = 1;

// Get all tasks
app.MapGet("/tasks", () => tasks);

app.MapGet("/todos", async (ToDoDbContext context) =>
{
    var todos = await context.Items.ToListAsync();
    return Results.Ok(todos);
});

// Add a new task
app.MapPost("/tasks", async (Items task, ToDoDbContext context) =>
{
    task.Id = id++;
    context.Items.Add(task);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

// Update a task
app.MapPut("/tasks/{taskId}", async (int taskId, Items task, ToDoDbContext context) =>
{
    var existingTask = await context.Items.FindAsync(taskId);
    if (existingTask != null)
    {
        existingTask.Name = task.Name;
        existingTask.IsComplete = task.IsComplete;
        await context.SaveChangesAsync();
        return Results.Ok(existingTask);
    }
    return Results.NotFound();
});

// Delete a task
app.MapDelete("/tasks/{taskId}", async (int taskId, ToDoDbContext context) =>
{
    var taskToRemove = await context.Items.FindAsync(taskId);
    if (taskToRemove != null)
    {
        context.Items.Remove(taskToRemove);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});
//
app.Run();
