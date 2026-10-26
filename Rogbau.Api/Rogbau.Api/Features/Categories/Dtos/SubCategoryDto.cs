namespace Rogbau.Api.Features.Categories.Dtos;

public record SubCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
}