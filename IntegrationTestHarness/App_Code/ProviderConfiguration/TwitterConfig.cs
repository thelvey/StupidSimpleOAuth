using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for TwitterConfig
/// </summary>
public class TwitterConfig : IProviderConfig
{
    public string RequestTokenUrl
    {
        get { return "https://api.twitter.com/oauth/request_token"; }
    }

    public string UserAuthUrl
    {
        get { return "https://api.twitter.com/oauth/authorize"; }
    }

    public string AccessTokenUrl
    {
        get { return "https://api.twitter.com/oauth/access_token"; }
    }

    public string MethodUrl
    {
        get { return "https://api.twitter.com/1"; }
    }

    public string DemoMethodUrl
    {
        get { return MethodUrl + "/statuses/user_timeline.json"; }
    }
    public string RequestLevel
    {
        get { return "permission=read"; }
    }
    public RequestLevelModes RequestLevelMode
    {
        get { return RequestLevelModes.AtUserAuthPage; }
    }
    public List<KeyValuePair<string, string>> DemoMethodArguments
    {
        get { return null; }
    }
    public List<string> TransposeResults(dynamic jsonResult)
    {
        List<string> transposedResult = new List<string>();
        dynamic[] results = jsonResult as dynamic[];

        foreach (dynamic d in results)
        {
            transposedResult.Add(d.text);
        }
        return transposedResult;
    }
}