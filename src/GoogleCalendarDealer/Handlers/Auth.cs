using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GoogleCalendarDealer.Handlers;

/// <summary>
/// Class responsible for authenticating in the google calendar.
/// </summary>
public class Auth
{
    /// <summary>
    /// The user credential.
    /// </summary>
    private UserCredential _credential;
    
    /// <summary>
    /// The object to access the google calendar data.
    /// </summary>
    public CalendarService Service { get; private set; }

    public async Task Authenticate(string applicationName = "FPB Api", string credentialFileName = "credentials.json")
    {
        await using (var stream = new FileStream(credentialFileName, FileMode.Open, FileAccess.Read))
        {
            var credPath = "token.json";
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                [CalendarService.Scope.Calendar],
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true));
        }
        
        // Create Calendar API service
        Service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = _credential,
            ApplicationName = applicationName,
        });
    }
}