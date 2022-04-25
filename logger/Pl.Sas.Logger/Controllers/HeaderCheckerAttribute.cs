using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Pl.Sas.Logger.Controllers
{
    public class HeaderCheckerAttribute : TypeFilterAttribute
    {
        public HeaderCheckerAttribute() : base(typeof(HeaderCheckerFilter))
        {
        }

        private class HeaderCheckerFilter : IActionFilter
        {
            private readonly string _apiKey;

            public HeaderCheckerFilter(IConfiguration configuration)
            {
                _apiKey = configuration["AppSettings:SecurityKey"];
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var requestKey = context.HttpContext.Request.Headers["SecurityKey"];
                if (string.IsNullOrEmpty(requestKey) || _apiKey != requestKey)
                {
                    context.Result = new ContentResult() { Content = "", ContentType = "text/plain", StatusCode = (int)HttpStatusCode.BadRequest };
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {

            }
        }
    }

}
