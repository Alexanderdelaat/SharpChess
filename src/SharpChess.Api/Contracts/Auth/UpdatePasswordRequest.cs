namespace SharpChess.Api.Contracts.Auth;

public record UpdatePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmNewPassword);
