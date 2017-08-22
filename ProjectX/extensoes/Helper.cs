namespace ProjectX
{
    using Fougerite;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml.Serialization;

    public static class Helper
    {

        public static void Log(string logName, string msg)
        {
            File.AppendAllText(ProjectX.GetAbsoluteFilePath(logName), string.Format("[{0} {1}] {2}\r\n", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), msg));
        }

        public static T ObjectFromFile<T>(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            StreamReader reader = new StreamReader(stream);
            BinaryFormatter formatter = new BinaryFormatter();
          //  formatter.Binder = new MagmaToRustPPModuleDeserializationBinder();
            return (T)formatter.Deserialize(reader.BaseStream);
        }

        public static T ObjectFromXML<T>(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            TextReader textReader = new StreamReader(path);
            T local = (T)serializer.Deserialize(textReader);
            textReader.Close();
            return local;
        }

        public static void ObjectToFile<T>(T ht, string path)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Create);
                StreamWriter writer = new StreamWriter(stream);
                formatter.Serialize(writer.BaseStream, ht);
            }
            catch (Exception ex)
            {
                Logger.LogError("[Rust++] " + path + " seems to be corrupted. Stop the server and delete the file. Error: " + ex);
            }
        }

        public static void ObjectToXML<T>(T obj, string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                TextWriter textWriter = new StreamWriter(path);
                serializer.Serialize(textWriter, obj);
                textWriter.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("[Rust++] " + path + " seems to be corrupted. Stop the server and delete the file. Error: " + ex);
            }
        }
    }

}
