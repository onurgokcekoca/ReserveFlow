namespace ReserveFlow.Search.Application.Common.Models;

public sealed record PropertySearchCriteria(
    string? Query,
    string? Country,
    string? City,
    int? MinGuests,
    int Page = 1,
    int PageSize = 20);
