namespace SchoolJournal.BusinessLogic.Exceptions;

/// <summary>Исключение, возникающее при нарушении правила предметной области.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
