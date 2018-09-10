using System;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace TemplateAndMerger.Module.BusinessObjects
{
    [DefaultClassOptions]
    [Mergeable]
    public class Part : BaseObject
    {
        public Part(Session session) : base(session) { }

        private Product product;
        [Association("Product-Parts")]
        public Product Product
        {
            get { return product; }
            set { SetPropertyValue(nameof(Product), ref product, value); }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { SetPropertyValue(nameof(Number), ref number, value); }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { SetPropertyValue(nameof(Name), ref name, value); }
        }
    }
}