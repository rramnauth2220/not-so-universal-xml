/* Copyright (c) 2018 Rebecca Ramnauth */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace xml_converter
{
    class Regulation
    {
        private string subscription, file, contentID;
        private List<string> columns;
        private List<string> meta = new List<string>();

        public Regulation(string f, string s, int n)
        {
            file = f;
            subscription = s;
            columns = Enumerable.Repeat("", n).ToList();
        }
        public Regulation(string f, string s, int n, string id)
        {
            file = f;
            subscription = s;
            contentID = id;
            columns = Enumerable.Repeat("", n).ToList();
        }

        public void addMeta(string content)
        {
            meta.Add(content);
        }

        public string getMeta(int idx)
        {
            return meta[idx];
        }

        public string getColumn(int idx)
        {
            return columns[idx];
        }

        public void setColumn(int idx, string content)
        {
            columns[idx] = content;
        }

        public string getSubscription()
        {
            return subscription;
        }

        public string getFileName()
        {
            return file;
        }

        public string getContentId()
        {
            return contentID;
        }

        public void PrintMeta()
        {
            foreach (string m in meta)
            {
                Console.WriteLine(m);
            }
        }

        public string ParseLogs()
        {
            Console.WriteLine("Parsing: " + file);
            string content = "";
            using (XmlReader reader = XmlReader.Create(file))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            content += "Start ELEMENT " + reader.Name + " of DEPTH " + reader.Depth;
                            if (reader.HasAttributes)
                            {
                                content += " with ATTRIBUTES ";
                                while (reader.MoveToNextAttribute())
                                {
                                    content += "___" + reader.Name + ">>>" + reader.Value;
                                }
                                content += "\n";
                                // Move the reader back to the element node.
                                reader.MoveToElement();
                            }
                            else
                            {
                                content += "\n";
                            }
                            break;
                        case XmlNodeType.Text:
                            content += "TEXT Node of DEPTH " + reader.Depth + ">>>" + reader.Value + "\n";
                            break;
                        case XmlNodeType.EndElement:
                            content += "End ELEMENT " + reader.Name + " of DEPTH " + reader.Depth + "\n";
                            break;
                        default:
                            content += "OTHER " + reader.NodeType + " of DEPTH " + reader.Depth + ">>>" + reader.Value + "\n";
                            break;
                    }
                }
            }
            Console.WriteLine(content);
            return content;
        }

        public string ParseByKey(List<string> keys)
        {
            int type = keys.Count;
            XDocument xdoc = XDocument.Load(file);
            IEnumerable<XElement> els;
            string val = "";
            switch (type)
            {
                case 1:
                    els = xdoc.Descendants().Where(p => p.Name == keys[0]);
                    break;
                case 2:
                    els = new List<XElement>();
                    foreach (XElement el in xdoc.Descendants(keys[0]))
                        val = (string)el.Attribute(keys[1]) ?? val;
                    break;
                case 3:
                    els = xdoc.Descendants().Where(p => p.Name == keys[0] && p.Attribute(keys[1]).Value == keys[2]);
                    break;
                case 4:
                    els = xdoc.Descendants().Where(p => p.Name == keys[0] && p.Attribute(keys[1]).Value == keys[2]).Elements(keys[3]);
                    break;
                default:
                    els = new List<XElement>();
                    break;
            }
            foreach (XElement el in els)
            {
                val += el.Value;
            }
            return val;
        }

        private string FormatText(IEnumerable<XElement> e)
        {
            string val = "";
            foreach (XElement el in e)
            {
                string indent = new string(' ', 2 * (e.AncestorsAndSelf().Count() - 1));
                val = indent + el.Value + " ";
            }
            return val;
        }
        
        public void PrintColumns()
        {
            foreach (string content in columns)
            {
                if (!string.IsNullOrEmpty(content) && content.Length > 15)
                {
                    Console.WriteLine(content.Substring(0, 15));
                }
                else
                    Console.WriteLine(content);
            }
        }
    }
}
