using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for LinkedInConfig
/// </summary>
public class LinkedInConfig : IProviderConfig
{
    private string baseUrl = "https://api.linkedin.com";

    public string RequestTokenUrl
    {
        get { return baseUrl + "/uas/oauth/requestToken"; }
    }

    public string UserAuthUrl
    {
        get { return baseUrl + "/uas/oauth/authenticate"; }
    }

    public string AccessTokenUrl
    {
        get { return baseUrl + "/uas/oauth/accessToken"; }
    }

    public string MethodUrl
    {
        get { return baseUrl + "/v1"; }
    }

    public string DemoMethodUrl
    {
        get { return "http://api.linkedin.com/v1/people/~/connections"; }
    }

    public string RequestLevel
    {
        get { return "scope=r_network"; }
    }
    public RequestLevelModes RequestLevelMode
    {
        get { return RequestLevelModes.AtRequestToken; }
    }
    public List<KeyValuePair<string, string>> DemoMethodArguments
    {
        get
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

            result.Add(new KeyValuePair<string, string>("format", "json"));

            return result;
        }
    }
    public List<string> TransposeResults(dynamic jsonResult)
    {
        List<string> transposedResult = new List<string>();
        List<dynamic> results = jsonResult.values as List<dynamic>;

        foreach (dynamic d in results)
        {
            transposedResult.Add(d.firstName + " " + d.lastName);
        }
        return transposedResult;
    }
}