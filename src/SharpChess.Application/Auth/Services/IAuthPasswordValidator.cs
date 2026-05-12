using FluentResults;

namespace SharpChess.Application.Auth.Services;

public interface IAuthPasswordValidator
{
    Result Validate(string password);
}
