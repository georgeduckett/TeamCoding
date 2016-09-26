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

            IdeModel.ModelChanged += IdeModel_ModelChanged;
            foreach (var property in SettingProperties)
            {
                property.Changing += SettingProperty_Changing;
                property.Changed += SettingProperty_Changed;
            }
        }
        private async void SettingProperty_Changing(object sender, EventArgs e)
        {
            if (await RequiredPropertiesAreSet())
            {
                SendModel(new RemoteIDEModel(new LocalIDEModel()));
            }
        }
        protected virtual async Task<bool> RequiredPropertiesAreSet()
        {
            if (SettingProperties.Any(p => string.IsNullOrEmpty(p.Value))) return false;

            var propertyTasks = SettingProperties.Select(p => p.IsValidAsync).ToArray();

            await Task.WhenAll(propertyTasks);

            return propertyTasks.Any(pt => !pt.Result);
        }
        private async void IdeModel_ModelChanged(object sender, EventArgs e)
        {
            await SendChanges();
        }
        private async void SettingProperty_Changed(object sender, EventArgs e)
        {
            await SendChanges();
        }
        private async Task SendChanges()
        {
            if (await RequiredPropertiesAreSet())
            {
                SendModel(new RemoteIDEModel(IdeModel));
            }
        }
        protected abstract void SendModel(RemoteIDEModel remoteModel);
        public virtual void Dispose()
        {
            foreach (var property in SettingProperties)
            {
                property.Changing += SettingProperty_Changing;
                property.Changed += SettingProperty_Changed;
            }
        }
    }
}
