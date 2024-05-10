using NHibernate.Cache;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;


namespace MultiThreadedWebServer
{
	internal class Server
	{
        static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..");
        static readonly Dictionary<string, byte[]> ResponseCache = new();
        static readonly object CacheLock = new();
        static int index = 0;


        public static void StartWebServer()
        {
			using HttpListener listener = new();
			listener.Prefixes.Add("http://localhost:5050/");
			listener.Start();
			Console.WriteLine("Listening for requests.");

			while (true)
			{
				HttpListenerContext context = listener.GetContext();
				ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
			}
		}

        static void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url != null)
            {
                string requestUrl = request.Url.LocalPath;
                Console.WriteLine($"Request received: {requestUrl}");

                byte[] cachedResponse;
                lock (CacheLock)
                {
                    if (ResponseCache.ContainsKey(requestUrl))
                    {
                        cachedResponse = ResponseCache[requestUrl];
                        Console.WriteLine("Cached response found.");
                        response.OutputStream.Write(cachedResponse, 0, cachedResponse.Length);
                        response.Close();
                        return;
                    }
                }
                Stopwatch stopwatch = Stopwatch.StartNew();

                string filePath = Path.Combine(RootFolder, requestUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);

                    ThreadPool.QueueUserWorkItem(_ => Logic.ConvertToGifThreadPool(filePath, ++index));

                    lock (CacheLock)
                    {
                        ResponseCache[requestUrl] = fileBytes;
                    }

                    response.ContentType = GetContentType(filePath);
                    response.ContentLength64 = fileBytes.Length;
                    response.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                    Console.WriteLine("A .gif file has been created.");
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    string errorMessage = $"File not found: {requestUrl}";
                    byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                    response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
                }

                response.Close();
                stopwatch.Stop();
                Console.WriteLine($"Request processed in {stopwatch.ElapsedMilliseconds} milliseconds.");
            }
        }

        static string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
