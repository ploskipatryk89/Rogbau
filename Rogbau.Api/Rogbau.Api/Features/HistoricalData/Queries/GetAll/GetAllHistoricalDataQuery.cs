using MediatR;
using Rogbau.Api.Common.Pagination;
using Rogbau.Api.Features.HistoricalData.Dtos;

public record GetAllHistoricalDataQuery : IRequest<PaginatedList<HistoricalDataDto>>
{
    public Guid? SiteId { get; init; }
    public Guid? EquipmentId { get; init; }
    public DateOnly? FromDate { get; init; }
    public DateOnly? ToDate { get; init; }
    public PaginatedRequest Pagination { get; set; } = new();


}