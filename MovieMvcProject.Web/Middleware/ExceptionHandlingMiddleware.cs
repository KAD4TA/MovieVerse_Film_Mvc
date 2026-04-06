using MovieMvcProject.Application.Commons.Exceptions;
using MovieMvcProject.Application.Interfaces.ILocalization;

namespace MovieMvcProject.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            
            var localizationService = context.RequestServices.GetRequiredService<ILocalizationService>();

            
            var localizer = localizationService.GetLocalizer("ExceptionResource");

            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                var message = Uri.EscapeDataString(localizer["NotFound", ex.Message]);
                context.Response.Redirect($"/Error/Error404?message={message}");
            }
            catch (ForbiddenAccessException)
            {
                var message = Uri.EscapeDataString(localizer["Forbidden"]);
                context.Response.Redirect($"/Error/Error403?message={message}");
            }
            catch (UnauthorizedException)
            {
                var message = Uri.EscapeDataString(localizer["Unauthorized"]);
                context.Response.Redirect($"/Error/Error401?message={message}");
            }
            catch (BadRequestException ex)
            {
                var message = ex.Errors != null
                    ? Uri.EscapeDataString(string.Join(", ", ex.Errors.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value)}")))
                    : Uri.EscapeDataString(localizer["BadRequest", ex.Message]);

                context.Response.Redirect($"/Error/Error400?message={message}");
            }
            catch (ConflictException ex)
            {
                var message = Uri.EscapeDataString(localizer["Conflict", ex.Message]);
                context.Response.Redirect($"/Error/Error409?message={message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                var message = Uri.EscapeDataString(localizer["ServerError"]);
                context.Response.Redirect($"/Error/Error500?message={message}");
            }
        }
    }
}