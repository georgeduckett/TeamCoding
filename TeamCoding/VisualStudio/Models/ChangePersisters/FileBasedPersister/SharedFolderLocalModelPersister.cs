using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.VisualStudio.Models.ChangePersisters.FileBasedPersister
{
    public class SharedFolderLocalModelPersister : FileBasedLocalModelPersisterBase
    {
        protected override string PersistenceFolderPath => TeamCodingPackage.Current.Settings.SharedSettings.FileBasedPersisterPath;
        public SharedFolderLocalModelPersister(LocalIDEModel model) : base(model) { }
        public static Task<string> FolderPathIsValid(string folderPath)
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(folderPath))
                {
                    return "Directory not found";
                }

                try
                {
                    File.Create(Path.Combine(folderPath, "test.tmp")).Dispose();
                }
                catch(Exception ex)
                {
                    return "Failed to create test file" + Environment.NewLine + Environment.NewLine + ex.ToString();
                }
                try
                {
                    File.Delete(Path.Combine(folderPath, "test.tmp"));
                }
                catch (Exception ex)
                {
                    return "Failed to delete test file" + Environment.NewLine + Environment.NewLine + ex.ToString();
                }

                return null;
            });
        }
    }
}
