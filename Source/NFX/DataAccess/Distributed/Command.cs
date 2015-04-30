/*<FILE_LICENSE>
* NFX (.NET Framework Extension) Unistack Library
* Copyright 2003-2014 IT Adapter Inc / 2015 Aum Code LLC
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.DataAccess.Cache;
using System.Collections;

namespace NFX.DataAccess.Distributed
{
    /// <summary>
    /// Defines a command sent into IDistributedDataStore implementor to retrieve or change data.
    /// A Command is a named bag of paremeters where every parameter has a name and a value 
    /// </summary>
    [Serializable]
    public class Command : List<Command.Param>, IParameters, INamed, IULongHashProvider, IShardingIDProvider
    {
            #region Inner Classes
                /// <summary>
                /// Represents a distributed command parameter
                /// </summary>
                [Serializable]
                public sealed class Param : IParameter
                {
                    public Param(string name, object value = null)
                    {
                        m_Name = name;
                        m_Value = value;
                    }

                    #region Fields
                        private string m_Name;  
                        private object m_Value; 
                    #endregion

                    #region Properties
                        public string Name   { get { return m_Name ?? string.Empty; } }

                        public object Value  { get { return m_Value;} }

                        public bool HasValue { get {return true; } }

                        public bool IsInput  { get { return true;} }
                    #endregion

                    #region Public

                        public override string ToString()
                        {
                          return "{0}='{1}'".Args(m_Name ?? StringConsts.NULL_STRING, m_Value ?? StringConsts.NULL_STRING);
                        }

                        public override int GetHashCode()
                        {
                          var result = Name.GetHashCode();
                          var valueHC = 0;
                          if (m_Value!=null)
                          {
                             if (m_Value is IStructuralEquatable)
                              valueHC = ((IStructuralEquatable)m_Value).GetHashCode(StructuralComparisons.StructuralEqualityComparer);
                             else
                              valueHC = m_Value.GetHashCode();
                          }
                          return result ^ valueHC;
                        }

                        /// <summary>
                        /// Equates param by name and values
                        /// </summary>
                        public override bool Equals(object obj)
                        {
                          var other = obj as Param;
                          if (other==null) return false;
                          if (!string.Equals(m_Name, other.m_Name, StringComparison.InvariantCultureIgnoreCase)) return false;

                          if (this.m_Value==null && other.m_Value==null) return true;

                          if (this.m_Value!=null)
                          {
                            if (this.m_Value is IStructuralEquatable)
                            {                                                    
                              return ((IStructuralEquatable)this.m_Value).Equals(other.m_Value, StructuralComparisons.StructuralEqualityComparer);
                            }
                            return this.m_Value.Equals(other.m_Value);
                          }

                          return false;
                        }
                    #endregion
                }


                /// <summary>
                /// Used to denote a list of values in command params, use this class instead of array as it gives better hash distribution
                /// </summary>
                [Serializable]
                public class PList : List<object>
                {
                  public override bool Equals(object obj)
                  {
                    var other = obj as PList;
                    if (other==null) return false;
                    
                    //loop is faster that SequenceEquals()
                    if (this.Count!=other.Count) return false;
                    for(var i=0; i<this.Count; i++)
                     if (!this[i].Equals(other[i])) return false; 
                    
                    return true;
                  }

                  public override int GetHashCode()
                  {
                    var csum = this.Count;
                    for(var i=0; i<this.Count; i++)
                     csum ^= this[i].GetHashCode() << (i % 3);
                    return csum;
                  }
                }
            #endregion
        
        #region .ctor
            
            public Command(string name, params Param[] pars )
            {
                m_Name = name;
                if (pars!=null) AddRange(pars);
            }

            public Command(string name, Parcel shardingParcel, params Param[] pars ) : this(name, 
                                                                                            shardingParcel.NonNull(text: "Command.ctor(shardingParcel=null)").GetType(), 
                                                                                            shardingParcel.NonNull(text: "Command.ctor(shardingParcel=null)").ShardingID, 
                                                                                            pars)
            {
               
            }

            public Command(string name, Type shardingParcel, object shardingID, params Param[] pars )
            {
                m_Name = name;
                
                if (shardingParcel!=null)
                 if (!typeof(Parcel).IsAssignableFrom(shardingParcel))
                   throw new DistributedDataAccessException(StringConsts.ARGUMENT_ERROR+ GetType().FullName+".ctor(shardingParcel='"+shardingParcel.FullName+"' isnot Parcel-derived");
                
                m_ShardingParcel = shardingParcel;
                m_ShardingID = shardingID;

                if (pars!=null) AddRange(pars);
            }

        #endregion
        
        #region Fields
            
            private string m_Name;

            private Type m_ShardingParcel;
            private object m_ShardingID;

        #endregion


        #region Properties
            
            /// <summary>
            /// Returns Command name, providers use it to locate modules particular to backend implementation that they represent
            /// </summary>
            public virtual string Name
            {
                get { return m_Name ?? string.Empty;}
            }

            /// <summary>
            /// Returns the type of parcel that sharding is performed on or null.
            /// </summary>
            public virtual Type ShardingParcel
            {
                get { return m_ShardingParcel; }
            }

            /// <summary>
            /// Returns the ID used for sharding or null
            /// </summary>
            public virtual object ShardingID
            {
                get { return m_ShardingID; }
            }


            /// <summary>
            /// Returns parameter by its name or null
            /// </summary>
            public Param this[string name]
            {
                get {return FindParamByName(name) as Param;}
            }
            
            
            public IEnumerable<IParameter> AllParameters
            {
                get { return this; }
            }
           
       #endregion

       #region Public
            
            public IParameter ParamByName(string name)
            {
                return this.First(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }

            public IParameter FindParamByName(string name)
            {
                return this.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.InvariantCultureIgnoreCase));
            }

            public override string ToString()
            {
                return "{0}[{1}]({2})".Args(GetType().FullName, Name, this.Count());
            }


            public override bool Equals(object obj)
            {
              var other = obj as Command;
              if (other==null) return false;
              if (this.GetType() != other.GetType()) return false;
              if (!string.Equals(this.Name, other.Name, StringComparison.InvariantCultureIgnoreCase)) return false;

              //Loop is faster that SequenceEquals()
              //if (!this.SequenceEqual(other)) return false;
              if (this.Count!=other.Count) return false;
              for(var i=0; i<this.Count; i++)
               if (!this[i].Equals(other[i])) return false;

              if (this.ShardingParcel != other.ShardingParcel) return false;//Type may use ref comparison
              
              if (this.ShardingID!=null) 
              {
               if (other.ShardingID==null) return false;
               if (!this.ShardingID.Equals(other.ShardingID)) return false;
              }
              else if (other.ShardingID!=null) return false;

              return true;
            }

            public override int GetHashCode()
            {
              var result = this.Name.GetHashCode();
              var csum = this.Count;
              for(var i=0; i<this.Count; i++)
               csum ^= this[i].GetHashCode() << (i % 3);
              return result ^ csum;
            }

            public ulong GetULongHash()
            {
              ulong result = ((ulong)this.Name.GetHashCode() << 32) ^ ((ulong)this.GetType().Name.GetHashCode() << 32);
              var csum = this.Count;
              for(var i=0; i<this.Count; i++)
               csum ^= this[i].GetHashCode() << (i % 3);
              return result ^ (ulong)(uint)csum;
            }

       #endregion
            
    }



}
