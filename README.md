StupidSimpleOAuth
======================

I created this library with the intent of creating the simplest OAuth implementation possible.

This library is not intended for production use and does, in fact, favor simplicity over error handling, logging, or any other features you might find in production ready code.

The goal was to create an OAuth implementaton to easily demonstrate how simple OAuth can be while keeping it loosely coupled enough that new OAuth integrations can be declared at run time.

The original insipiration for this library was actually DotNetOpenAuth. When I first tried to use DNOA before I fully understood OAuth, it was overwhelming. So I chose instead to take the time to actually learn the OAuth spec and write my own implementaton.

##Versions of OAuth Spec Supported
Currently, only 1.0a is supported.

##Using the Library
Included in the solution is a web site project named IntegrationTestHarness. This web site contains an example of how StupidSimpleOAuth can be consumed. Implementations.aspx contains a fully working integration against Twitter and LinkedIn APIs.