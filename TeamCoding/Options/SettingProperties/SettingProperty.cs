using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Options
{
    public class SettingProperty<TProperty>
    {
        private readonly object Owner;
        private readonly Func<TProperty, bool> IsValidFunc;
        public SettingProperty(object owner, Func<TProperty, bool> isValidFunc = null) { Owner = owner; IsValidFunc = isValidFunc; }
        private TProperty _Value;
        public TProperty Value
        {
            get { return _Value; }
            set
            {
                if (!EqualityComparer<TProperty>.Default.Equals(_Value, value))
                {
                    Changing?.Invoke(Owner, EventArgs.Empty);
                    _Value = value;
                    Changed?.Invoke(Owner, EventArgs.Empty);
                }
            }
        }
        public bool IsValid => IsValidFunc == null || IsValidFunc(Value);
        public event EventHandler Changing;
        public event EventHandler Changed;
    }
}
