/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MongoDB.Bson;

namespace MongoDB.Driver.TestConsoleApplication
{
    class Program
    {
        static StreamWriter ConfigureConsoleOutputToFile(FileStream fileStream)
        {
            FileStream oldStream;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                writer = new StreamWriter(fileStream);
            }
            catch (Exception e)
            {
                DiagnosticLog("Cannot open log.txt for writing. Exception: " + e);
                throw;
            }

            Console.SetOut(writer);
            return writer;
        }

        static void RunSsh(string command)
        {
            try
            {
                // process start info
                System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + "ssh ubuntu@ec2-3-21-232-207.us-east-2.compute.amazonaws.com " + command);
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true; // Don't show console

                // create the process
                using System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = processStartInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                DiagnosticLog(output);
            }
            catch (Exception exception)
            {
                DiagnosticLog("ssh exception:" + exception + "\n");
            }
        }

        static void Main(string[] args)
        {
            const string MONGODB_URI = "mongodb://admin:Sup3rS3kr3t@ec2-3-21-232-207.us-east-2.compute.amazonaws.com,ec2-3-138-142-135.us-east-2.compute.amazonaws.com,ec2-18-216-182-45.us-east-2.compute.amazonaws.com/?replicaSet=rs0&authSource=admin";
            var url = new MongoUrl(MONGODB_URI);
            var settings = MongoClientSettings.FromUrl(url);

            MongoInternalDefaults.RttTimeout = TimeSpan.FromSeconds(5);
            MongoInternalDefaults.RttReadTimeout = TimeSpan.FromSeconds(3);

            //void socketConfigurator(Socket socket)
            //{
            //    const SocketOptionName TCP_USER_TIMEOUT = (SocketOptionName)18;
            //    const uint timeoutMS = 5000;
            //    var timeoutBytes = BitConverter.GetBytes(timeoutMS);
            //    // TCP_USER_TIMEOUT = TCP_KEEPIDLE + TCP_KEEPINTVL * TCP_KEEPCNT
            //    // See https://blog.cloudflare.com/when-tcp-sockets-refuse-to-die/ for more information.
            //    //socket.SetRawSocketOption((int)SocketOptionLevel.Tcp, (int)TCP_USER_TIMEOUT, timeoutBytes);
            //    //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            //    //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 3);
            //    //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            //    //socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
            //}
            settings.ClusterConfigurator = cb =>
            {
                cb.Subscribe(new CustomEventSubscriber());
                //cb.ConfigureTcp(tcp => tcp.With(socketConfigurator: (Action<Socket>)socketConfigurator));
            };

            using var fileStream = new FileStream(@"c:\Logs\bosh.log", FileMode.OpenOrCreate, FileAccess.Write);
            using var writeStream = ConfigureConsoleOutputToFile(fileStream);

            DiagnosticLog($"Starting");

            var client = new MongoClient(settings);
            var db = client.GetDatabase("dotnet_test");
            var coll = db.GetCollection<BsonDocument>("test");
            coll.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            Console.WriteLine($"{DateTime.UtcNow:O}\tDeleted all documents");
            int counter = 0;

            var stopwatch = new Stopwatch();
            var wholeOperation = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    if (wholeOperation.Elapsed.TotalSeconds > 3)
                    {
                        // break primary
                        DiagnosticLog($"Breaking primary sending.");
                        RunSsh("./blackhole-on.sh");
                        DiagnosticLog($"Breaking primary has been finished.");
                    }

                    var document = new BsonDocument { { "timestamp", DateTime.UtcNow }, { "counter", counter } };

                    stopwatch.Restart();
                    coll.InsertOne(document);
                    stopwatch.Stop();

                    DiagnosticLog($"Insert counter {counter}");

                    counter++;
                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        DiagnosticLog($"Recovered from loss of primary. Test terminated.");
                        break;
                    }

                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    DiagnosticLog("Failed to insert into collection. Exception: " + ex);
                }
            }
        }

        private static void DiagnosticLog(string message)
        {
            Console.WriteLine($"\n##{DateTime.UtcNow:O}. Diagnostic log: " + message + "\n");
        }
    }
}
