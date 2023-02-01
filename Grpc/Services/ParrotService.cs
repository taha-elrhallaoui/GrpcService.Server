using System.Globalization;
using Google.Type;
using Grpc.Core;
using GrpcService.Server.Exceptions;
using GrpcService.Server.Helpers;
using Parrot;

namespace GrpcService.Server.Services;

public class ParrotService : Parrot.Parrot.ParrotBase
{
    private readonly HashSet<CultureInfo> _supportedCultures;
    private readonly CultureInfo _defaultCulture;
    private readonly ILogger<ParrotService> _logger;

    public ParrotService(IConfiguration configuration, ILogger<ParrotService> logger)
    {
        _logger = logger;

        var supportedCultures = configuration.GetSection("Parrot:SupportedCultures").Get<string[]>();
        if (supportedCultures == null)
        {
            throw new Exception();
        }

        _supportedCultures = supportedCultures.Distinct().Select(c => new CultureInfo(c)).ToHashSet();

        var defaultCulture = configuration["Parrot:DefaultCulture"];
        _defaultCulture = defaultCulture == null
            ? CultureInfo.CurrentCulture
            : new CultureInfo(defaultCulture);
    }

    private bool IsLocaleSupported(string locale) =>
        _supportedCultures.Any(c => c.Name.Equals(locale, StringComparison.InvariantCultureIgnoreCase));

    public override Task<FormatReply> Format(FormatRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("received a request: {}", request);

        var culture = GetCultureInfo(request.Locale);

        var parameters = request.Items.Values.Select(ProtocolBufferHelper.ExtractValue).ToList();
        if (parameters.Any(p => p == null))
        {
            return Task.FromResult(new FormatReply
            {
                Error = "NullParameterNotAllowed"
            });
        }

        if (!TryFormat(request.Id, parameters!, culture, out var message, out var usedCulture))
        {
            // TODO better error handling
            return Task.FromResult(new FormatReply
            {
                Error = "InvalidMessageId"
            });
        }

        return Task.FromResult(new FormatReply
        {
            Message = new LocalizedText
            {
                LanguageCode = usedCulture!.Name,
                Text = message
            }
        });
    }

    /**
     * TODO improve fallback logic
     */
    private CultureInfo GetCultureInfo(string locale) =>
        _supportedCultures.FirstOrDefault(c => c.Name.Equals(locale, StringComparison.InvariantCulture)) ??
        _defaultCulture;

    private static string Format(string id, IEnumerable<string>? parameters, CultureInfo cultureInfo)
    {
        // TODO add fallback to default culture
        var message = Messages.ResourceManager.GetString(id, cultureInfo);
        if (message == null)
        {
            throw new InvalidMessageIdException(id);
        }

        return parameters == null ? message : string.Format(cultureInfo, message, parameters.ToArray<object>());
    }

    private bool TryFormat(string id, IEnumerable<string> parameters, CultureInfo cultureInfo, out string? result,
        out CultureInfo? usedCultureInfo)
    {
        try
        {
            result = Format(id, parameters, cultureInfo);
            usedCultureInfo = cultureInfo;
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning("failed to format message with ID `{id}`", exception);
            result = null;
            usedCultureInfo = null;
            return false;
        }
    }
}