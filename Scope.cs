namespace Compiler;
public class Scope
{
    public Scope parentScope;
    public Scope(Scope Parent= null)
    {
        parentScope= Parent;
    }
    public bool WithoutReps=false;
    public List < Expression > Variables= new();
    private void InternalFind(Expression tofind, out Expression Finded, out Scope Where)
    {
        bool b= false;
        Finded = null;
        Where = null;
        foreach(Expression indic in Variables)
        {
            if(tofind.Equals(indic)) 
            {
                Where = this;
                Finded = indic;
                b=true;
            }
        }
        if(!b)
        {
            if(parentScope!=null)
            {
                parentScope.InternalFind(tofind, out Finded, out Where);
            }
            else{
                Where = null;
                Finded = null;
            }
        }
    }
    public bool Find(Expression exp, out ValueType? type)
    {
        Expression Finded;
        Scope Where;
        InternalFind(exp,out Finded, out Where);
        if(Where!= null)
        {
            type= Finded.CheckType;
            return true;
        }
        else
        {
            type = null;
            return false;
        }
    }
    public void AddVar(Expression exp, Expression Value= null)
    {
        Expression Finded;
        Scope Where;
        InternalFind(exp,out Finded, out Where);
        if(Where!= null)
        {
            if(!WithoutReps)
                Where.Variables.Add(Value);
            else
                throw new Exception("A no Reps statement was violated");
        }
        else
        {
            Variables.Add(exp);
        }
    }
}

public enum ValueType
{
    Int,
    String,
    Bool,
    Unassigned,
    Null,
    Void,
    Player,
    Context,
    Predicate,
    CardCollection,
    Card,

    Checked,
}

    
