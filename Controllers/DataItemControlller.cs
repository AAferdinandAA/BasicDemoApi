using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text.Json;
using BasicsDemoApi.Models;

namespace BasicsDemoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LargeDataController : ControllerBase
{
    private const int TotalCount = 1000000;

    [HttpGet("stream")]
    public async Task GetLargeDataStreamNdjson(CancellationToken cancellationToken)
    {
        var acceptEncoding = Request.Headers.AcceptEncoding.ToString().ToLower();
        Stream outputStream = Response.Body;

        // 检测客户端压缩能力
        if (acceptEncoding.Contains("br"))
            outputStream = new BrotliStream(Response.Body, CompressionLevel.Optimal, leaveOpen: true);
        else if (acceptEncoding.Contains("gzip"))
            outputStream = new GZipStream(Response.Body, CompressionLevel.Optimal, leaveOpen: true);

        if (outputStream != Response.Body)
            Response.Headers.ContentEncoding = acceptEncoding.Contains("br") ? "br" : "gzip";

        // 设置响应头
        Response.ContentType = "application/x-ndjson; charset=utf-8";
        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";

        // 数据生成
        var random = new Random();
        var baseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - TotalCount * 1000;

        for (int i = 0; i < TotalCount; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var dataItem = new DataItem
            {
                Time = baseTime + i * 1000,
                Value = random.NextDouble() * 1000
            };

            // 序列化到 NDJSON 
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(dataItem);

            // 写入流 + 换行
            await outputStream.WriteAsync(jsonBytes, cancellationToken);
            await outputStream.WriteAsync(new byte[] { (byte)'\n' }, cancellationToken);

            // 每 1000 条 flush
            if (i % 1000 == 0)
                await outputStream.FlushAsync(cancellationToken);
        }

        await outputStream.FlushAsync(cancellationToken);
        // 不手动 Dispose 压缩流，ASP.NET Core 自动处理
    }
}
