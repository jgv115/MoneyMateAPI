using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TransactionService.Services.Initialisation.CategoryInitialisation.Exceptions;

namespace TransactionService.Services.Initialisation.CategoryInitialisation;

public class LambdaCategoryInitialiser : ICategoryInitialiser
{
    private readonly ILogger<LambdaCategoryInitialiser> _logger;
    private readonly IAmazonLambda _lamdaClient;
    private readonly LambdaCategoryInitialiserSettings _settings;

    public LambdaCategoryInitialiser(IAmazonLambda lamdaClient,
        IOptions<LambdaCategoryInitialiserSettings> settings, ILogger<LambdaCategoryInitialiser> logger)
    {
        _lamdaClient = lamdaClient;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task InitialiseCategories(Guid userId, Guid profileId)
    {
        try
        {
            var response = await _lamdaClient.InvokeAsync(new InvokeRequest
            {
                FunctionName = _settings.LambdaName,
                Payload = JsonSerializer.Serialize(
                    new CategoryInitialiserLambdaInvokeRequest(userId.ToString(), profileId.ToString()))
            });

            if (response.FunctionError != null)
            {
                _logger.LogError(
                    "Received an error when invoking lambda to initialise categories for {UserId} and {ProfileId} with error payload: {ErrorPayload}",
                    userId, profileId, response.FunctionError);
                throw new CategoryInitialisationException("CategoryInitialiser Lambda invocation failed");
            }
        }
        catch (Exception ex)
        {
            throw new CategoryInitialisationException("CategoryInitialiser Lambda invocation failed", ex);
        }
    }
}