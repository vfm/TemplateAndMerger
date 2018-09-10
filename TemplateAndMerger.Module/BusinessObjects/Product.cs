using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;

namespace TemplateAndMerger.Module.BusinessObjects
{
    [DefaultClassOptions]
    [Mergeable]
    public class Product : BaseObject
    {
        public Product(Session session) : base(session) { }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(nameof(Name), ref name, value); }
        }

        [Association("Product-Parts")]
        public XPCollection<Part> Parts
        {
            get
            {
                return GetCollection<Part>(nameof(Parts));
            }
        }
    }
}