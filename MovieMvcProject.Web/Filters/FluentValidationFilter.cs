
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace MovieMvcProject.Web.Filters
{
    public class FluentValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
           
            if (!context.HttpContext.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            
            var model = context.ActionArguments.Values
                .FirstOrDefault(v => v != null &&
                    context.HttpContext.RequestServices.GetService(
                        typeof(IValidator<>).MakeGenericType(v.GetType())) != null);

            if (model == null)
            {
                await next();
                return;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;
            if (validator == null)
            {
                await next();
                return;
            }

            var validationContext = new ValidationContext<object>(model);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var controller = context.Controller as Controller;

                
                foreach (var error in result.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName ?? string.Empty, error.ErrorMessage ?? string.Empty);
                }

                context.Result = new ViewResult
                {
                    ViewName = context.RouteData.Values["action"]?.ToString(),
                    ViewData = controller?.ViewData ?? new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState),
                    TempData = controller?.TempData
                };

                
                if (context.Result is ViewResult vr && vr.ViewData != null)
                {
                    vr.ViewData.Model = model;
                }
                return;
            }

            await next();
        }
    }
}