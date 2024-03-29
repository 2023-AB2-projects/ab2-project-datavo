﻿using System.Net;
using Server.Logging;
using Server.Server.Http;
using Server.Server.Responses;

namespace Server.Server;

internal class HttpServer
{
    private readonly HttpListener _httpListener;

    public HttpServer()
    {
        _httpListener = new HttpListener();
        //_httpListener.Prefixes.Add("http://+:8001/");
        _httpListener.Prefixes.Add("http://localhost:8001/");
    }

    public async Task Start()
    {
        Logger.Info("Starting server on port 8001");
        _httpListener.Start();
        Logger.Info("Server listening on port 8001");

        while (true)
        {
            var context = await _httpListener.GetContextAsync();

            _ = Task.Run(() => ProcessRequest(context));
        }
    }

    private static async Task ProcessRequest(HttpListenerContext context)
    {
        try
        {
            Logger.Info($"New Request from {context.Request.UserHostName}");

            var response = await Task.Run(() => Router.HandleRequest(context));

            await WriteResponse(context, response);
        }
        catch (Exception ex)
        {
            await WriteResponse(context, new ErrorResponse(ex));
        }
    }

    public static async Task WriteResponse(HttpListenerContext context, Response response)
    {
        using var sw = new StreamWriter(context.Response.OutputStream);
        await sw.FlushAsync();
        await sw.WriteAsync(response.ToJson());
    }
}