using System;

namespace GameLib
{

public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
public class ValueChangedEventArgs : EventArgs
{ public ValueChangedEventArgs(object old) { OldValue=old; }
  public object OldValue;
}

public struct Pair // TODO: look through code and find places this could be used
{ public Pair(object a, object b) { First=a; Second=b; }

  public bool IsType(Type type) { return IsType(type, false); }
  public bool IsType(Type type, bool allowNull)
  { return allowNull ? (First==null || First.GetType()==type) && (Second==null || Second.GetType()==type) :
                       First!=null && First.GetType()==type && Second!=null && Second.GetType()==type;
  }
  public void CheckType(Type type) { CheckType(type, false); }
  public void CheckType(Type type, bool allowNull)
  { if(!IsType(type, allowNull))
      throw new ArgumentException(String.Format("Expected a pair of {0} (nulls {1}allowed)", type,
                                                allowNull ? "" : "not "));
  }

  public object First, Second;
}

internal class Global
{ private Global() { }
  public static Random Rand = new Random();
}

} // namespace GameLib