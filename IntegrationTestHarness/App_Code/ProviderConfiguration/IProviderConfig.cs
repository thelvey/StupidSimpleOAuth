using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for IProviderConfig
/// </summary>
public enum OAuthProviders { Twitter, LinkedIn };
public enum RequestLevelModes { AtRequestToken, AtUserAuthPage };
public interface IProviderConfig
{
    string RequestTokenUrl { get; }
    string UserAuthUrl { get; }
    string AccessTokenUrl { get; }
    string MethodUrl { get; }
    string DemoMethodUrl { get; }
    string RequestLevel { get; }
    List<KeyValuePair<string, string>> DemoMethodArguments { get; }
    RequestLevelModes RequestLevelMode { get; }
    List<string> TransposeResults(dynamic jsonResult);
}