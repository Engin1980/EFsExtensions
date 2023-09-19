using Eng.Chlaot.ChlaotModuleBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FailuresModule.Types.Old
{
    public class FailGroup : NotifyPropertyChangedBase
    {
        public enum ESelector
        {
            Any,
            One,
            All,
            None
        }

        public FailGroup() : this("?") { }

        public FailGroup(string title)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Groups = new();
            Failures = new();
            void list_Changed(object? sender, ListChangedEventArgs e)
            {
                InvokePropertyChanged(nameof(Items));
            };

            Groups.ListChanged += list_Changed;
            Failures.ListChanged += list_Changed;
        }

        private void list_Changed(object? sender, ListChangedEventArgs e)
        {
            InvokePropertyChanged(nameof(Items));
        }

        public BindingList<FailGroup> Groups
        {
            get => GetProperty<BindingList<FailGroup>>(nameof(Groups))!;
            set
            {
                UpdateProperty(nameof(Groups), value);
                if (value != null)
                    value.ListChanged += list_Changed;
            }
        }

        public BindingList<Failure> Failures
        {
            get => GetProperty<BindingList<Failure>>(nameof(Failures))!;
            set
            {
                UpdateProperty(nameof(Failures), value);
                if (value != null)
                    value.ListChanged += list_Changed;
            }
        }

        public string Title { get; set; }

        public FailureFrequency Frequency
        {
            get => GetProperty<FailureFrequency>(nameof(Frequency))!;
            set => UpdateProperty(nameof(Frequency), value);
        }

        public ESelector Selector { get; set; }

        public List<object> Items
        {
            get
            {
                List<object> ret = new();
                ret.AddRange(Groups.OrderBy(q => q.Title));
                ret.AddRange(Failures.OrderBy(q => q.Definition.Title));
                return ret;
            }
        }
    }
}
