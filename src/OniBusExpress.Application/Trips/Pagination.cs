namespace OniBusExpress.Application.Trips;

public sealed record Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }

    private Pagination(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    public int Skip => (Page - 1) * PageSize;

    public static Pagination From(int? page, int? pageSize)
    {
        var normalizedPage = page is > 0 ? page.Value : 1;
        var normalizedSize = pageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize.Value
        };

        return new Pagination(normalizedPage, normalizedSize);
    }
}
