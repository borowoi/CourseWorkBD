using System.Globalization;

namespace SchoolJournal.Web.Helpers;

/// <summary>Преобразование фильтров в параметры ссылок (для пагинации и сортировки).</summary>
public static class RouteHelper
{
    /// <summary>Возвращает непустые свойства объекта в виде набора параметров маршрута.</summary>
    public static Dictionary<string, string> ToRouteValues(this object filter, params string[] exclude)
    {
        var result = new Dictionary<string, string>();
        foreach (var property in filter.GetType().GetProperties())
        {
            if (exclude.Contains(property.Name))
                continue;

            var value = property.GetValue(filter);
            if (value is null)
                continue;

            string? text = value switch
            {
                DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                bool b => b ? "true" : null,
                string s => string.IsNullOrWhiteSpace(s) ? null : s,
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };

            if (text is not null)
                result[property.Name] = text;
        }
        return result;
    }

    /// <summary>Копия набора параметров с изменённым (или удалённым, если значение пусто) ключом.</summary>
    public static Dictionary<string, string> With(
        this Dictionary<string, string> source, string key, string? value)
    {
        var copy = new Dictionary<string, string>(source);
        if (string.IsNullOrEmpty(value))
            copy.Remove(key);
        else
            copy[key] = value;
        return copy;
    }
}
