using Articles.Domain.Entities;

namespace Articles.Services.Impl;

public class TagEqualityComparer : IEqualityComparer<Tag>
{
    public bool Equals(Tag? x, Tag? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;

        if (x.Id != 0 && y.Id != 0)
            return x.Id == y.Id;

        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Tag obj)
    {
        if (obj.Id != 0)
            return obj.Id.GetHashCode();

        return obj.Name?.ToLowerInvariant().GetHashCode() ?? 0;
    }
}