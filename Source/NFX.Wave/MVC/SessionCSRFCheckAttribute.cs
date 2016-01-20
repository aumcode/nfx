using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFX.Wave.MVC
{
  /// <summary>
  /// Decorates controller actions that need to check CSRF token against the user session
  /// </summary>
  public sealed class SessionCSRFCheckAttribute : ActionFilterAttribute
  {
    public const string DEFAULT_TOKEN_NAME = "token";


    public SessionCSRFCheckAttribute()
    {
      TokenName = DEFAULT_TOKEN_NAME;
    }

    public SessionCSRFCheckAttribute(string tokenName) : this(tokenName, true)
    {
    }

    public SessionCSRFCheckAttribute(string tokenName, bool onlyExistingSession)
    {
      TokenName = tokenName;
     
      if (TokenName.IsNullOrWhiteSpace())
        TokenName = DEFAULT_TOKEN_NAME;

      OnlyExistingSession = onlyExistingSession;
    }

    
    public string TokenName{ get; private set; }
    public bool OnlyExistingSession{ get; private set; }


    protected internal override bool BeforeActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      if (work.IsGET) return false;

      work.NeedsSession(OnlyExistingSession);

      var session = work.Session; 
      var supplied = work.MatchedVars[TokenName].AsString();

      if (session==null || 
          !session.CSRFToken.EqualsOrdIgnoreCase(supplied))
        throw new HTTPStatusException(NFX.Wave.SysConsts.STATUS_400, NFX.Wave.SysConsts.STATUS_400_DESCRIPTION, "CSRF failed");

      return false;
    }

    protected internal override bool AfterActionInvocation(Controller controller, WorkContext work, string action, MethodInfo method, object[] args, ref object result)
    {
      return false;
    }
  }
}
