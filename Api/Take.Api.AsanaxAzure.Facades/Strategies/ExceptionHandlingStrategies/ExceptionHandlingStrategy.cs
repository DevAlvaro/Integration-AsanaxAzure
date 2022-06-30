using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Take.Api.AsanaxAzure.Facades.Strategies.ExceptionHandlingStrategies
{
    public abstract class ExceptionHandlingStrategy
    {
        public abstract Task<HttpContext> HandleAsync(HttpContext context, Exception exception);
    }
}
