using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Environment;
using NFX.Serialization.JSON;

namespace NFX.ApplicationModel.Pile
{
    /// <summary>
    /// Provides config options for cache tables
    /// </summary>
    [Serializable]
    public sealed class TableOptions : INamed
    {
        public const long INITIAL_CAPACITY_DEFAULT = 256 * 16;
        public const long INITIAL_CAPACITY_MIN = 1024;

        public const double GROWTH_FACTOR_DEFAULT = 2.0d;
        public const double GROWTH_FACTOR_MIN = 1.20d;
        public const double GROWTH_FACTOR_MAX = 4.0d;

        public const double SHRINK_FACTOR_DEFAULT = 0.75d;
        public const double SHRINK_FACTOR_MIN = 0.50d;
        public const double SHRINK_FACTOR_MAX = 0.90d;

        public const double LOAD_FACTOR_LWM_DEFAULT = 0.50d;
        public const double LOAD_FACTOR_LWM_MIN = 0.10d;
        public const double LOAD_FACTOR_LWM_MAX = 0.70d;

        public const double LOAD_FACTOR_HWM_DEFAULT = 0.70d;
        public const double LOAD_FACTOR_HWM_MIN = 0.45d;
        public const double LOAD_FACTOR_HWM_MAX = 0.95d;

        public const int DEFAULT_MAX_AGE_SEC_DEFAULT = 15 * 60;


        public TableOptions(IConfigSectionNode node, bool nameRequired = true)
        {
            if (nameRequired)
            if (node==null || node.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.IsNullOrWhiteSpace())
                throw new PileException(StringConsts.ARGUMENT_ERROR + "TableOptions.ctor($name=null|Empty)");

            ConfigAttribute.Apply(this, node);

            if (this.m_Name.IsNullOrWhiteSpace())
             m_Name = Guid.NewGuid().ToString();
        }

        public TableOptions(string name)
        {
            if (name.IsNullOrWhiteSpace())
                throw new PileException(StringConsts.ARGUMENT_ERROR + "TableOptions.ctor(name=null|Empty)");

            m_Name = name;
        }


        [Config("$name")]internal string m_Name;
        private long m_InitialCapacity = INITIAL_CAPACITY_DEFAULT;
        private long m_MinimumCapacity;
        private long m_MaximumCapacity;
        private double m_GrowthFactor = GROWTH_FACTOR_DEFAULT;
        private double m_ShrinkFactor = SHRINK_FACTOR_DEFAULT;
        private double m_LoadFactorLWM = LOAD_FACTOR_LWM_DEFAULT;
        private double m_LoadFactorHWM = LOAD_FACTOR_HWM_DEFAULT;

        private int m_DefaultMaxAgeSec = DEFAULT_MAX_AGE_SEC_DEFAULT;


        public string Name       { get{ return m_Name;} }


        /// <summary>
        /// How many elements a table may have at minimum, the property is checked at shrinking.
        /// Zero = no limit.
        /// The value is dependent on bucket count, so the actual table capacity is bucket-dependent
        /// </summary>
        [Config]
        public long MinimumCapacity
        {
          get{ return m_MinimumCapacity;}
          set { m_MinimumCapacity = value < 0 ? 0 : value;}
        }

        /// <summary>
        /// How many elements a table may have at maximum, the property is checked at growth.
        /// Zero = no limit
        /// </summary>
        [Config]
        public long MaximumCapacity
        {
          get{ return m_MaximumCapacity;}
          set { m_MaximumCapacity = value < 0 ? 0 : value;}
        }


        /// <summary>
        /// How many elements an empty table should contain. The value is dependent on bucket count, so the actual table capacity is bucket-dependent
        /// </summary>
        [Config]
        public long InitialCapacity
        {
          get{ return m_InitialCapacity;}
          set { m_InitialCapacity = value < INITIAL_CAPACITY_MIN ? INITIAL_CAPACITY_MIN : value;}
        }


        /// <summary>
        /// Defines the factor of growth - how much does a table grow when HWM is reached. The number has to be at least 1.2d
        /// </summary>
        [Config]
        public double GrowthFactor
        {
          get{ return m_GrowthFactor;}
          set { m_GrowthFactor = value < GROWTH_FACTOR_MIN ? GROWTH_FACTOR_MIN : value > GROWTH_FACTOR_MAX ? GROWTH_FACTOR_MAX : value;}
        }

