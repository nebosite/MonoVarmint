using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace MonoVarmint.Tools
{
    public class VarmintId
    {
        //--------------------------------------------------------------------------------------
        // A hack to implement unique device id's for tracking, encryption, etc.
        //--------------------------------------------------------------------------------------
        public static Guid GetLocalDeviceId()
        {
            try
            {
                var idFileName = "VarmintDeviceId.txt";
                var idText = LoadData(idFileName);
                if (idText == null)
                {
                    idText = Guid.NewGuid().ToString();
                    SaveData(idFileName, idText);
                }
                var id = Guid.Parse(idText);
                return id;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Error getting device id: " + e.Message);
                return Guid.NewGuid();
            }
        }

        //--------------------------------------------------------------------------------------
        // SaveData
        //--------------------------------------------------------------------------------------
        static void SaveData(string fileName, string output)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();

            // Open the file using the established file stream.
            if (storage.FileExists(fileName))
            {
                storage.DeleteFile(fileName);
            }
            using (var isolatedFileStream = storage.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                var textWriter = new StreamWriter(isolatedFileStream);
                textWriter.Write(output);
                textWriter.Flush();
                textWriter.Dispose();
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// LoadData
        /// </summary>
        //--------------------------------------------------------------------------------------
        static string LoadData(string fileName)
        {
            IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForDomain();

            if (!storage.FileExists(fileName))
            {
                return null;
            }

            // Open the file using the established file stream.
            using (var isolatedFileStream = storage.OpenFile(fileName, FileMode.Open, FileAccess.Read))
            {
                var textReader = new StreamReader(isolatedFileStream);
                return textReader.ReadToEnd();
            }
        }

    }
}
