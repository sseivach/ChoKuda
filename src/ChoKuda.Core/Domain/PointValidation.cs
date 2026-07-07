namespace ChoKuda.Core.Domain;

public static class PointValidation
{
    public static IReadOnlyList<string> ValidateRequiredFields(
        Guid id,
        string title,
        double latitude,
        double longitude)
    {
        var errors = new List<string>();

        if (id == Guid.Empty)
        {
            errors.Add("Point id is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add("Point title is required.");
        }

        if (double.IsNaN(latitude) || double.IsInfinity(latitude))
        {
            errors.Add("Point latitude must be a finite number.");
        }
        else if (latitude is < -90 or > 90)
        {
            errors.Add("Point latitude must be between -90 and 90.");
        }

        if (double.IsNaN(longitude) || double.IsInfinity(longitude))
        {
            errors.Add("Point longitude must be a finite number.");
        }
        else if (longitude is < -180 or > 180)
        {
            errors.Add("Point longitude must be between -180 and 180.");
        }

        return errors;
    }
}

