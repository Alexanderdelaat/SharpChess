using FluentResults;
using SharpChess.Application.Auth.Errors;
using SharpChess.Application.Auth.Services;

namespace SharpChess.Api.Tests.Auth;

[TestClass]
public sealed class PasswordValidatorTests
{
    private readonly PasswordValidator _validator = new();

    [TestMethod]
    public void Validate_NullPassword_Fails()
    {
        string password = null!;

        Result result = _validator.Validate(password);

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordTooShort);
    }

    [TestMethod]
    public void Validate_EmptyPassword_Fails()
    {
        Result result = _validator.Validate(string.Empty);

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordTooShort);
    }

    [TestMethod]
    public void Validate_WhitespacePassword_Fails()
    {
        Result result = _validator.Validate("   ");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordTooShort);
    }

    [TestMethod]
    public void Validate_TooShortPassword_Fails()
    {
        Result result = _validator.Validate("Ab1!");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordTooShort);
    }

    [TestMethod]
    public void Validate_PasswordWithoutUppercase_Fails()
    {
        Result result = _validator.Validate("lowercasepass1!");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresUppercase);
    }

    [TestMethod]
    public void Validate_PasswordWithoutLowercase_Fails()
    {
        Result result = _validator.Validate("UPPERCASEPASS1!");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresLowercase);
    }

    [TestMethod]
    public void Validate_PasswordWithoutDigit_Fails()
    {
        Result result = _validator.Validate("PasswordWithoutDigit!");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresDigit);
    }

    [TestMethod]
    public void Validate_PasswordWithoutSpecialCharacter_Fails()
    {
        Result result = _validator.Validate("PasswordWithoutSymbol1");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresSpecialCharacter);
    }

    [TestMethod]
    public void Validate_PasswordMeetingAllRequirements_Succeeds()
    {
        Result result = _validator.Validate("StrongPassword1!");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void Validate_PasswordAtMinimumLength_Succeeds()
    {
        Result result = _validator.Validate("Abcdef1!Ghij");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void Validate_PasswordOneCharacterBelowMinimumLength_Fails()
    {
        Result result = _validator.Validate("Abcde1!Ghij");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordTooShort);
    }

    [TestMethod]
    public void Validate_PasswordWithMultipleSpecialCharacters_Succeeds()
    {
        Result result = _validator.Validate("ValidPass1!@#");

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void Validate_PasswordContainingOnlyLetters_Fails()
    {
        Result result = _validator.Validate("AbcdefGhijkl");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresDigit);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresSpecialCharacter);
    }

    [TestMethod]
    public void Validate_PasswordContainingOnlyNumbers_Fails()
    {
        Result result = _validator.Validate("123456789012");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresUppercase);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresLowercase);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresSpecialCharacter);
    }

    [TestMethod]
    public void Validate_PasswordContainingOnlySpecialCharacters_Fails()
    {
        Result result = _validator.Validate("!@#$%^&*()_+");

        Assert.IsTrue(result.IsFailed);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresUppercase);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresLowercase);
        CollectionAssert.Contains(GetErrorMessages(result), AuthErrorCodes.PasswordRequiresDigit);
    }

    private static List<string> GetErrorMessages(Result result)
    {
        return result.Errors.Select(error => error.Message).ToList();
    }
}
