using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents;
using TeamCoding.Options;

namespace TeamCoding.VisualStudio.Models.ChangePersisters
{
    public abstract class LocalModelPersisterBase : ILocalModelPerisister
    {
        private readonly LocalIDEModel IdeModel;
        private readonly SettingProperty<string>[] SettingProperties;
        public LocalModelPersisterBase(LocalIDEModel model, params SettingProperty<string>[] settingProperties)
        {
            SettingProperties = settingProperties;
            IdeModel = model;

            IdeModel.ModelChanged += IdeModel_ModelChangedAsync;
            foreach (var property in SettingProperties)
            {
                property.Changing += SettingProperty_ChangingAsync;
                property.Changed += SettingProperty_ChangedAsync;
            }
        }
        private async void SettingProperty_ChangingAsync(object sender, EventArgs e)
        {
            if (await RequiredPropertiesAreSetAsync())
            {
                SendModel(new RemoteIDEModel(new LocalIDEModel()));
            }
        }
        protected virtual async Task<bool> RequiredPropertiesAreSetAsync()
        {
            if (SettingProperties.Any(p => string.IsNullOrEmpty(p.Value))) return false;

            var propertyTasks = SettingProperties.Select(p => p.IsValidAsync()).ToArray();

            await Task.WhenAll(propertyTasks);

            return propertyTasks.All(pt => pt.Result);
        }
        private async void IdeModel_ModelChangedAsync(object sender, EventArgs e)
        {
            await SendChangesAsync();
        }
        private async void SettingProperty_ChangedAsync(object sender, EventArgs e)
        {
            await SendChangesAsync();
        }
        public Task SendUpdateAsync()
        {
            return SendChangesAsync();
        }
        private async Task SendChangesAsync()
        {
            if (await RequiredPropertiesAreSetAsync())
            {
                SendModel(new RemoteIDEModel(IdeModel));
            }
        }
        protected abstract void SendModel(RemoteIDEModel remoteModel);
        public virtual void Dispose()
        {
            foreach (var property in SettingProperties)
            {
                property.Changing += SettingProperty_ChangingAsync;
                property.Changed += SettingProperty_ChangedAsync;
            }
        }
    }
}
