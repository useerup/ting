namespace TingCompiler
{
    public abstract class Exp
    {

    }


    public abstract class BinaryExp
    {
        protected BinaryExp(Exp left, Exp right)
        {
            Left = left;
            Right = right;
        }

        public Exp Left { get; set; }

        public Exp Right { get; set; }
    }


    public abstract class UnaryExp
    {
        protected UnaryExp(Exp exp)
        {
            Exp = exp;
        }

        public Exp Exp { get; set; }
    }


    public abstract class ListExp
    {
        protected ListExp(IList<Exp> exps)
        {
            Exps = exps;
        }

        public IList<Exp> Exps { get; set; }
    }


    public sealed class Identifier : Exp
    {
        public Identifier(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }


    public sealed class Apply : BinaryExp
    {
        public Apply(Exp left, Exp right) : base(left, right)
        {
        }
    }


    public sealed class MapTo : BinaryExp
    {
        public MapTo(Exp left, Exp right) : base(left, right)
        {
        }
    }


    public sealed class MapFrom : BinaryExp
    {
        public MapFrom(Exp left, Exp right) : base(left, right)
        {
        }
    }


    public sealed class MapToFrom : BinaryExp
    {
        public MapToFrom(Exp left, Exp right) : base(left, right)
        {
        }
    }


    public sealed class List : ListExp
    {
        public List(IList<Exp> exps) : base(exps)
        {
        }
    }


    public sealed class Record : ListExp
    {
        public Record(IList<Exp> exps) : base(exps)
        {
        }
    }


    public sealed class Set : ListExp
    {
        public Set(IList<Exp> exps) : base(exps)
        {
        }
    }


    public sealed class Reference : UnaryExp
    {
        public Reference(Exp exp) : base(exp)
        {
        }
    }


    public sealed class Declare : UnaryExp
    {
        public Declare(Exp exp) : base(exp)
        {
        }
    }


    public sealed class Property : UnaryExp
    {
        public Property(Exp exp, string name) : base(exp)
        {
            Name = name;
        }

        public string Name { get; set; }
    }


    public sealed class MemberProperty : UnaryExp
    {
        public MemberProperty(Exp exp, string name) : base(exp)
        {
            Name = name;
        }

        public string Name { get; set; }

    }


    public sealed class Lambda : BinaryExp
    {
        public Lambda(Exp left, Exp right) : base(left, right)
        {
        }
    }


    public sealed class InverseLambda : BinaryExp
    {
        public InverseLambda(Exp left, Exp right) : base(left, right)
        {
        }
    }



    public abstract class Scope
    {

    }


    public class DeclarativeScope : Scope
    {
    }


    public class ReferentialScope : Scope
    {
    }


}