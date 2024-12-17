using FootballLeague.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FootballLeague.API.Helpers
{
    public static class ControllerProcessor
    {
        public static async Task<IActionResult> ProcessAsync<T>(Func<Task<T>> action, ControllerBase controller, bool checkModelState = false)
        {
            if (checkModelState && !controller.ModelState.IsValid)
            {
                return controller.BadRequest(controller.ModelState);
            }

            try
            {
                T? result = await action();

                if (result != null && result.Equals(true))
                {
                    return controller.Ok();
                }

                if (result != null && result.Equals(false))
                {
                    throw new AppException($"No data found!").SetStatusCode(404);
                }

                if (result == null || (result is IEnumerable<object> enumerable && !enumerable.Any()))
                {
                    throw new AppException($"No data found!").SetStatusCode(404);
                }

                if (result is IActionResult actionResult)
                {
                    return actionResult;
                }

                return controller.Ok(result);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new AppException($"Oops! Something went wrong! Please try again later.").SetStatusCode(500);
            }
        }
    }
}