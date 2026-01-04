namespace Ting.Compiler
{
    // Base class for all expressions
    public abstract record Expression
    {
    }

    public record FunctionApplicationExpression(Expression Function, Expression Argument) : Expression
    {
    }

    public record ReferenceExpression(Expression Argument) : Expression
    {
    }

    public record DeclareExpression(Expression Argument) : Expression
    {
    }

}