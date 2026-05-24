namespace SharpChess.Application.Auth;

public static class AuthErrorCodes
{
    public const string PasswordMismatch = "Wachtwoorden komen niet overeen.";

    public const string InvalidCredentials = "Ongeldige gebruikersnaam of wachtwoord.";
    public const string InvalidRefreshToken = "Ongeldige of verlopen sessie.";
    public const string PasswordTooShort = "Het wachtwoord moet minimaal 12 tekens bevatten.";
    public const string PasswordRequiresUppercase = "Het wachtwoord moet minimaal 1 hoofdletter bevatten.";
    public const string PasswordRequiresLowercase = "Het wachtwoord moet minimaal 1 kleine letter bevatten.";
    public const string PasswordRequiresDigit = "Het wachtwoord moet minimaal 1 cijfer bevatten.";
    public const string PasswordRequiresSpecialCharacter = "Het wachtwoord moet minimaal 1 speciaal teken bevatten.";
    public const string UsernameAlreadyExists = "Deze gebruikersnaam is al in gebruik.";
    public const string EmailAlreadyExists = "Dit e-mailadres is al in gebruik.";
    public const string InvalidEmail = "Voer een geldig e-mailadres in.";
    public const string UserNotFound = "Gebruiker niet gevonden.";
    public const string InvalidCurrentPassword = "Het huidige wachtwoord is onjuist.";
}
