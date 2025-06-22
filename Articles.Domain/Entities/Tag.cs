namespace Articles.Domain.Entities;

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } =  null!;

#if DEBUG
    public override string ToString() => $"Tag: Id:{Id} Name:{Name}";
#endif
}