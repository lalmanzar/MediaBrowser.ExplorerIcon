using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MediaBrowser.ExplorerIcon.Entities
{
    public class FolderIcon
    {
        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        private static extern UInt32 SHGetSetFolderCustomSettings(ref Lpshfoldercustomsettings pfcs, string pszPath, UInt32 dwReadWrite);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct Lpshfoldercustomsettings
        {
            public readonly UInt32 dwSize;
            public UInt32 dwMask;
            public readonly IntPtr pvid;
            public readonly string pszWebViewTemplate;
            public readonly UInt32 cchWebViewTemplate;
            public readonly string pszWebViewTemplateVersion;
            public readonly string pszInfoTip;
            public readonly UInt32 cchInfoTip;
            public readonly IntPtr pclsid;
            public readonly UInt32 dwFlags;
            public string pszIconFile;
            public readonly UInt32 cchIconFile;
            public readonly int iIconIndex;
            public readonly string pszLogo;
            public readonly UInt32 cchLogo;
        }

        private static void SetFolderIcon(string icoFile, string folderPath)
        {
            try
            {
                var folderSettings = new Lpshfoldercustomsettings {dwMask = 0x10, pszIconFile = icoFile};

                const uint fcsForcewrite = 0x00000002;

                var pszPath = folderPath;
                SHGetSetFolderCustomSettings(ref folderSettings, pszPath, fcsForcewrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void CreateIconedFolder
            (string folderName, string iconFile, int iconIndex = 0, string toolTip = "", bool preventSharing = false,
                bool confirmDelete = true)
        {
            #region Private Variables

            var fileName = "desktop.ini";

            #endregion Private Variables

            #region Data Validation

            if (Directory.Exists(folderName) == false)
            {
                return;
            }

            #endregion Data Validation

            try
            {
                var folder = new DirectoryInfo(folderName);
                fileName = Path.Combine(folderName, fileName);

                // Create the file
                using (var sw = new StreamWriter(fileName))
                {
                    sw.WriteLine("[.ShellClassInfo]");
                    sw.WriteLine("ConfirmFileOp={0}", confirmDelete);
                    sw.WriteLine("NoSharing={0}", preventSharing);
                    sw.WriteLine("IconFile={0}", iconFile);
                    sw.WriteLine("IconIndex={0}", iconIndex);
                    sw.WriteLine("IconResource={0},{1}", iconFile, iconIndex);
                    sw.WriteLine("InfoTip={0}", toolTip);
                    sw.Close();
                }

                // Update the folder attributes
                folder.Attributes = folder.Attributes | FileAttributes.System;

                // "Hide" the desktop.ini
                File.SetAttributes(fileName, File.GetAttributes(fileName) | FileAttributes.Hidden);
                SetFolderIcon(iconFile, folderName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}