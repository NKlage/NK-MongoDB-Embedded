using System;
using System.Runtime.InteropServices;
using NK.MongoDB.Embedded.Enums;

namespace NK.MongoDB.Embedded
{
    public class MonogServerBuilder
    {
        private Os _operatingSystem;
        private Distribution _linuxDistribution;
        private MongoDbVersion _mongoDbVersion;
        private OsArchitecture _architecture;
        private const string DownloadUrl = "https://fastdl.mongodb.org/";
        
        internal Os OperatingSystem => _operatingSystem;
        internal Distribution LinuxDistribution => _linuxDistribution;
        internal MongoDbVersion MongoDbVersion => _mongoDbVersion;
        internal OsArchitecture Architecture => _architecture;
        internal string ArchiveName => GetArchiveName();


        public MonogServerBuilder()
        {
            _operatingSystem = DetectOs();
        }
        
        public MonogServerBuilder UseOs(Os os)
        {
            _operatingSystem = os;
            return this;
        }

        /// <summary>
        /// Linux distribution on which the MongoDB will be used.
        /// Only relevant for Linux OS.
        /// </summary>
        /// <param name="distribution"><see cref="Distribution"/></param>
        /// <returns><see cref="MongoServer"/></returns>
        public MonogServerBuilder UseDistribution(Distribution distribution)
        {
            _linuxDistribution = distribution;
            return this;
        }

        /// <summary>
        /// MongoDB version to be used
        /// </summary>
        /// <param name="mongoDbVersion"><see cref="MongoDbVersion"/></param>
        /// <returns><see cref="MongoServer"/></returns>
        public MonogServerBuilder UseMongoVersion(MongoDbVersion mongoDbVersion)
        {
            _mongoDbVersion = mongoDbVersion;
            return this;
        }

        /// <summary>
        /// Specifies the server architecture
        /// X86_64
        /// </summary>
        /// <param name="architecture"><see cref="Architecture"/></param>
        /// <returns><see cref="MongoServer"/></returns>
        public MonogServerBuilder UseArchitecture(OsArchitecture architecture)
        {
            _architecture = architecture;
            return this;
        }
        
        /// <summary>
        /// Build the MongoDb Server URL
        /// </summary>
        /// <returns>Download URL</returns>
        public string Build()
        {
            return _operatingSystem switch
            {
                Os.Osx => BuildOsxDownloadUrl(),
                Os.Windows => BuildWindowsDownloadUrl(),
                Os.Linux => BuildLinuxDownloadUrl(),
                _  => throw new ArgumentException(message: "Invalid OS value", paramName: nameof(_operatingSystem))
            };
        }

        private string BuildWindowsDownloadUrl()
        {
            return $"{DownloadUrl}win32/mongodb-win32-x86_64-2012plus-{GetMongoServerVersion(_mongoDbVersion)}.zip";
        }

        private string BuildOsxDownloadUrl()
        {
            return $"{DownloadUrl}{_operatingSystem.ToString().ToLower()}/mongodb-macos-{_architecture}-{GetMongoServerVersion(_mongoDbVersion)}.tgz";
        }

        private string BuildLinuxDownloadUrl()
        {
            return
                $"{DownloadUrl}{_operatingSystem.ToString().ToLower()}/mongodb-{_operatingSystem.ToString().ToLower()}-" +
                $"{_architecture}-{_linuxDistribution.ToString().Replace("_", "").ToLower()}-" +
                $"{GetMongoServerVersion(_mongoDbVersion)}.tgz";
        }

        private string GetMongoServerVersion(MongoDbVersion mongoDbVersion)
        {
            return mongoDbVersion.ToString().Replace("V", "").Replace("_", ".");
        }

        private string GetArchiveName()
        {
            string[] urlSegments = Build().Split("/");
            return urlSegments[^1];
        }
        
        private Os DetectOs() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Os.Osx;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return Os.Linux;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Os.Windows;   
             
            return Os.Windows;             
        }
    }
}