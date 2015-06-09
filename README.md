# Push2NugetServer
Pack and Push *.nupkg to self Nuget Server.

- Install Push2SelfNuGetServer.vsix.
- To ensure that the nuget package is enabled.
- Add the Nuget.xml file in the.Nuget folder and set the ServerUrl and ApiKey.
 
NuGet.xml like this：

    <?xml version="1.0" encoding="utf-8"?>
    <SelfServer> 
      <Url>http://localhost:88</Url> 
      <ApiKey>123</ApiKey>
    </SelfServer>
