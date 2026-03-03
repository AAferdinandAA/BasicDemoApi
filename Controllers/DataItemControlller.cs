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
        bool isCompressed = false;

        // 1. 根据客户端能力包装流
        if (acceptEncoding.Contains("br"))
        {
            outputStream = new BrotliStream(Response.Body, CompressionLevel.Optimal, leaveOpen: true);
            Response.Headers.ContentEncoding = "br";
            isCompressed = true;
        }
        else if (acceptEncoding.Contains("gzip"))
        {
            outputStream = new GZipStream(Response.Body, CompressionLevel.Optimal, leaveOpen: true);
            Response.Headers.ContentEncoding = "gzip";
            isCompressed = true;
        }

        // 设置响应头
        Response.ContentType = "application/x-ndjson; charset=utf-8";
        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";

        // 数据生成
        var random = new Random();
        var baseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - TotalCount * 1000;

        try
        {
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
        }
   
        finally
        {
            // 这会冲刷（Flush）压缩算法最后的尾部字节并关闭包装流
            if (isCompressed)
            {
                await outputStream.DisposeAsync();
            }
        }

       

     
    }
}
