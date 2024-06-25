using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Call CreateRedirectionRulesFile() function to create the rules of redirection 
            CreateRedirectionRulesFile();
            //Start server
            Server serv = new Server(1000, "redirectionRules.txt");
            // 1) Make server object on port 1000
            // 2) Start Server
            serv.StartServer();
        }

        static void CreateRedirectionRulesFile()
        {
            try
            {
                string fileName = "redirectionRules.txt";

                // Check if the file already exists
                if (File.Exists(fileName))
                {
                    Console.WriteLine($"File '{fileName}' already exists.");
                    return;
                }

                // Create a new file for writing the redirection rules
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    // Write redirection rules to the file
                    writer.WriteLine("aboutus.html,aboutus2.html");

                    Console.WriteLine($"Redirection rules file '{fileName}' created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating redirection rules file: {ex.Message}");
            }
        }
    }
}