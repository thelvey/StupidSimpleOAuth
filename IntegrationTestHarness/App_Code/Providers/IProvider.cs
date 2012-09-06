using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SimpleOAuth.OAuth;

/// <summary>
/// Summary description for Class1
/// </summary>
public interface IProvider
{
    readonly string RequestTokenUrl { get; }
    readonly string UserAuthUrl { get; }
    readonly string AccessTokenUrl { get; }
    readonly string MethodUrl { get; }

    AuthRequestResult GenerateUnauthorizedRequestToken(OAuthConsumerConfig config);
    AccessToken ExchangeForAccessToken(OAuthConsumerConfig config, string authToken, string tokenSecret, string verifier);
}