using System;
using System.Runtime.InteropServices;
using NK.MongoDB.Embedded.Enums;
using MongoDB.Entities;
using MongoDB.Entities.Core;

namespace NK.MongoDB.Embedded.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            int mongoDbServerPort = 21020;
            
            MonogServerBuilder builder = new MonogServerBuilder()
                .UseMongoVersion(MongoDbVersion.V4_2_8);
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                builder.UseOs(Os.Windows);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                builder.UseOs(Os.Osx);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                builder.UseOs(Os.Linux).UseDistribution(Distribution.Ubuntu_1804);
            
            MongoServer mongoServer = new MongoServer(builder).UseMongoServerPort(mongoDbServerPort);
            mongoServer.Start().GetAwaiter().GetResult();

            new DB("embedded-test", "127.0.0.1", mongoDbServerPort);

            BookModel book1 = new BookModel
            {
                Author = "Nico Klage",
                Title = "How to use NK.MongoDB.Embedded Package"
            };

            book1.Save();
            mongoServer.Stop().Cleanup();
        }
    }

    class BookModel : IEntity
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}