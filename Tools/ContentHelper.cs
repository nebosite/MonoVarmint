using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MonoVarmint.Tools
{
    class ContentHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="nameHint">Resource has to end with the file name + file extension (e.g. fileName.txt). Case-sensitive.</param>
        /// <returns></returns>
        public static string GetEmbeddedResourceString(Assembly assembly, string nameHint)
        {
            string[] ResourceNames = assembly.GetManifestResourceNames();
            string SourceFile = "";

            foreach(string name in ResourceNames)
            {
                if (name.EndsWith(nameHint))
                    SourceFile = name;
            }

            if (SourceFile.Equals(""))
            {
                throw new FileNotFoundException("Could not find " + nameHint + " in ManifestResourceNames");
            }

            Stream stream = assembly.GetManifestResourceStream(SourceFile);
            StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
