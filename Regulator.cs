/* Copyright (c) 2018 Rebecca Ramnauth */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace xml_converter
{
    class Regulator
    {
        private string dir;
        private string subscription;
        private List<string> format;
        private List<string> m_format;
        private List<string> mm_format;
        
        public Regulator(string d, string sub_id, List<string> meta, List<string> metameta, List<string> f)
        {
            dir = d + "/" + sub_id + "/";
            subscription = sub_id;
            format = f;
            m_format = meta;
            mm_format = metameta;
        }

        public List<Regulation> ParseReg()
        {
            List<Regulation> regs = new List<Regulation>();
            List<string> meta;
            List<List<string>> metameta;
            List<List<string>> keys = InterpretFormat(format);

            foreach (String file in Directory.EnumerateFiles(dir, "*-01.xml")) // each feed file
            {
                try
                {
                    meta = InterpretMeta(file);
                    metameta = InterpretFeed(file);
                    for (int i = 2; i < metameta.Count; i++)
                    {
                    
                            Guid g = Guid.NewGuid();
                            Regulation r = new Regulation(dir + meta[1] + "-" + i.ToString("D2") + ".xml", subscription, keys.Count, metameta[i][0]);
                            foreach (string mitem in meta)
                            {
                                r.addMeta(mitem);
                            }
                            foreach (string mmitem in metameta[i])
                            {
                                r.addMeta(mmitem);
                            }
                            for (int j = 0; j < keys.Count; j++)
                            {
                                r.setColumn(j, r.ParseByKey(keys[j]));
                            }
                            regs.Add(r);
                    
                    }
                }
                catch (FileNotFoundException f) { Console.WriteLine(f); }
            }
            return regs;
        }

        public List<string> InterpretMeta(String file)
        {
            List<string> meta = new List<string>();
            XDocument xdoc = XDocument.Load(file);
            for (int i = 0; i < m_format.Count - 1; i++)
            {
                meta.Add(xdoc.Descendants(m_format[i]).FirstOrDefault().Value);
            }
            return meta;
        }

        public List<List<string>> InterpretFeed(String file)
        {
            List<List<string>> metameta = new List<List<string>>();
            XDocument xdoc = XDocument.Load(file);
            IEnumerable<XElement> entries = xdoc.Descendants(m_format[m_format.Count-1]); // tag "entry"
            int count = 0;
            foreach (XElement entry in entries) {
                
                List<string> mentry = new List<string>();
                for (int i = 0; i < mm_format.Count; i++)
                {
                    mentry.Add(entries.Descendants(mm_format[i]).ElementAt(count).Value);
                }
                metameta.Add(mentry);
                count++;
            }
            return metameta;
        }

        public List<List<string>> InterpretFormat(List<string> format)
        {
            List<List<string>> subformat = new List<List<string>>();
            foreach (string key in format)
            {
                subformat.Add(Interpret(key));
            }
            return subformat;
        }

        private void PrintNestedList(List<List<string>> s)
        {
            foreach (List<string> st in s)
            {
                Console.WriteLine(st.Count + " > " + st.GetType());
                foreach (string str in st)
                {
                    Console.WriteLine("   " + str);
                }
            }
        }

        private void PrintList(List<string> s)
        {
            Console.WriteLine(s.Count + " > " + s.GetType());
            foreach (string str in s)
            {
                Console.WriteLine("   " + str);
            }
        }

        public List<string> Interpret(string key)
        {
            List<string> subkeys = new List<string>();
            if (key == null)
            {
                return subkeys;
            }
            else if (key.Contains("/") && key.Contains("=@")) // sub-element by attribute
            {
                subkeys.Add(key.Substring(0, key.IndexOf("["))); // get element name
                subkeys.Add(key.Substring(key.IndexOf("[") + 1, key.IndexOf("=@") - key.IndexOf("[") - 1)); // get attribute name
                subkeys.Add(key.Substring(key.IndexOf("=@") + 2, key.IndexOf("]") - key.IndexOf("=@") - 2)); // get attribute value
                subkeys.Add(key.Substring(key.IndexOf("/") + 1)); // get sub element
            }
            else if (key.Contains("=@")) // attribute based
            {
                subkeys.Add(key.Substring(0, key.IndexOf("["))); // get element name
                subkeys.Add(key.Substring(key.IndexOf("[") + 1, key.IndexOf("=@") - key.IndexOf("[") - 1)); // get attribute name
                subkeys.Add(key.Substring(key.IndexOf("=@") + 2, key.IndexOf("]") - key.IndexOf("=@") - 2)); // get attribute value
            }
            else if (key.Contains("["))
            {
                subkeys.Add(key.Substring(0, key.IndexOf("["))); // get element name
                subkeys.Add(key.Substring(key.IndexOf("[") + 1, key.IndexOf("]") - key.IndexOf("[") - 1));
            }
            else
            {
                subkeys.Add(key); // get element name
            }
            return subkeys;
        }

        private List<string> DefaultFormat()
        {
            return new List<string>(8);
        }
    }
}
