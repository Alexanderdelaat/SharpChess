namespace SharpChess.Domain.Entities;

public class User
{
    public Guid Id {get; private set;}
    public string Username {get; private set;}
    public string Email {get; private set;}

    public User(string Username, string email)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
    }



}