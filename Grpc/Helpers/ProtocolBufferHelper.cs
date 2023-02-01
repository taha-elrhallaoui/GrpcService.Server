using System.Globalization;
using Google.Protobuf.WellKnownTypes;

namespace GrpcService.Server.Helpers;

public static class ProtocolBufferHelper
{
    public static string? ExtractValue(Value value)
    {
        var result = string.Format($"{value}", CultureInfo.CurrentCulture);
        Console.WriteLine(result);
        
        return value.KindCase switch
        {
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.NumberValue => value.NumberValue.ToString(CultureInfo.InvariantCulture),
            Value.KindOneofCase.BoolValue => value.BoolValue.ToString(CultureInfo.InvariantCulture),
            Value.KindOneofCase.ListValue => string.Join(", ", value.ListValue.Values.Select(ExtractValue)),
            Value.KindOneofCase.NullValue => null,
            Value.KindOneofCase.None => null,
            Value.KindOneofCase.StructValue => throw new ArgumentOutOfRangeException(value.KindCase.ToString()),
            _ => throw new ArgumentOutOfRangeException(value.KindCase.ToString())
        };
    }
}