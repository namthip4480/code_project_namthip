using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WARP.OpenShotParser.Models;

namespace WARP.OpenShotParser.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class WarpAnalyticsController : ControllerBase
    {
        private static readonly List<StudentLogModel> _studentLogs = new();
        private static string _latestEsp32Signal = "SIG_IDLE";

        // POST: api/logs/submit -> บันทึกการตอบคำถาม
        [HttpPost("logs/submit")]
        public IActionResult SubmitStudentLog([FromBody] StudentLogModel log)
        {
            if (log == null) return BadRequest(new { success = false, message = "Invalid log payload" });

            log.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            log.Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            
            _studentLogs.Add(log);

            if (!string.IsNullOrEmpty(log.Esp32Signal))
            {
                _latestEsp32Signal = log.Esp32Signal;
            }

            return Ok(new { success = true, logId = log.Id, signal = _latestEsp32Signal });
        }

        // GET: api/logs -> ดึงข้อมูล Log ไปแสดงใน Dashboard
        [HttpGet("logs")]
        public IActionResult GetStudentLogs()
        {
            return Ok(_studentLogs);
        }

        // DELETE: api/logs/clear -> ล้างประวัติ Log
        [HttpDelete("logs/clear")]
        public IActionResult ClearLogs()
        {
            _studentLogs.Clear();
            _latestEsp32Signal = "SIG_IDLE";
            return Ok(new { success = true });
        }

        // GET: api/esp32/status -> บอร์ด ESP32 ดึงไปแสดงผลไฟ/มอเตอร์
        [HttpGet("esp32/status")]
        public IActionResult GetEsp32Status()
        {
            return Ok(new {
                latestSignal = _latestEsp32Signal,
                timestamp = DateTime.Now.ToString("HH:mm:ss")
            });
        }

        // POST: api/esp32/trigger -> บอร์ด ESP32 ส่งสัญญาณปุ่มกดเข้ามา
        [HttpPost("esp32/trigger")]
        public IActionResult TriggerFromEsp32([FromBody] Esp32PayloadRequest payload)
        {
            if (payload == null) return BadRequest();

            _latestEsp32Signal = payload.SignalCode;
            
            return Ok(new { 
                success = true, 
                receivedSignal = payload.SignalCode 
            });
        }
    }
}