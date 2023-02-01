namespace GrpcService.Server.Exceptions;

public class InvalidMessageIdException : Exception
{
    private string _id;

    public InvalidMessageIdException(string id)
    {
        _id = id;
    }
}