using brewlog.application.Interfaces;
using Carter.ModelBinding;
namespace brewlog.api.Extentions
{
    public static class ValidationExtentions
    {
        public static object Validate<T>(this T cmd, HttpContext ctx) where T : IBrewSessionValidate
        {
            var validation = ctx.Request.Validate(cmd);
            return validation.IsValid ? true : validation.GetFormattedErrors();
        }
    }
}
