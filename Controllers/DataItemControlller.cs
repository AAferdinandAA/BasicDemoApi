using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text.Json;
using BasicDemoApi.Models;

namespace BasicDemoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LargeDataController : ControllerBase
{
    private const int actualCount = 100000;

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

        var random = new Random();
        // 获取当前的 Unix 毫秒时间戳作为起点
        long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

<<<<<<< HEAD
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
=======
        // 定义步长：例如每条数据间隔 1000 毫秒（1秒）
        int stepMs = 1000;

        for (int i = 0; i < actualCount; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            var dataItem = new DataItem
            {
                // 使用索引 i 乘以步长，确保每一条数据的时间戳绝对不同
                Time = startTime + (long)i * stepMs,

                // 生成具有波动性的数据，方便观察图表
                Value = Math.Sin(i * 0.1) * 100 + random.NextDouble() * 50
            };

            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(dataItem);
            await outputStream.WriteAsync(jsonBytes, cancellationToken);
            await outputStream.WriteAsync(new byte[] { (byte)'\n' }, cancellationToken);

            // 每 1000 条数据 Flush 一次
            if (i % 1000 == 0)
                await outputStream.FlushAsync(cancellationToken);
>>>>>>> 4918f9d5c5d1a0ebefe64110266a457069ec44c8
        }

       

     
    }
}