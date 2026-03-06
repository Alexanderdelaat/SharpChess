using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace SharpChess.Application.Auth.Command.Register;

public record RegisterCommand(
    string Username,
    string Email, 
    string Password,
    String ConfirmPassword);

public record RegisterResult(
    string Id,
    string Username,
    String Email
);