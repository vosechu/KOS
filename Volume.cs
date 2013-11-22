using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using kOS.Craft;

namespace kOS
{
    public class Volume
    {
        public int Capacity = -1;
        public String Name = "";
        protected List<File> files = new List<File>();

        public bool Renameable = true;

        public virtual File GetByName(String name)
        {
            return files.FirstOrDefault(p => p.Filename.ToUpper() == name.ToUpper());
        }

        public virtual void AppendToFile(string name, string str) 
        {
            var file = GetByName(name) ?? new File(name);

            file.Add(str);

            SaveFile(file);
        }

        public virtual void DeleteByName(String name)
        {
            foreach (var p in files.Where(p => p.Filename.ToUpper() == name.ToUpper()))
            {
                files.Remove(p);
                return;
            }
        }

        public virtual bool SaveFile(File file)
        {
            DeleteByName(file.Filename);
            files.Add(file);

            return true;
        }

        public virtual int GetFreeSpace() { return -1; }
        public virtual bool IsRoomFor(File newFile) { return true; }
        public virtual void LoadPrograms(List<File> programsToLoad) { }
        public virtual ConfigNode Save(string nodeName) { return new ConfigNode(nodeName); }

        public virtual List<FileInfo> GetFileList()
        {
            return files.Select(file => new FileInfo(file.Filename, file.GetSize())).ToList();
        }

        public virtual bool CheckRange()
        {
            return true;
        }
    }

    public class Archive : Volume
    {
        public string ArchiveFolder = GameDatabase.Instance.PluginDataFolder + "/Plugins/PluginData/Archive/";

        private readonly Vessel vessel;

        public Archive(Vessel vessel)
        {
            this.vessel = vessel;

            Renameable = false;
            Name = "Archive";

            LoadAll();
        }

        public override bool IsRoomFor(File newFile)
        {
            return true;
        }

        private void LoadAll()
        {
            Directory.CreateDirectory(ArchiveFolder);

            // Attempt to migrate files from old archive drive
            if (!KSP.IO.File.Exists<File>(HighLogic.fetch.GameSaveFolder + "/arc")) return;

            var reader = KSP.IO.BinaryReader.CreateForType<File>(HighLogic.fetch.GameSaveFolder + "/arc");

            var fileCount = reader.ReadInt32();

            for (var i = 0; i < fileCount; i++)
            {
                try
                {
                    var filename = reader.ReadString();
                    var body = reader.ReadString();

                    var file = new File(filename);
                    file.Deserialize(body);

                    files.Add(file);
                    SaveFile(file);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }

            reader.Close();

            KSP.IO.File.Delete<File>(HighLogic.fetch.GameSaveFolder + "/arc");
        }

        public override File GetByName(string name)
        {
            try
            {
                using (var infile = new StreamReader(ArchiveFolder + name + ".txt", true))
                {
                    var fileBody = infile.ReadToEnd().Replace("\r\n", "\n") ;

                    var retFile = new File(name);
                    retFile.Deserialize(fileBody);
                    
                    base.DeleteByName(name);
                    files.Add(retFile);

                    return retFile;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override bool SaveFile(File file)
        {
            base.SaveFile(file);

            if (!CheckRange())
            {
                throw new kOSException("Volume is out of range.");
            }

            Directory.CreateDirectory(ArchiveFolder);

            try
            {
                using (var outfile = new StreamWriter(ArchiveFolder + file.Filename + ".txt", false))
                {
                    var fileBody = file.Serialize();

                    if (Application.platform == RuntimePlatform.WindowsPlayer)
                    {
                        // Only evil windows gets evil windows line breaks
                        fileBody = fileBody.Replace("\n", "\r\n");
                    }

                    outfile.Write(fileBody);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override void DeleteByName(string name)
        {
            System.IO.File.Delete(ArchiveFolder + name + ".txt");
        }

        public override List<FileInfo> GetFileList()
        {
            var retList = new List<FileInfo>();

            try
            {
                retList.AddRange(Directory.GetFiles(ArchiveFolder, "*.txt")
                    .Select(file => new System.IO.FileInfo(file))
                    .Select(sysFileInfo => new FileInfo(sysFileInfo.Name.Substring(0, sysFileInfo.Name.Length - 4), (int) sysFileInfo.Length)));
            }
            catch (DirectoryNotFoundException)
            {
            }

            return retList;
        }

        public override bool CheckRange()
        {
            return (vessel.GetDistanceToKerbinSurface() < vessel.GetCommRange());
        }
    }
}
