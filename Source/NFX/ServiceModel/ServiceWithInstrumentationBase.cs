using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Instrumentation;

namespace NFX.ServiceModel
{
  /// <summary>
  /// Provides base implementation for Service with IInstrumentable logic
  /// </summary>
  public abstract class ServiceWithInstrumentationBase<TDirector> : Service<TDirector>, IInstrumentable where TDirector : class
  {

    protected ServiceWithInstrumentationBase() : base()
    {
    }

    protected ServiceWithInstrumentationBase(TDirector director) : base(director)
    {
    }

    /// <summary>
    /// Turns instrumentation on/off
    /// </summary>
    public abstract bool InstrumentationEnabled
    {
      get;
      set;
    }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters{ get { return ExternalParameterAttribute.GetParameters(this); } }

    /// <summary>
    /// Returns named parameters that can be used to control this component
    /// </summary>
    public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
    {
      return ExternalParameterAttribute.GetParameters(this, groups);
    }

    /// <summary>
    /// Gets external parameter value returning true if parameter was found
    /// </summary>
    public virtual bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
        return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
    }

    /// <summary>
    /// Sets external parameter value returning true if parameter was found and set
    /// </summary>
    public virtual bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return ExternalParameterAttribute.SetParameter(this, name, value, groups);
    }
  }
}
