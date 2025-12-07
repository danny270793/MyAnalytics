namespace Backend.Responses;

public class PagedResponse<T>
{
    public required IEnumerable<T> Items { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required int TotalItems { get; set; }
    public int TotalPages => (int) Math.Ceiling((double) TotalItems / PageSize);
}
