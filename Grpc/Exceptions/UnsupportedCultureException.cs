using System.Globalization;

namespace GrpcService.Server.Exceptions;

public class UnsupportedCultureException : Exception
{
    private CultureInfo _cultureInfo;

    public UnsupportedCultureException(CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
    }
}