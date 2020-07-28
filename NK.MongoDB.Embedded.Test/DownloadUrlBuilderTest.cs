using System;
using System.Collections.Immutable;
using FluentAssertions;
using NK.MongoDB.Embedded.Enums;
using Xunit;

namespace NK.MongoDB.Embedded.Test
{
    public class DownloadUrlBuilderTest
    {
        [Fact]
        public void Should_Get_Ubuntu_1604_URL()
        {
            // Arrange
            MonogServerBuilder builder = new MonogServerBuilder()
                .UseOs(Os.Linux)
                .UseDistribution(Distribution.Ubuntu_1604)
                .UseArchitecture(OsArchitecture.x86_64)
                .UseMongoVersion(MongoDbVersion.V4_3_6);
            
            // Act
            string downloadUrl = builder.Build();
            
            // Assert
            downloadUrl.Should()
                .Be("https://fastdl.mongodb.org/linux/mongodb-linux-x86_64-ubuntu1604-4.3.6.tgz", 
                    "Builder should generate the correct URL for Ubuntu 16.04");
        }

        [Fact]
        public void Should_Get_macOS_URL()
        {
            // Arrange
            MonogServerBuilder builder = new MonogServerBuilder()
                .UseOs(Os.Osx)
                .UseMongoVersion(MongoDbVersion.V4_3_6);
            
            // Act
            string downloadUrl = builder.Build();
            
            // Assert
            downloadUrl.Should().Be("https://fastdl.mongodb.org/osx/mongodb-macos-x86_64-4.3.6.tgz", 
                "Builder should generate the correct URL for macOS");
        }

        [Fact]
        public void Should_Get_Windows_URL()
        {
            // Arrange
            MonogServerBuilder builder = new MonogServerBuilder()
                .UseOs(Os.Windows)
                .UseMongoVersion(MongoDbVersion.V4_3_6);
            
            // Act
            string downloadUrl = builder.Build();
            
            // Assert
            downloadUrl.Should().Be("https://fastdl.mongodb.org/win32/mongodb-win32-x86_64-2012plus-4.3.6.zip", 
                "Builder should generate the correct URL for Windows");
        }
    }
}