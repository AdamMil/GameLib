#!/usr/bin/perl
while(<>)
{ if(/^\.module extern SDL_image$/) { s/SDL_image/SDL/ }
  elsif(/^\.module extern SDL$/) { s/SDL/SDL_image/ }

  if (/\.class .* CallConv.*Attribute/) { $incallconvattributeimpl=1; next; }
  if(/\/\/ end of class .*CallConv.*Attribute/) { $incallconvattributeimpl=0; next; }
  if(/\.custom instance void .*CallConv(.*)Attribute/) { $pcc = $1; next; }
  if ($pcc)
  { if(m/^\s*(?:instance default [\w\.]+ )?Invoke\s*\(/)
    { print "modopt([mscorlib]System.Runtime.CompilerServices.CallConv$pcc)\n";
      $pcc = 0;
    }
  }
  print unless $incallconvattributeimpl;
}
