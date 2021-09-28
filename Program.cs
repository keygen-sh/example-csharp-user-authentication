using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

class Keygen
{
  public RestClient Client = null;

  public Keygen(string accountId)
  {
    Client = new RestClient($"https://api.keygen.sh/v1/accounts/{accountId}");
  }

  public Dictionary<string, object> AuthenticateAsUser(string email, string password)
  {
    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{password}"));
    var request = new RestRequest("tokens", Method.POST);

    request.AddHeader("Content-Type", "application/vnd.api+json");
    request.AddHeader("Accept", "application/vnd.api+json");
    request.AddHeader("Authorization", $"Basic {credentials}");

    var response = Client.Execute<Dictionary<string, object>>(request);
    if (response.Data.ContainsKey("errors"))
    {
      var errors = (List<Dictionary<string, object>>) response.Data["errors"];
      if (errors != null)
      {
        Console.WriteLine("[ERROR] [AuthenticateAsUser] Status={0} Errors={1}", response.StatusCode, errors);

        Environment.Exit(1);
      }
    }

    return (Dictionary<string, object>) response.Data["data"];
  }

  public RestSharp.JsonArray ListLicensesForToken(string token)
  {
    var request = new RestRequest("licenses", Method.GET);

    request.AddHeader("Authorization", $"Bearer {token}");
    request.AddHeader("Content-Type", "application/vnd.api+json");
    request.AddHeader("Accept", "application/vnd.api+json");

    var response = Client.Execute<Dictionary<string, object>>(request);
    if (response.Data.ContainsKey("errors"))
    {
      var errors = (RestSharp.JsonArray) response.Data["errors"];
      if (errors != null)
      {
        Console.WriteLine("[ERROR] [ListLicensesForToken] Status={0} Errors={1}", response.StatusCode, errors);

        Environment.Exit(1);
      }
    }

    return (RestSharp.JsonArray) response.Data["data"];
  }
}

class Program
{
  public static void Main (string[] args)
  {
    var keygen = new Keygen("demo");

    // Authenticate and list the user's licenses
    var userToken = keygen.AuthenticateAsUser("demo@example.com", "demo");
    var attrs = (Dictionary<string, object>) userToken["attributes"];
    var token = (string) attrs["token"];
    var licenses = keygen.ListLicensesForToken(token);

    // Print the overall results
    Console.WriteLine("[INFO] [Main] Token={0} LicenseCount={1}", token, licenses.Count);
  }
}
