# Push2NugetServer
Pack and Push *.nupkg to self Nuget Server.

- Install [Push2SelfNuGetServer.vsix](https://visualstudiogallery.msdn.microsoft.com/04c43535-6124-404a-8eea-ae6fb8968c23).
- To ensure that the nuget package is enabled.
- Add the Nuget.xml file in the.Nuget folder and set the ServerUrl and ApiKey.
 
NuGet.xml like this：

    <?xml version="1.0" encoding="utf-8"?>
    <SelfServer> 
      <Url>http://localhost:88</Url> 
      <ApiKey>123</ApiKey>
    </SelfServer>

![push2nugetpic](http://images.cnblogs.com/cnblogs_com/idoudou/682251/o_push2nuget.png)
