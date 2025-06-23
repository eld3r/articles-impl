using System.ComponentModel.DataAnnotations;

namespace Articles.Services.Attributes;

public class EachTagMaxLenght(int eachMaximum) : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is Array array)
        {
            return array.OfType<string>().All(tag => tag.Length <= eachMaximum);
        }

        return false;
    }

    public override string FormatErrorMessage(string name)
    {
        return $"All of {name} must be less then {eachMaximum} elements each.";
    }
}