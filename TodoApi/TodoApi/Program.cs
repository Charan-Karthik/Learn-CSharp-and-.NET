// Built this following a guide:
// https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0&tabs=visual-studio-mac

using Microsoft.EntityFrameworkCore;

// The two lines below that start with 'var'
// Create a WebApplicationBuilder and WebApplication w/ preconfigured defaults
var builder = WebApplication.CreateBuilder(args);
// The two lines below that start with 'builder.'
// add database context to dependency injection container
// enables displaying database-related exceptions
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

// MapGroup method (from MapGroup API) allows organization of URL prefixes (promotes 'DRY' code)
RouteGroupBuilder todoItems = app.MapGroup("/todoitems");
//var todoItems = app.MapGroup("/todoitems");

// Using Route Handler Method, which uses the TypedResultsAPI (see alternatives at end of program)
todoItems.MapGet("/", GetAllTodos);
// Un-updated route (no DTO)
//todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
}

// Above method without DTO:
//static async Task<IResult> GetAllTodos(TodoDb db)
//{
//    return TypedResults.Ok(await db.Todos.ToArrayAsync());
//}

// One possible unit test for the above method
//public async Task GetAllTodos_ReturnsOkOfTodosResult()
//{
//    // Arrange
//    var db = CreateDbContext();

//    // Act
//    var result = await TodosApi.GetAllTodos(db);

//    // Assert: Check for the correct returned type
//    Assert.IsType<Ok<Todo[]>>(result);
//}



// Un-updated method (no DTO)
//static async Task<IResult> GetCompleteTodos(TodoDb db)
//{
//    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
//}



static async Task<IResult> GetTodo(int id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            //? TypedResults.Ok(todo) // without DTO update
            : TypedResults.NotFound();
}



static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

// Without DTO
//static async Task<IResult> CreateTodo(Todo todo, TodoDb db)
//{
//    db.Todos.Add(todo);
//    await db.SaveChangesAsync();

//    return TypedResults.Created($"/todoitems/{todo.Id}", todo);
//}



static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return TypedResults.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

// Without DTO
//static async Task<IResult> UpdateTodo(int id, Todo inputTodo, TodoDb db)
//{
//    var todo = await db.Todos.FindAsync(id);

//    if (todo is null) return TypedResults.NotFound();

//    todo.Name = inputTodo.Name;
//    todo.IsComplete = inputTodo.IsComplete;

//    await db.SaveChangesAsync();

//    return TypedResults.NoContent();
//}


// no change w/ or w/o DTO
static async Task<IResult> DeleteTodo(int id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.Ok(todo);
    }

    return TypedResults.NotFound();
}





//
// Using lambdas (similar to callbacks in JS) in combination with MapGroup
//
//todoItems.MapGet("/", async (TodoDb db) =>
//    await db.Todos.ToListAsync());

//todoItems.MapGet("/complete", async (TodoDb db) =>
//    await db.Todos.Where(t => t.IsComplete).ToListAsync());

//todoItems.MapGet("/{id}", async (int id, TodoDb db) =>
//    await db.Todos.FindAsync(id)
//        is Todo todo
//            ? Results.Ok(todo)
//            : Results.NotFound());

//todoItems.MapPost("/", async (Todo todo, TodoDb db) =>
//{
//    db.Todos.Add(todo);
//    await db.SaveChangesAsync();

//    return Results.Created($"/todoitems/{todo.Id}", todo);
//});

//todoItems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) =>
//{
//    var todo = await db.Todos.FindAsync(id);

//    if (todo is null) return Results.NotFound();

//    todo.Name = inputTodo.Name;
//    todo.IsComplete = inputTodo.IsComplete;

//    await db.SaveChangesAsync();

//    return Results.NoContent();
//});

//todoItems.MapDelete("/{id}", async (int id, TodoDb db) =>
//{
//    if (await db.Todos.FindAsync(id) is Todo todo)
//    {
//        db.Todos.Remove(todo);
//        await db.SaveChangesAsync();
//        return Results.Ok(todo);
//    }

//    return Results.NotFound();
//});





//
// Without MapGroup
//
//app.MapGet("/todoitems", async (TodoDb db) =>
//    await db.Todos.ToListAsync());

//app.MapGet("/todoitems/complete", async (TodoDb db) =>
//    await db.Todos.Where(t => t.IsComplete).ToListAsync());

//app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
//    await db.Todos.FindAsync(id)
//        is Todo todo
//            ? Results.Ok(todo)
//            : Results.NotFound());

//app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
//{
//    db.Todos.Add(todo);
//    await db.SaveChangesAsync();

//    return Results.Created($"/todoitems/{todo.Id}", todo);
//});

//app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
//{
//    var todo = await db.Todos.FindAsync(id);

//    if (todo is null) return Results.NotFound();

//    todo.Name = inputTodo.Name;
//    todo.IsComplete = inputTodo.IsComplete;

//    await db.SaveChangesAsync();

//    return Results.NoContent();
//});

//app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
//{
//    if (await db.Todos.FindAsync(id) is Todo todo)
//    {
//        db.Todos.Remove(todo);
//        await db.SaveChangesAsync();
//        return Results.Ok(todo);
//    }

//    return Results.NotFound();
//});