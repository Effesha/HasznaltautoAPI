namespace HasznaltAuto.API.GrpcServices;

public class BaseService(ILogger<BaseService> logger)
{
    internal readonly List<string> sessionList = [];

    public async Task<ResultResponse> RequestFailed(string message)
    {
        return await Task.FromResult(new ResultResponse
        {
            Success = false,
            Message = message
        });
    }

    public async Task<ResultResponse> RequestSuccessful(string message)
    {
        return await Task.FromResult(new ResultResponse
        {
            Success = true,
            Message = message
        });
    }
}
