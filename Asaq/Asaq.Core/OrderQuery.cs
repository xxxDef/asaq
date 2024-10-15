namespace Asaq.Core;

public class OrderQuery<TFields> where TFields : class
{
    public TFields? SortAsc { get; set; }
    public TFields? SortDesc { get; set; }
}
