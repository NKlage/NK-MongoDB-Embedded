using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NK.MongoDB.Embedded.Enums;
using Xunit;

namespace NK.MongoDB.Embedded.Test
{
    public class ServerTest
    {
        [Fact]
        public async Task Should_Download_and_Start_Server()
        {
            // Arrange
            MonogServerBuilder builder = new MonogServerBuilder()
                .UseMongoVersion(MongoDbVersion.V4_2_0);

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
                .Cleanup();
            
            // Assert
        }
    }
}