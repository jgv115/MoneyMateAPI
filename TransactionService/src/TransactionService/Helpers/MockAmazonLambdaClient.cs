using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.Runtime.Endpoints;

namespace TransactionService.Helpers;

public class MockAmazonLambdaClient : IAmazonLambda
{
    private string LambdaName { get; init; }

    public delegate Task LambdaImplementation();

    private LambdaImplementation _lambdaImplementation;

    public MockAmazonLambdaClient()
    {
        LambdaName = "TestLambda";
        _lambdaImplementation = () => { return Task.CompletedTask; };
    }

    public MockAmazonLambdaClient(string lambdaName, LambdaImplementation lambdaImplementation)
    {
        LambdaName = lambdaName;
        _lambdaImplementation = lambdaImplementation;
    }

    public IClientConfig Config { get; }

    public void Dispose()
    {
    }

    public async Task<InvokeResponse> InvokeAsync(InvokeRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        await _lambdaImplementation.Invoke();
        return new InvokeResponse();
    }

    public Task<AddLayerVersionPermissionResponse> AddLayerVersionPermissionAsync(
        AddLayerVersionPermissionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<AddPermissionResponse> AddPermissionAsync(AddPermissionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<CreateAliasResponse> CreateAliasAsync(CreateAliasRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<CreateCodeSigningConfigResponse> CreateCodeSigningConfigAsync(CreateCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<CreateEventSourceMappingResponse> CreateEventSourceMappingAsync(CreateEventSourceMappingRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunctionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<CreateFunctionUrlConfigResponse> CreateFunctionUrlConfigAsync(CreateFunctionUrlConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteAliasResponse> DeleteAliasAsync(DeleteAliasRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteCodeSigningConfigResponse> DeleteCodeSigningConfigAsync(DeleteCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteEventSourceMappingResponse> DeleteEventSourceMappingAsync(DeleteEventSourceMappingRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteFunctionResponse> DeleteFunctionAsync(string functionName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteFunctionResponse> DeleteFunctionAsync(DeleteFunctionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteFunctionCodeSigningConfigResponse> DeleteFunctionCodeSigningConfigAsync(
        DeleteFunctionCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteFunctionConcurrencyResponse> DeleteFunctionConcurrencyAsync(
        DeleteFunctionConcurrencyRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteFunctionEventInvokeConfigResponse> DeleteFunctionEventInvokeConfigAsync(
        DeleteFunctionEventInvokeConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteFunctionUrlConfigResponse> DeleteFunctionUrlConfigAsync(DeleteFunctionUrlConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteLayerVersionResponse> DeleteLayerVersionAsync(DeleteLayerVersionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<DeleteProvisionedConcurrencyConfigResponse> DeleteProvisionedConcurrencyConfigAsync(
        DeleteProvisionedConcurrencyConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetAccountSettingsResponse> GetAccountSettingsAsync(GetAccountSettingsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetAliasResponse> GetAliasAsync(GetAliasRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetCodeSigningConfigResponse> GetCodeSigningConfigAsync(GetCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetEventSourceMappingResponse> GetEventSourceMappingAsync(GetEventSourceMappingRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionResponse> GetFunctionAsync(string functionName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionResponse> GetFunctionAsync(GetFunctionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionCodeSigningConfigResponse> GetFunctionCodeSigningConfigAsync(
        GetFunctionCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionConcurrencyResponse> GetFunctionConcurrencyAsync(GetFunctionConcurrencyRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionConfigurationResponse> GetFunctionConfigurationAsync(string functionName,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionConfigurationResponse> GetFunctionConfigurationAsync(GetFunctionConfigurationRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionEventInvokeConfigResponse> GetFunctionEventInvokeConfigAsync(
        GetFunctionEventInvokeConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionRecursionConfigResponse> GetFunctionRecursionConfigAsync(GetFunctionRecursionConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetFunctionUrlConfigResponse> GetFunctionUrlConfigAsync(GetFunctionUrlConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetLayerVersionResponse> GetLayerVersionAsync(GetLayerVersionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetLayerVersionByArnResponse> GetLayerVersionByArnAsync(GetLayerVersionByArnRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetLayerVersionPolicyResponse> GetLayerVersionPolicyAsync(GetLayerVersionPolicyRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetPolicyResponse> GetPolicyAsync(GetPolicyRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetProvisionedConcurrencyConfigResponse> GetProvisionedConcurrencyConfigAsync(
        GetProvisionedConcurrencyConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<GetRuntimeManagementConfigResponse> GetRuntimeManagementConfigAsync(
        GetRuntimeManagementConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<InvokeAsyncResponse> InvokeAsyncAsync(InvokeAsyncRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<InvokeWithResponseStreamResponse> InvokeWithResponseStreamAsync(InvokeWithResponseStreamRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListAliasesResponse> ListAliasesAsync(ListAliasesRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListCodeSigningConfigsResponse> ListCodeSigningConfigsAsync(ListCodeSigningConfigsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListEventSourceMappingsResponse> ListEventSourceMappingsAsync(ListEventSourceMappingsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListFunctionEventInvokeConfigsResponse> ListFunctionEventInvokeConfigsAsync(
        ListFunctionEventInvokeConfigsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListFunctionsResponse> ListFunctionsAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListFunctionsResponse> ListFunctionsAsync(ListFunctionsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListFunctionsByCodeSigningConfigResponse> ListFunctionsByCodeSigningConfigAsync(
        ListFunctionsByCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListFunctionUrlConfigsResponse> ListFunctionUrlConfigsAsync(ListFunctionUrlConfigsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListLayersResponse> ListLayersAsync(ListLayersRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListLayerVersionsResponse> ListLayerVersionsAsync(ListLayerVersionsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListProvisionedConcurrencyConfigsResponse> ListProvisionedConcurrencyConfigsAsync(
        ListProvisionedConcurrencyConfigsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListTagsResponse> ListTagsAsync(ListTagsRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<ListVersionsByFunctionResponse> ListVersionsByFunctionAsync(ListVersionsByFunctionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PublishLayerVersionResponse> PublishLayerVersionAsync(PublishLayerVersionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PublishVersionResponse> PublishVersionAsync(PublishVersionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PutFunctionCodeSigningConfigResponse> PutFunctionCodeSigningConfigAsync(
        PutFunctionCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PutFunctionConcurrencyResponse> PutFunctionConcurrencyAsync(PutFunctionConcurrencyRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PutFunctionEventInvokeConfigResponse> PutFunctionEventInvokeConfigAsync(
        PutFunctionEventInvokeConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PutFunctionRecursionConfigResponse> PutFunctionRecursionConfigAsync(PutFunctionRecursionConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PutProvisionedConcurrencyConfigResponse> PutProvisionedConcurrencyConfigAsync(
        PutProvisionedConcurrencyConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<PutRuntimeManagementConfigResponse> PutRuntimeManagementConfigAsync(
        PutRuntimeManagementConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<RemoveLayerVersionPermissionResponse> RemoveLayerVersionPermissionAsync(
        RemoveLayerVersionPermissionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<RemovePermissionResponse> RemovePermissionAsync(RemovePermissionRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<TagResourceResponse> TagResourceAsync(TagResourceRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UntagResourceResponse> UntagResourceAsync(UntagResourceRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateAliasResponse> UpdateAliasAsync(UpdateAliasRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateCodeSigningConfigResponse> UpdateCodeSigningConfigAsync(UpdateCodeSigningConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateEventSourceMappingResponse> UpdateEventSourceMappingAsync(UpdateEventSourceMappingRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateFunctionCodeResponse> UpdateFunctionCodeAsync(UpdateFunctionCodeRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateFunctionConfigurationResponse> UpdateFunctionConfigurationAsync(
        UpdateFunctionConfigurationRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateFunctionEventInvokeConfigResponse> UpdateFunctionEventInvokeConfigAsync(
        UpdateFunctionEventInvokeConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Task<UpdateFunctionUrlConfigResponse> UpdateFunctionUrlConfigAsync(UpdateFunctionUrlConfigRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new System.NotImplementedException();
    }

    public Endpoint DetermineServiceOperationEndpoint(AmazonWebServiceRequest request)
    {
        throw new System.NotImplementedException();
    }

    public ILambdaPaginatorFactory Paginators { get; }
}