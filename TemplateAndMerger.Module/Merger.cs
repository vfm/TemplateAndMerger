using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplateAndMerger.Module.Interfaces;

namespace TemplateAndMerger.Module
{
    [DomainComponent]
    public class Merger<T> : IMerger<T> where T : XPBaseObject
    {
        private List<T> _mlist = null;

        public Merger()
        {
            _mlist = new List<T>();
        }

        public List<T> ListToMerge
        {
            get { return _mlist; }
            set { _mlist = value; }
        }

        [DataSourceProperty("ListToMerge")]
        public T WinnerObject
        {
            get;
            set;
        }

        public void MergeObjects()
        {
            //
        }
    }
}