        /// <summary>
        /// Defines the factor of shrinking - how much does a table shrink when LWM is reached. The number has to be at most 0.7d
        /// </summary>
        [Config]
        public double ShrinkFactor
        {
          get{ return m_ShrinkFactor;}
          set { m_ShrinkFactor = value < SHRINK_FACTOR_MIN ? SHRINK_FACTOR_MIN : value > SHRINK_FACTOR_MAX ? SHRINK_FACTOR_MAX : value;}
        }

        /// <summary>
        /// Defines the load factor below which the shrinking is triggered
        /// </summary>
        [Config]
        public double LoadFactorLWM
        {
          get{ return m_LoadFactorLWM;}
          set { m_LoadFactorLWM = value < LOAD_FACTOR_LWM_MIN ? LOAD_FACTOR_LWM_MIN : value > LOAD_FACTOR_LWM_MAX ? LOAD_FACTOR_LWM_MAX : value;}
        }

        /// <summary>
        /// Defines the load factor above which the growth is triggered
        /// </summary>
        [Config]
        public double LoadFactorHWM
        {
          get{ return m_LoadFactorHWM;}
          set { m_LoadFactorHWM = value < LOAD_FACTOR_HWM_MIN ? LOAD_FACTOR_HWM_MIN : value > LOAD_FACTOR_HWM_MAX ? LOAD_FACTOR_HWM_MAX : value;}
        }


        /// <summary>
        /// Specifies default max age length which is applied to cache items if Put() does not specify particular max age
        /// </summary>
        [Config]
        public int DefaultMaxAgeSec
        {
          get { return m_DefaultMaxAgeSec;}
          set { m_DefaultMaxAgeSec = value > 0 ? value : 0;}
        }


        /// <summary>
        /// True to include instrumentation details per table
        /// </summary>
        [Config]
        public bool DetailedInstrumentation
        {
          get; set;
        }



        /// <summary>
        /// Allows to get/set options as external parameters
        /// </summary>
        public object AsExternalParameter
        {
          get
          {
            var mc = new MemoryConfiguration();
            mc.Create("opt");

            foreach(var pi in this.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(ConfigAttribute))))
            {
              mc.Root.AddAttributeNode(pi.Name, pi.GetValue(this));
            }
            return CoreConsts.EXT_PARAM_CONTENT_LACONIC + mc.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.Compact);
          }
          set
          {
            try
            {
                  if (value==null) return;

                  if (value is IConfigSectionNode)
                  {
                    var name = m_Name;
                    ConfigAttribute.Apply(this, value as IConfigSectionNode);
                    m_Name = name;
                  }

                  ConfigSectionNode node = null;

                  if (value is JSONDataMap)
                    node = ((JSONDataMap)value).ToConfigNode();
                  else
                  {
                    var str = value.ToString();

                    if (str.IsNullOrWhiteSpace()) return;

                    if (str.StartsWith(CoreConsts.EXT_PARAM_CONTENT_LACONIC))
                    {
                      str = str.Remove(0, CoreConsts.EXT_PARAM_CONTENT_LACONIC.Length);
                      node = str.AsLaconicConfig();
                      if (node==null) node = ("opt{"+str+"}").AsLaconicConfig(handling: ConvertErrorHandling.Throw);
                    }
                    else
                    {
                      if (str.StartsWith(CoreConsts.EXT_PARAM_CONTENT_JSON))
                       str = str.Remove(0, CoreConsts.EXT_PARAM_CONTENT_JSON.Length);

                      var map = JSONReader.DeserializeDataObject( str ) as JSONDataMap;
                      if (map==null)
                        throw new PileException(str);

                      node = map.ToConfigNode();
                    }

                  }
                  var wasname = m_Name;
                  ConfigAttribute.Apply(this, node);
                  m_Name = wasname;

            }
            catch(Exception error)
            {
                throw new PileException(StringConsts.ARGUMENT_ERROR + GetType().Name+".AsExternalParameter.set({0})".Args(value) + error.ToMessageWithType());
            }
          }


        }


        /// <summary>
        /// Makes an identical copy of this instance
        /// </summary>
        public TableOptions Clone()
        {
          using(var ms = new MemoryStream())
          {
             var ser = new Serialization.Slim.SlimSerializer();
             ser.Serialize(ms, this);
             ms.Position = 0;
             return ser.Deserialize(ms) as TableOptions;
          }
        }


    }
}
