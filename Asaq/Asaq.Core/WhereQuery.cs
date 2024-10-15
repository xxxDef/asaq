namespace Asaq.Core;

public class WhereQuery<TFields> where TFields : class
{
    public TFields? Match { get; set; }
    public TFields? Contains { get; set; }
    public TFields? From { get; set; }
    public TFields? To { get; set; }
}
