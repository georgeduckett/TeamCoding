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
        private readonly Property<string>[] SettingProperties;
        public LocalModelPersisterBase(LocalIDEModel model, params Property<string>[] settingProperties)
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
        private void SettingProperty_Changing(object sender, EventArgs e)
        {
            SendModel(new RemoteIDEModel(new LocalIDEModel()));
        }
        private void IdeModel_ModelChanged(object sender, EventArgs e)
        {
            SendChanges();
        }
        private void SettingProperty_Changed(object sender, EventArgs e)
        {
            SendChanges();
        }
        private void SendChanges() => SendModel(new RemoteIDEModel(IdeModel));
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
