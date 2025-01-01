namespace MoneyMateApi.Controllers.Tags.Dtos;

public record CreateTagDto
{
    public required string Name { get; init; }
}