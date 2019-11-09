using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using sotsedit;

namespace s2edit
{
    class Processor
    {
        static public void process(Arguments arguments)
		{
			List<string> errors = new List<string>();
			string[] files = getListOfFiles(arguments.files);

            Modifier modifier = null;
            if(arguments.operation == Arguments.Operation.Change)
            {
                try{
                modifier = new Modifier(arguments.details);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            foreach(string file in files)
            {
                XmlDocument doc;
                try
                {
                    doc = loadXML(file);
                }
                catch(Exception e)
                {
                    errors.Add(e.Message);
                    continue;
                }

                if(arguments.operation == Arguments.Operation.Insert)
                {
                    insertNode(ref doc, arguments.key, arguments.details);
                    continue;
                }
                
                XmlNodeList nodes;
                try
                {
                    nodes = doc.SelectNodes(arguments.key);
                }
                catch(Exception e)
                {
                    errors.Add(file + ": " + e.Message);
                    continue;
                }

                if(nodes.Count == 0)
                {
                    errors.Add(file + ": no matching XML nodes found. Nothing to do.");
                    continue;
                }

                switch(arguments.operation)
                {
                    case Arguments.Operation.Change:
                        foreach(XmlNode node in nodes)
                        {
                            string value = node.InnerText;
                            modifier.apply(ref value);
                            node.InnerText = value;
                        }
                        break;
                    case Arguments.Operation.Delete:
                        foreach(XmlNode node in nodes)
                            node.ParentNode.RemoveChild(node);
                        break;
                    default:
                        throw new System.Exception("invalid operation value");
                }

                //done. Write output.
                using(MemoryStream stream = new MemoryStream())
                {
                    doc.Save(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                    using(FileStream fileStream = File.Create(file))
                        fileStream.Write(data, 0, data.Length);
                }
                Console.WriteLine(file);
            }

            if(errors.Count != 0)
            {
                Console.WriteLine("Errors!");
                foreach(string error in errors)
                    Console.WriteLine(error);
            }
        }

        static void insertNode(ref XmlDocument doc, String path, String value)
        {
            throw new System.Exception("insert not yet implemented.");
        }

        static string[] getListOfFiles(String files)
        {
            if(files.Contains('\\'))
            {
                int index = files.LastIndexOf('\\');
                String path = files.Substring(0, index);
                String name = files.Substring(index+1);
                return Directory.GetFiles(path, name);
            }
            else
            {
                return Directory.GetFiles(".", files);
            }
        }

        static XmlDocument loadXML(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            try{
                xml.Load(fileName);
                return xml;
            }
            catch(Exception e)
            {
                throw new Exception("Failed to load XML file:" + fileName + "\n\n" + e.Message);
            }
        }
    }
}
