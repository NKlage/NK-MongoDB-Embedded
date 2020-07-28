[![Build status](https://ci.appveyor.com/api/projects/status/74xitqk7gks58o0u/branch/master?svg=true)](https://ci.appveyor.com/project/NKlage/nk-mongodb-embedded/branch/master)

This project makes it possible to run unit test on a real MongoDB Instance without 
mocking the persistence layer.

# General

The `MongoServerBuilder` is the base for the Mongo Server-Version to be used. 
Based on the OS-Configuration and the MongoDB Version, the corresponding download 
link generated.

## Supported OS
    Windows
    macOS
    Linux

## Supported Linux Distributions
    Ubuntu 16.04
    Ubunut 18.04

## Supported MongoDB-Versions
    4.2.0
    4.2.1
    4.2.2
    4.2.3
    4.2.5
    4.2.6
    4.2.7
    4.2.8
    4.3.6

`MongoServer` downloads the TAR archive (or ZIP for Windows) based on the 
`MongoServerBuilder` configuration and unpacks it into the Home directory.

Example:

```csharp

    public class ServerTest
    {
        [Fact]
        public async Task Should_Download_and_Start_Server()
        {
            // Arrange
            MonogServerBuilder builder = new MonogServerBuilder()
                .UseMongoVersion(MongoDbVersion.V4_2_0);
            
            // Checks the underlying operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                builder.UseOs(Os.Windows);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                builder.UseOs(Os.Osx);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                builder.UseOs(Os.Linux).UseDistribution(Distribution.Ubuntu_1804);
            
            MongoServer mongoServer = new MongoServer(builder)
                .UseMongoServerPort(27099);
            
            // Act
            await mongoServer.Start();
            mongoServer
                .Stop()
                .Cleanup(); // Remove MongoDB-Data after server stop
            
            // Assert
        }
    }

```

# TODO

- Command line argument passed to server start
- Pass MongoDB Configuration File
- Configure download & data directory