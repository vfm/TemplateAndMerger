using System;
using System.Collections.Generic;
using System.Linq;

namespace TemplateAndMerger.Module.Interfaces
{
    public interface IMerger<T>
    {
        List<T> ListToMerge { get; set; }
        T WinnerObject { get; set; }
        void MergeObjects();
    }
}
