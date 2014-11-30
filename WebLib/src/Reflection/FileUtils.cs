using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Ai.Reflection
{
    public class FileUtils
    {
        public static void CopyFileFromResourceToFile(Assembly assembly, string resourceName,
            string targetFileName)
        {
            using (Stream fromStream = assembly.GetManifestResourceStream(resourceName))
            {
                BinaryReader br = new BinaryReader(fromStream);
                using (Stream toStream = File.Create(targetFileName))
                {
                    BinaryWriter bw = new BinaryWriter(toStream);
                    bw.Write(br.ReadBytes((int)fromStream.Length));
                }
            }
        }
        public static string GetFileText(string filePath)
        {
            string ret = "";
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fileStream))
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                    ret = sr.ReadToEnd();
                }
            }
            return ret;
        }
        public static string GetResourceFileText(Assembly assembly, string resourceName)
        {
            string ret = "";
            using (Stream fromStream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader sr = new StreamReader(fromStream))
                {
                    fromStream.Seek(0, SeekOrigin.Begin);
                    ret = sr.ReadToEnd();
                }
            }
            return ret;
        }
        public static string GenerateTempFileName()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString() + ".tmp";
        }
        public static void SetFileText(string filePath, string text)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(filePath))
                        sw.Write(text);
                }
                catch
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }
        public static void EnsureDirectoryInFileSystemCreated(string folderName, bool isHidden)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
                if (isHidden)
                {
                    DirectoryInfo di = new DirectoryInfo(folderName);
                    di.Attributes = FileAttributes.Hidden;
                }
            }
        }
        public static void CheckOrCreateDirectory(string path, System.Web.UI.WebControls.WebControl control, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new System.Web.HttpException(string.Format(
                    // StringResources.FileUtils_PathCannotBeEmpty, 
                    "FileUtils: {0} {1} error PathCannotBeEmpty {2}", 
                    control.ID, control.GetType().Name, propertyName));

            bool hasAccess = true;
            try
            {
                hasAccess = CheckOrCreateDirectoryUnsafe(path);
            }
            catch (System.Security.SecurityException) { }
            catch (Exception)
            {
                hasAccess = false;
            }
            if (!hasAccess)
                throw new System.Web.HttpException(string.Format(
                    // StringResources.FileUtils_ControlHasNoAccessToPath,
                    "FileUtils_Control:{0} {1} HasNoAccessToPath = {2}",
                    control.ID, control.GetType().Name, path));
        }

        static bool CheckOrCreateDirectoryUnsafe(string path)
        {
            string resolvedPath = path; // web:  UrlUtils.ResolvePhysicalPath(path);

            DirectoryInfo dir = new DirectoryInfo(resolvedPath);
            if (!dir.Exists)
                dir.Create();
            DirectorySecurity sec = dir.GetAccessControl(System.Security.AccessControl.AccessControlSections.Access);
            System.Security.AccessControl.AuthorizationRuleCollection rules = 
                sec.GetAccessRules(true, true, typeof(SecurityIdentifier));
            FileSystemRights rightsToCheck = FileSystemRights.Read | FileSystemRights.Write
                                             | FileSystemRights.Delete | FileSystemRights.ListDirectory;
            System.Security.Principal.WindowsIdentity user = WindowsIdentity.GetCurrent();
            foreach (FileSystemAccessRule rule in rules)
            {
                if ((rule.FileSystemRights & rightsToCheck) > 0)
                {
                    SecurityIdentifier sid = (SecurityIdentifier)rule.IdentityReference;
                    if ((sid.IsAccountSid() && user.User == sid) ||
                      (!sid.IsAccountSid() && user.Groups.Contains(sid)))
                    {
                        if (rule.AccessControlType == AccessControlType.Deny)
                            return false;
                    }
                }
            }
            return true;
        }
    }

}
