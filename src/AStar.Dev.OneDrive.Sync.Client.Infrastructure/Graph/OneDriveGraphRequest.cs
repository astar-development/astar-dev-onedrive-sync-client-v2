using System.Net.Http.Headers;

namespace AStar.Dev.OneDrive.Sync.Client.Infrastructure.Graph;

/// <summary>
/// Represents a OneDrive Graph HTTP request.
/// </summary>
/// <param name="Method">The HTTP method.</param>
/// <param name="Path">The relative Graph API path.</param>
/// <param name="AccessToken">The bearer token used for the request.</param>
/// <param name="Content">Optional request content.</param>
public sealed record OneDriveGraphRequest(HttpMethod Method, string Path, string AccessToken, HttpContent? Content = null)
{
    /// <summary>
    /// Creates a GET request descriptor.
    /// </summary>
    public static OneDriveGraphRequest Get(string path, string accessToken) => new(HttpMethod.Get, path, accessToken);

    internal HttpRequestMessage BuildHttpRequest()
    {
        var request = new HttpRequestMessage(Method, Path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
        request.Content = Content;
        return request;
    }
}
