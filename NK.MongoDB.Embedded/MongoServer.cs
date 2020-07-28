using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using NK.MongoDB.Embedded.Enums;

namespace NK.MongoDB.Embedded
{
    /// <summary>
    /// Manages the Mongo Server
    /// </summary>
    public class MongoServer
    {
        private readonly MonogServerBuilder _builder;
        private readonly string _userHomePath;
        private string _mongoHome = "mongo-embedded";
        private Process _mongoProcess;
        private int _mongoServerPort = 27017;
        private string _dataFolder;
        private bool _cleanup = false;
        private WebProxy _proxy;
        private NetworkCredential _credentails;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="monogServerBuilder"><see cref="MonogServerBuilder"/></param>
        public MongoServer(MonogServerBuilder monogServerBuilder)
        {
            _builder = monogServerBuilder;
            _userHomePath = (Environment.OSVersion.Platform == PlatformID.Unix || 
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
                
            
        }

        /// <summary>
        /// Set Mongo Server Port, default is Port 27017
        /// </summary>
        /// <param name="port">MongoDB Server Port</param>
        /// <returns><see cref="MongoServer"/></returns>
        public MongoServer UseMongoServerPort(int port)
        {
            _mongoServerPort = port;
            return this;
        }

        /// <summary>
        /// Remove Data Directory
        /// </summary>
        /// <returns><see cref="MongoServer"/></returns>
        public MongoServer Cleanup()
        {
            Directory.Delete(_dataFolder, true);
            return this;
        }

        /// <summary>
        /// Proxy configuration, accepts optional proxy credentails
        /// </summary>
        /// <param name="proxy">Proxy configuration</param>
        /// <param name="credentials">Proxy credentials</param>
        /// <returns><see cref="MongoServer"/></returns>
        public MongoServer UseProxy(WebProxy proxy, NetworkCredential credentials = null)
        {
            _proxy = proxy;
            _credentails = credentials;
            return this;
        }
        
        /// <summary>
        /// Start Mongo Server
        /// </summary>
        /// <returns><see cref="MongoServer"/></returns>
        public async Task Start()
        {
            await Download();
            string serverFolder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Extract ZIP for Windows OS
                ExtractZip(Path.Combine(_userHomePath, _mongoHome, _builder.ArchiveName), Path.Combine(_userHomePath, _mongoHome));
                serverFolder = Path.Combine(_userHomePath, _mongoHome, _builder.ArchiveName).Replace(".zip", ""); // TODO: Get with Function
            }
            else
            {
                // Extract for other Plattforms
                ExtractTgz(Path.Combine(_userHomePath, _mongoHome, _builder.ArchiveName), Path.Combine(_userHomePath, _mongoHome));
                serverFolder = Path.Combine(_userHomePath, _mongoHome, _builder.ArchiveName).Replace(".tgz", ""); // TODO: Get with Function
            }

            if(string.IsNullOrEmpty(serverFolder))
                throw new Exception("Server Folder not defined");
            
            SetPermissions();
            
            _dataFolder = Path.Combine(serverFolder, "data", Guid.NewGuid().ToString());

            Directory.CreateDirectory(_dataFolder);
            
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.Combine(serverFolder, "bin"),
                FileName = Path.Combine(serverFolder, "bin", "mongod"),
                Arguments = $"--port {_mongoServerPort} --sslMode disabled --dbpath {_dataFolder}",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            _mongoProcess = Process.Start(startInfo);
            _mongoProcess.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
            _mongoProcess.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
            Console.WriteLine($"MongoDb started on Port {_mongoServerPort}");
        }

        /// <summary>
        /// Stop Mongo Server
        /// </summary>
        public MongoServer Stop()
        {
            _mongoProcess.Kill();
            return this;
        }

        /// <summary>
        /// Download MongoDB Binaries
        /// </summary>
        /// <returns></returns>
        private async Task Download()
        {
            string archive = Path.Combine(_userHomePath, _mongoHome, _builder.ArchiveName);
            if (!File.Exists(archive))
            {
                if (!Directory.Exists(Path.Combine(_userHomePath, _mongoHome)))
                    Directory.CreateDirectory(Path.Combine(_userHomePath, _mongoHome));
                    
                HttpClient client = null == _proxy ? new HttpClient() : new HttpClient(GetHttpClientHandler());
                HttpResponseMessage response = await client.GetAsync(_builder.Build());
                response.EnsureSuccessStatusCode();
                byte[] result = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(archive, result);
            }
        }

        /// <summary>
        /// ClientHandler for WebProxy
        /// </summary>
        /// <returns><see cref="HttpClientHandler"/></returns>
        private HttpClientHandler GetHttpClientHandler()
        {
            HttpClientHandler handler = null;
            if (null != _proxy)
            {
                handler = new HttpClientHandler
                {
                    Proxy = _proxy,
                    PreAuthenticate = true,
                    UseDefaultCredentials = null == _credentails
                };
                if (null != _credentails)
                    handler.Credentials = _credentails;
            }

            return handler;
        }
        
        /// <summary>
        /// Extracts the TGZ Archive
        /// </summary>
        /// <param name="gzArchiveName">Archive Name</param>
        /// <param name="destFolder">Destination Folder</param>
        private void ExtractTgz(string gzArchiveName, string destFolder)
        {
           if(Directory.Exists(Path.Combine(destFolder, gzArchiveName).Replace(".tgz", "")))
           {
               Console.WriteLine($"Archive Folder exists => {Path.Combine(destFolder, gzArchiveName).Replace(".tgz", "")}");
               return;
           }
           
            Stream inStream = File.OpenRead(gzArchiveName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }

        /// <summary>
        /// Extracts the ZIP Archive
        /// </summary>
        /// <param name="zipArchiveName">Archive Name</param>
        /// <param name="destFolder">Destination Folder</param>
        private void ExtractZip(string zipArchiveName, string destFolder)
        {
            if(Directory.Exists(Path.Combine(destFolder, zipArchiveName).Replace(".zip", "")))
            {
                Console.WriteLine($"Archive Folder exists => {Path.Combine(destFolder, zipArchiveName).Replace(".zip", "")}");
                return;
            }
            
            System.IO.Compression.ZipFile.ExtractToDirectory(zipArchiveName, destFolder);
        }

        /// <summary>
        /// Sets the permissions to execute the binaries for non Windows Systems
        /// </summary>
        void SetPermissions()
        {
            // Do nothing if it runs under Windows
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
            
            string serverFolder = Path.Combine(_userHomePath, _mongoHome, _builder.ArchiveName).Replace(".tgz", "");
            
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Path.Combine(serverFolder, "bin"),
                Arguments = $"a+x {Path.Combine(serverFolder, "bin", "mongod")}",
                FileName = "chmod"
            };
            
            var proc = Process.Start(startInfo);
            proc.WaitForExit();
        }
        

    }
}