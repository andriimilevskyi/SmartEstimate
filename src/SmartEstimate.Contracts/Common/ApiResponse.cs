namespace SmartEstimate.Contracts.Common;

/// <summary>
/// A stable envelope for successful and expected API responses.
/// </summary>
/// <typeparam name="TData">The response payload type.</typeparam>
public sealed record ApiResponse<TData>(bool Success, TData? Data, ApiError? Error)
{
    public static ApiResponse<TData> FromData(TData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return new ApiResponse<TData>(true, data, null);
    }

    public static ApiResponse<TData> FromError(ApiError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new ApiResponse<TData>(false, default, error);
    }
}
