using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD,
        UNKNOWN
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09,
        UNKNOWN
    }

    class Request
    {
        RequestMethod method;
        public string relativeURI;
        Dictionary<string, string> headerLines = new Dictionary<string, string>();
        HTTPVersion httpVersion;
        string requestString;
        string[] contentLines;

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        public Request(string requestString)
        {
            this.requestString = requestString;
        }

        /// <summary>
        /// Parses the request string and loads the request line, header lines and content, returns false if there is a parsing error
        /// </summary>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
        public bool ParseRequest()
        {
            string[] lines = requestString.Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (lines.Length < 3) return false; // Basic check for minimal request format

            if (!ParseRequestLine(lines[0])) return false;
            if (!LoadHeaderLines(lines.Skip(1).TakeWhile(line => line != "").ToArray())) return false;
            if (!ValidateBlankLine(lines)) return false;

            contentLines = lines.SkipWhile(line => line != "").Skip(1).ToArray(); // Skip past the blank line
            return true;
        }

        private bool ParseRequestLine(string requestLine)
        {
            string[] tokens = requestLine.Split(' ');
            if (tokens.Length != 3) return false;
            ///////////////
            if (!Enum.TryParse(tokens[0], true, out RequestMethod parsedMethod)) return false;
            method = parsedMethod;
            //tokens[0] would be the string representing the HTTP method (e.g., "GET", "POST", "PUT", etc.).
            //Enum.TryParse: This is a static method of the Enum class in C#. It tries to parse a string value (in this case, tokens[0]) into an existing enumeration type
            //true: The second argument (true) specifies that the parsing should be case-insensitive. So, "GET" and "get" would be considered equivalent.
            //out RequestMethod parsedMethod: This declares an output variable named parsedMethod of type RequestMethod. If the parsing is successful, the parsed HTTP method will be stored in this variable.
            if (method != RequestMethod.GET) return false; // Ensure the method is GET

            relativeURI = tokens[1];
            httpVersion = ParseHTTPVersion(tokens[2]);
            headerLines.Add("URI", relativeURI);

            return ValidateIsURI(relativeURI) && httpVersion != HTTPVersion.UNKNOWN;
        }

        private HTTPVersion ParseHTTPVersion(string httpVersionToken)
        {
            if (httpVersionToken == "HTTP/1.1")
            {
                return HTTPVersion.HTTP11;
            }
            else if (httpVersionToken == "HTTP/1.0")
            {
                return HTTPVersion.HTTP10;
            }
            else if (httpVersionToken == "HTTP/0.9")
            {
                return HTTPVersion.HTTP09;
            }
            else
            {
                return HTTPVersion.UNKNOWN;
            }
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }

        private bool LoadHeaderLines(string[] headerLinesArray)
        {
            foreach (var line in headerLinesArray)
            {
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex == -1) return false; // Malformed header line
                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();

                headerLines[key] = value; // Store or replace the header line
            }

            return true;
        }

        private bool ValidateBlankLine(string[] lines)
        {
            // Ensures that a blank line exists between headers and potential content

            return lines.Contains("");
        }
    }
}
