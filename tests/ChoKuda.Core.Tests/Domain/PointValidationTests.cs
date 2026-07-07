using ChoKuda.Core.Domain;

namespace ChoKuda.Core.Tests.Domain;

public sealed class PointValidationTests
{
    [Fact]
    public void ValidateRequiredFieldsAcceptsValidPoint()
    {
        var errors = PointValidation.ValidateRequiredFields(
            Guid.NewGuid(),
            "Havasu Falls",
            36.255,
            -112.697);

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateRequiredFieldsRejectsMissingIdAndTitle()
    {
        var errors = PointValidation.ValidateRequiredFields(
            Guid.Empty,
            " ",
            0,
            0);

        Assert.Contains("Point id is required.", errors);
        Assert.Contains("Point title is required.", errors);
    }

    [Theory]
    [InlineData(double.NaN, "Point latitude must be a finite number.")]
    [InlineData(double.PositiveInfinity, "Point latitude must be a finite number.")]
    [InlineData(-90.1, "Point latitude must be between -90 and 90.")]
    [InlineData(90.1, "Point latitude must be between -90 and 90.")]
    public void ValidateRequiredFieldsRejectsInvalidLatitude(
        double latitude,
        string expectedError)
    {
        var errors = PointValidation.ValidateRequiredFields(
            Guid.NewGuid(),
            "Point",
            latitude,
            0);

        Assert.Contains(expectedError, errors);
    }

    [Theory]
    [InlineData(double.NaN, "Point longitude must be a finite number.")]
    [InlineData(double.NegativeInfinity, "Point longitude must be a finite number.")]
    [InlineData(-180.1, "Point longitude must be between -180 and 180.")]
    [InlineData(180.1, "Point longitude must be between -180 and 180.")]
    public void ValidateRequiredFieldsRejectsInvalidLongitude(
        double longitude,
        string expectedError)
    {
        var errors = PointValidation.ValidateRequiredFields(
            Guid.NewGuid(),
            "Point",
            0,
            longitude);

        Assert.Contains(expectedError, errors);
    }
}

