using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ProviderConfigFactory
/// </summary>
public static class ProviderConfigFactory
{
	public static IProviderConfig GetImplementation(OAuthProviders provider)
	{
        IProviderConfig result = null;

        switch (provider)
        {
            case OAuthProviders.LinkedIn:
                result = new LinkedInConfig();
                break;
            case OAuthProviders.Twitter:
                result = new TwitterConfig();
                break;
        }

        return result;
	}
}