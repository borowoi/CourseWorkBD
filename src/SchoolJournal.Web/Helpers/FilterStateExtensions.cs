using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SchoolJournal.Web.Helpers;

/// <summary>
/// Сохранение состояния страницы (фильтры, сортировка, номер страницы) в сессии:
/// после ухода со страницы и возврата по ссылке меню условия применяются снова.
/// </summary>
public static class FilterStateExtensions
{
    /// <summary>
    /// Если в адресе есть параметры, они применяются и запоминаются.
    /// Если параметров нет (переход по меню), восстанавливается сохранённое состояние.
    /// Параметр reset=true сбрасывает сохранённое состояние.
    /// </summary>
    public static T ResolveFilter<T>(this Controller controller, T filter, string key, bool reset)
        where T : class, new()
    {
        var session = controller.HttpContext.Session;

        if (reset)
        {
            session.Remove(key);
            return new T();
        }

        var hasParameters = controller.Request.Query.Keys
            .Any(k => !string.Equals(k, "reset", StringComparison.OrdinalIgnoreCase));

        if (!hasParameters)
        {
            var json = session.GetString(key);
            if (json is not null)
            {
                var saved = JsonSerializer.Deserialize<T>(json);
                if (saved is not null)
                    return saved;
            }
            return filter;
        }

        session.SetString(key, JsonSerializer.Serialize(filter));
        return filter;
    }
}
