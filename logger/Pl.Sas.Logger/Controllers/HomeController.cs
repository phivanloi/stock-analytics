using Microsoft.AspNetCore.Mvc;
using Pl.Sas.Logger.Data;
using Pl.Sas.Logger.Models;
using System.Diagnostics;

namespace Pl.Sas.Logger.Controllers
{
    public class HomeController : Controller
    {
        private readonly LoggerData _loggerData;

        public HomeController(
            LoggerData loggerData)
        {
            _loggerData = loggerData;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/getnewerlog")]
        public async Task<IActionResult> GetNewerLogAsync(long? startTime = null, int? type = null)
        {
            return Json(await _loggerData.FindAllAsync(1000, startTime, type));
        }

        [Route("/getcontent")]
        public async Task<IActionResult> GetContentAsync(string id)
        {
            var log = await _loggerData.FindAsync(id);
            return Content(log?.Content ?? "");
        }

        [Route("/clearlog")]
        public async Task<IActionResult> ClearLogAsync()
        {
            await _loggerData.ClearLogAsync();
            return Redirect($"/");
        }

        [HttpPost]
        [HeaderChecker]
        [Route("/write")]
        public async Task<IActionResult> WriteAsync([FromBody] WriteObjectModel writeObject)
        {
            var writeItem = new LogEntry()
            {
                Id = Utilities.GenerateShortGuid(),
                CreatedTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Content = writeObject.FullMessage,
                Host = writeObject.Host,
                Message = writeObject.Message,
                Type = writeObject.Type
            };
            await _loggerData.WriteAsync(writeItem);
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
