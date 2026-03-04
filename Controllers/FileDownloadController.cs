using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;

namespace FileDownloadApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileDownloadController : ControllerBase
    {
        // 基础版接口（同上，可保留）
        [HttpGet("BasicDownload")]
        public IActionResult BasicDownload([FromQuery] string filePath)
        {
            // 基础版代码，略
        }

        /// <summary>
        /// 支持断点续传的文件流下载接口（推荐大文件使用）
        /// </summary>
        /// <param name="filePath">本地文件完整路径</param>
        /// <returns>文件流/文件片段流</returns>
        [HttpGet("ResumeDownload")]
        public IActionResult ResumeDownload([FromQuery] string filePath)
        {
            // 1. 基础校验
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("文件路径不能为空");
            if (!System.IO.File.Exists(filePath))
                return NotFound("文件不存在");

            try
            {
                var fileInfo = new FileInfo(filePath);
                long fileTotalLength = fileInfo.Length; // 文件总大小（字节）
                var fileName = Path.GetFileName(filePath);

                // 2. 解析请求头中的Range（断点续传核心）
                var rangeHeader = Request.Headers["Range"].FirstOrDefault();
                if (string.IsNullOrEmpty(rangeHeader))
                {
                    // 无Range头：返回整个文件流（同基础版，兼容普通下载）
                    var fullStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    Response.Headers.Add("Content-Length", fileTotalLength.ToString());
                    return new FileStreamResult(fullStream, "application/octet-stream")
                    {
                        FileDownloadName = fileName
                    };
                }

                // 3. 解析Range：格式为 Range: bytes=start-end ，需提取start和end
                var rangeParts = rangeHeader.Replace("bytes=", "").Split('-');
                if (!long.TryParse(rangeParts[0], out long start) || start < 0 || start >= fileTotalLength)
                {
                    // 起始位置无效，返回整个文件
                    return BadRequest("无效的文件请求范围");
                }

                // 4. 处理end：若end为空（如 bytes=1024- ），则取文件末尾
                long end = fileTotalLength - 1;
                if (rangeParts.Length > 1 && !string.IsNullOrEmpty(rangeParts[1]) && long.TryParse(rangeParts[1], out long parsedEnd))
                {
                    end = Math.Min(parsedEnd, fileTotalLength - 1); // 防止end超过文件总大小
                }

                // 5. 计算当前返回的片段大小
                long segmentLength = end - start + 1;

                // 6. 打开文件流并定位到起始位置（Seek：跳过前start字节，直接读取指定片段）
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                fileStream.Seek(start, SeekOrigin.Begin); // 定位到Range起始位置

                // 7. 设置断点续传核心响应头
                Response.StatusCode = StatusCodes.Status206PartialContent; // 206表示部分内容（断点续传关键）
                Response.Headers.Add("Accept-Ranges", "bytes"); // 告知客户端支持字节范围请求
                Response.Headers.Add("Content-Length", segmentLength.ToString()); // 当前返回片段的大小
                Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileTotalLength}"); // 当前范围/总大小
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{Uri.EscapeDataString(fileName)}\""); // 解决中文文件名乱码
                Response.ContentType = "application/octet-stream";

                // 8. 返回文件片段流（使用FileStreamResult，自动管理流释放）
                return new FileStreamResult(fileStream, Response.ContentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"文件下载失败：{ex.Message}");
            }
        }
    }
}