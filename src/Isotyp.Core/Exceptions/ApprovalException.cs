namespace Isotyp.Core.Exceptions;

/// <summary>
/// Exception thrown when an approval workflow is violated.
/// </summary>
public class ApprovalException : DomainException
{
    public ApprovalException(string message) : base(message) { }
}
