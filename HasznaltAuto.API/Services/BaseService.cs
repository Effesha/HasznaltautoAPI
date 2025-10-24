namespace HasznaltAuto.API.Services;

public class BaseService(ILogger<BaseService> logger)
{
    public readonly List<string> _sessionList = [];

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
