// The model for a to-do task item

public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    // Data Transfer Object(DTO), input model, or view model
    // used for security reasons (i.e. prevent over-posting, hiding/omiting properties, flattening object graphs, etc.)
    public string? Secret { get; set; }
}