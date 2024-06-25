using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocK = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Server(int portNumber, string redirectionMatrixPath)
        {
            //TODO: call this.LoadRedirectionRules passing redirectionMatrixPath to it
            this.LoadRedirectionRules(redirectionMatrixPath);

            //TODO: initialize this.serverSocket

            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, portNumber);

            this.serverSocK.Bind(ipEndpoint);
        }

        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocK.Listen(2000);

            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket clientSocket = this.serverSocK.Accept();

                Thread newThread = new Thread(new ParameterizedThreadStart(HandleConnection));

                newThread.Start(clientSocket);

            }
        }

        public void HandleConnection(object obj)
        {
            // TODO: Create client socket 
            Socket clientSock = (Socket)obj;
            // set client socket ReceiveTimeout = 0 to indicate an infinite time-out period
            clientSock.ReceiveTimeout = 0;
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] receviedMsg = new byte[1024 * 1024];
                    int recivedLength = clientSock.Receive(receviedMsg);
                    string receivedData = Encoding.ASCII.GetString(receviedMsg);
                    // TODO: break the while loop if receivedLen==0
                    if (recivedLength == 0)
                    {
                        break;
                    }
                    // TODO: Create a Request object using received request string
                    Request cRequest = new Request(receivedData);
                    // TODO: Call HandleRequest Method that returns the response
                    Response sResponse = HandleRequest(cRequest);
                    // TODO: Send Response back to client
                    clientSock.Send(Encoding.ASCII.GetBytes(sResponse.ResponseString));
                }
                catch (Exception ex)
                {
                    // TODO: log exception using Logger class
                    Logger.LogException(ex);
                }
            }

            // TODO: close client socket
            clientSock.Close();
        }

        Response HandleRequest(Request request)
        {
            string content;
            try
            {

                //TODO: check for bad request 
                if (!request.ParseRequest())
                {
                    content = LoadDefaultPage(Configuration.BadRequestDefaultPageName);
                    return new Response(StatusCode.BadRequest, "html", content, "");
                }

                //TODO: map the relativeURI in request to get the physical path of the resource.
                string thePhysicaPath = Path.Combine(Configuration.RootPath, request.relativeURI.Substring(1));


                //TODO: check for redirect
                string redirectionPath = GetRedirectionPagePathIFExist(request.relativeURI.Substring(1));

                if (!String.IsNullOrEmpty(redirectionPath))
                {
                    // Load the content of the redirection target
                    //content = LoadDefaultPage(Configuration.RedirectionDefaultPageName);
                    content = LoadDefaultPage(redirectionPath);
                    // Return a redirect response with the content of the redirection target
                    return new Response(StatusCode.Redirect, "html", content, redirectionPath);
                }


                //TODO: check file exists
                if (!File.Exists(thePhysicaPath))
                {
                    content = LoadDefaultPage(Configuration.NotFoundDefaultPageName);
                    return new Response(StatusCode.NotFound, "html", content, "");
                }

                //TODO: read the physical file
                content = File.ReadAllText(thePhysicaPath);

                // Create OK response
                return new Response(StatusCode.OK, "html", content, "");

            }

            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                // TODO: in case of exception, return Internal Server Error. 
                content = LoadDefaultPage(Configuration.InternalErrorDefaultPageName);
                return new Response(StatusCode.InternalServerError, "html", content, "");
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            // using Configuration.RedirectionRules return the redirected page path if exists else returns empty

            if (Configuration.RedirectionRules.ContainsKey(relativePath))
            {
                return Configuration.RedirectionRules[relativePath];
            }
            else
                return string.Empty;
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string file_path = Path.Combine(Configuration.RootPath, defaultPageName);
            // TODO: check if filepath not exist log exception using Logger class and return empty string
            if (!File.Exists(file_path))
            {
                Logger.LogException(new FileNotFoundException("File Not Found!", file_path));
                return string.Empty;
            }

            // else read file and return its content

            string contentOfFile = File.ReadAllText(file_path);
            return contentOfFile;
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                Configuration.RedirectionRules = new Dictionary<string, string>();
                string[] lines = File.ReadAllLines(filePath);
                //Dictionary<string,string> redirectionsites= new Dictionary<string,string>();
                foreach (string line in lines)
                {
                    string[] sites = line.Split(',');
                    Configuration.RedirectionRules.Add(sites[0], sites[1]);
                    Array.Clear(sites, 0, sites.Length);

                }
                // TODO: using the filepath paramter read the redirection rules from file 
                // then fill Configuration.RedirectionRules dictionary 
            }
            catch (Exception ex)
            {
                // TODO: log exception using Logger class
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }
    }
}