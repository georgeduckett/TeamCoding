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
        private readonly Func<TProperty, Task<string>> InvalidReasonFunc;
        private bool? IsValid = null;
        public SettingProperty(object owner, Func<TProperty, Task<string>> invalidReasonFunc = null) { Owner = owner; InvalidReasonFunc = invalidReasonFunc; }
        private TProperty _Value;
        public TProperty Value
        {
            get { return _Value; }
            set
            {
                if (!EqualityComparer<TProperty>.Default.Equals(_Value, value))
                {
                    Changing?.Invoke(Owner, EventArgs.Empty);
                    IsValid = null;
                    _Value = value;
                    Changed?.Invoke(Owner, EventArgs.Empty);
                }
            }
        }
        public async Task<bool> IsValidAsync() => IsValid ?? (bool)(IsValid = await GetNewValueInvalidReasonAsync(Value).ContinueWith(t => !t.IsFaulted && t.Result == null));
        public Task<string> GetNewValueInvalidReasonAsync(TProperty newValue) => InvalidReasonFunc == null ? Task.FromResult<string>(null) : InvalidReasonFunc(newValue);
        public event EventHandler Changing;
        public event EventHandler Changed;
    }
}