﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace NGinnBPM.ProcessModel
{
    [DataContract]
    public abstract class NodeDef : IValidate, IHaveExtensionProperties
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public Dictionary<string, string> ExtensionProperties { get; set; }
        
        [IgnoreDataMember]
        public CompositeTaskDef Parent { get; set; }
        [IgnoreDataMember]
        public ProcessDef ParentProcess { get; set; }

        public abstract bool Validate(List<string> problemsFound);


        #region IHaveExtensionProperties Members

        public IEnumerable<string> GetExtensionProperties(string xmlns)
        {
            throw new NotImplementedException();
        }

        public string GetExtensionProperty(string xmlns, string name)
        {
            throw new NotImplementedException();
        }

        public string GetExtensionProperty(string fullName)
        {
            throw new NotImplementedException();
        }

        public void SetExtensionProperty(string xmlns, string name, string value)
        {
            ExtensionProperties[xmlns + ":" + name] = value;
        }

        #endregion

        [IgnoreDataMember]
        public IEnumerable<FlowDef> FlowsOut
        {
            get
            {
                if (Parent == null) throw new Exception();
                return Parent.Flows.Where(x => x.From == this.Id);
            }
        }

        [IgnoreDataMember]
        public IEnumerable<FlowDef> FlowsIn
        {
            get
            {
                if (Parent == null) throw new Exception();
                return Parent.Flows.Where(x => x.From == this.Id);
            }
        }
        [IgnoreDataMember]
        public IEnumerable<NodeDef> NodesOut
        {
            get
            {
                return this.FlowsOut.Select(x => Parent.GetNode(x.To));
            }
        }

        [IgnoreDataMember]
        public IEnumerable<NodeDef> NodesIn
        {
            get
            {
                return this.FlowsIn.Select(x => Parent.GetNode(x.From));
            }
        }

    }
}
