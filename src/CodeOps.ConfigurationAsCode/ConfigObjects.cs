using System.Linq.Expressions;
using Definit.Configuration;

namespace CodeOps.ConfigurationAsCode;

public static class SectionExtensions
{

    public static ConfigAsCode AddValue<TSection, TValue>(this ConfigAsCode configAsCode, Expression<Func<TSection, TValue>> expression)
        where TSection : 
    {

    }

    private static string SectionName<T>(string sectionName, Expression<T> expression)
    {
        var member = expression.Body as MemberExpression;
        var parameterName = member!.Member.Name;

        return $"{sectionName}:{parameterName}";
    }
}

