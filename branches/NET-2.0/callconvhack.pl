#!/usr/bin/perl
while(<>)
{ #if (/\.class .*CallConv.*Attribute/) { $incallconvattributeimpl=1; next; }
  #if(/\/\/ end of class .*CallConv.*Attribute/) { $incallconvattributeimpl=0; next; }
  if(/\.custom instance void .*CallConv(.*)Attribute/) { $pcc = $1; next; }
  if ($pcc)
  { if(m/^(\s*(?:instance ?)?[\w\.]+) (Invoke\s*\(.*)/)
    { print
      print "$1 modopt([mscorlib]System.Runtime.CompilerServices.CallConv$pcc) $2\n";
      $pcc = 0;
      next;
    }
  }
  print unless $incallconvattributeimpl;
}