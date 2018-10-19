/* Copyright (c) 2018 Rebecca Ramnauth */

using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace xml_converter
{
    class Program
    {
        // DIRECTORIES 
        public static readonly String parent_folder = "test_files/";
        public static readonly String dir = parent_folder + "/inflow_content/";
        public static readonly String meta = parent_folder + "/meta_data/";
        public static readonly String generated = parent_folder + "/regulation_content/";
        public static readonly String content = parent_folder + "/content_log/";
        public static readonly String processed = parent_folder + "/processed_content/";

        // CONTROL SWITCHES
        public static Boolean getMeta = false;
        public static Boolean getContentLog = false;
        public static Boolean transferProcessed = true;

        // LANG. INTERPRETATION -- temporarily discontinued
        public static ExcelPackage t = new ExcelPackage(new FileInfo("test_files/meta.xlsx")); // lang interpreter environment
        public static readonly ExcelWorksheet wt = t.Workbook.Worksheets["template"]; // lang intepreter home spreadsheet stating XML<>XSLX structure
        public static int offset = 4; // for language interpreter outputting

        // INTER LOG
        public static ExcelPackage j = new ExcelPackage();
        public static ExcelWorksheet iu_job = j.Workbook.Worksheets.Add("Job Log");
        public static FileInfo log = new FileInfo("test_files/job_log.xlsx");
        public static int job_id = 1;

        // INTRA LOG
        public static ExcelPackage p = new ExcelPackage();
        public static ExcelWorksheet iu_file = p.Workbook.Worksheets.Add("File Log");
        public static ExcelWorksheet ws = p.Workbook.Worksheets.Add("Content");

        // SUBSCRIPTION IDs -- add ids and intel as neccessary
        public static readonly String CFR = "c528100b-a50e-4ffc-8c8b-f3ebbfe25e52"; // other sub_ids allowed for further intel
        public static readonly String PUC = "64632b65-fbc2-47ab-bf6c-136322eec66a";

        public static void Main(string[] args)
        {
            //try { job_id = iu_job.Dimension.End.Row + 1; } catch (Exception e) {} // appending to existing excel table
            createJobLog();

            Write(Guid.NewGuid().ToString(), 1, job_id + 1, 0, j);
            Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 3, job_id + 1, 0, j);
            Write("Reading Content Directory", 2, job_id + 1, 0, j);

            ReadDir();
            Directory.CreateDirectory(content);

            Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 4, job_id + 1, 0, j); job_id++;

            Write(Guid.NewGuid().ToString(), 1, job_id + 1, 0, j);
            Write("Structuring XML Content", 2, job_id + 1, 0, j);
            Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 3, job_id + 1, 0, j);

            StreamWriter s = null;
            foreach (String file in Directory.EnumerateFiles(generated, "*.xml"))
            {
                try
                {
                    s = new StreamWriter(content + Path.GetFileNameWithoutExtension(file) + ".txt");
                    XmlDocument d = new XmlDocument();
                    d.Load(file);
                    TraverseNodes(d.ChildNodes, s);
                }
                catch (Exception e) { /*Console.WriteLine(e);*/ }
                s.Close();
            }

            Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 4, job_id + 1, 0, j); job_id++;

            //NarrowHeaders(); // uncomment for narrow table format
            //NarrowContents();

            Write(Guid.NewGuid().ToString(), 1, job_id + 1, 0, j);
            Write("Parsing XML Content", 2, job_id + 1, 0, j);
            Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 3, job_id + 1, 0, j);

            SpecificHeaders();
            SpecificContents();

            Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 4, job_id + 1, 0, j); job_id++;

            if (transferProcessed)
            {
                Write(Guid.NewGuid().ToString(), 1, job_id + 1, 0, j);
                Write("Transferring Processed Content", 2, job_id + 1, 0, j);
                Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 3, job_id + 1, 0, j);

                Directory.Move(dir, processed);
                Directory.CreateDirectory(dir);

                Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 4, job_id + 1, 0, j); job_id++;
            }

            j.SaveAs(log);
        }

        public static void createJobLog()
        {
            iu_job.Row(1).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            iu_job.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MidnightBlue);
            iu_job.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
            iu_job.Row(1).Style.Font.Bold = true;
            iu_job.Row(1).Style.Font.Name = "Consolas";
            iu_job.Row(1).Style.Font.Size = 10;

            iu_job.Cells.Style.Font.Name = "Consolas";
            iu_job.Cells.Style.Font.Size = 10;

            iu_job.Cells[1, 1].Value = "Job ID";
            iu_job.Cells[1, 2].Value = "Job Type";
            iu_job.Cells[1, 3].Value = "Start Time";
            iu_job.Cells[1, 4].Value = "End Time";
        }

        public static void SpecificHeaders() // make specific: source > topic > section > subsection
        {

            // for an XLSX format interpreter, uncomment
            /*
             var wt = t.Workbook.Worksheets["template"]; 
             for (int i = wt.Dimension.Start.Column;
                        i <= wt.Dimension.End.Column - offset;
                        i++)
             {
                ws.Cells[1, i].Value = wt.Cells[1, i + offset].Value;
                //Console.WriteLine(ws.Cells[1, i].Value);
            } */

            iu_file.Row(1).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            iu_file.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MidnightBlue);
            iu_file.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
            iu_file.Row(1).Style.Font.Bold = true;
            iu_file.Row(1).Style.Font.Name = "Consolas";
            iu_file.Row(1).Style.Font.Size = 10;

            iu_file.Cells.Style.Font.Name = "Consolas";
            iu_file.Cells.Style.Font.Size = 10;

            iu_file.Cells[1, 1].Value = "Tbl_ID";
            iu_file.Cells[1, 2].Value = "Subscription ID";
            iu_file.Cells[1, 3].Value = "File Name";
            iu_file.Cells[1, 4].Value = "Content ID";
            iu_file.Cells[1, 5].Value = "Start Time";
            iu_file.Cells[1, 6].Value = "End Time";

            ws.Row(1).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MidnightBlue);
            ws.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Row(1).Style.Font.Bold = true;
            ws.Row(1).Style.Font.Name = "Consolas";
            ws.Row(1).Style.Font.Size = 10;

            ws.Cells.Style.Font.Name = "Consolas";
            ws.Cells.Style.Font.Size = 10;

            ws.Cells[1, 1].Value = "#"; // scanner line number      // -
            ws.Cells[1, 2].Value = "Subscription ID";               // -
            ws.Cells[1, 3].Value = "File Name";                     // +
            ws.Cells[1, 4].Value = "Content ID";                    // >
            ws.Cells[1, 5].Value = "Date Last Updated";             // -
            ws.Cells[1, 6].Value = "Date Name";                     // -
            ws.Cells[1, 7].Value = "Date Value";                    // -
            ws.Cells[1, 8].Value = "Citation";                      // -
            ws.Cells[1, 9].Value = "Type";                          // -
            ws.Cells[1, 10].Value = "Reference";                    // -
            ws.Cells[1, 11].Value = "Publish Type";                 // -
            ws.Cells[1, 12].Value = "Action";                       // +
            ws.Cells[1, 13].Value = "Jurisdiction";                 // +
            ws.Cells[1, 14].Value = "Source";                       // +
            ws.Cells[1, 15].Value = "Title 1";                      // +
            ws.Cells[1, 16].Value = "Title 2";                      // +
            ws.Cells[1, 17].Value = "Topic 1";                      // +
            ws.Cells[1, 18].Value = "Topic 2";                      // +
            ws.Cells[1, 19].Value = "Section 1";                    // +
            ws.Cells[1, 20].Value = "Section 2";                    // +
            ws.Cells[1, 21].Value = "Subsection";                   // +
            ws.Cells[1, 22].Value = "Description";                  // -
        }
        
        private static int MatchSource(String file)
        {
            for (int j = wt.Dimension.Start.Row;
                        j <= wt.Dimension.End.Row;
                        j++)
            {
                if (file.Contains(wt.Cells[j, 2].Value.ToString()))
                {
                    return j;
                }
            }
            return -1;
        }

        private static int UntilIllegalLetter(char[] c)
        {
            int i = 0;
            while ((c[i] >= 65 && c[i] <= 90) || (c[i] >= 97 && c[i] <= 122))
            {
                i++;
            }
            return i;
        }

        private static bool isEmpty(string str)
        {
            string s = Regex.Replace(str, @"[^a-zA-Z0-9]", "").Trim();
            char[] sc = s.ToCharArray();
            Console.WriteLine("String = " + str + " has " + sc.Length + " chars");
            if (sc.Length > 0)
            {
                return true;
            }
            return false;
        }

        // XLSX <> XML language interpreter - skeleton
        /*private static void getRule(int row, int col)
        {
            List<string> anatomy = new List<string>();

            string rule = wt.Cells[row, col + offset].Value.ToString();
            List<string> parts = rule.Split('-').ToList(); // denotes concatenation separated by hyphen

            int partition;
            string rule_head, rule_tail;
            List<string> parameters;

            for (int i = 0; i < parts.Count; i++)
            {
                partition = UntilIllegalLetter(parts[i].ToCharArray());
                rule_head = parts[i].Substring(0, partition); // UntilIllegalLetter(wt.Cells[row, col + offset].Value.ToString().ToCharArray());
                rule_tail = Regex.Replace(parts[i].Substring(partition), @"[^a-zA-Z0-9,]", ""); // .Replace("[()]", "");
                parameters = rule_tail.Split(',').ToList();
                Console.WriteLine("Head = " + rule_head + "; Tail = " + rule_tail + "; Parameters# = " + parameters.Count);

                if(!isEmpty(rule_head) && isEmpty(rule_tail))
                {   // get node content by head value only
                    
                }

            }
            
            //Console.WriteLine("Head = " + rule_head + "; Tail = " + rule_tail);
        }*/

        /*// requires accessibility modifiers
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            foreach (string needle in needles)
            {
                if (haystack.Contains(needle))
                    return true;
            }

            return false;
        } */

        public static bool isNullOrEmpty(string s)
        {
            return (s == null || s == String.Empty) ? true : false;
        }

        private static List<List<List<string>>> getKeywords(int row)
        {
            List<List<List<string>>> keywords = new List<List<List<string>>>();
            for (int k = 0; k < wt.Dimension.Start.Column - offset; k++) // k corresponds to column in the content file
            {
                string rule = wt.Cells[row, k + offset].Value.ToString();
                List<string> parts = rule.Split('-').ToList(); // denotes concatenation by hyphen

                int partition;
                string rule_head, rule_tail;
                //List<string> parameters;

                for (int i = 0; i < parts.Count; i++)
                {
                    partition = UntilIllegalLetter(parts[i].ToCharArray());
                    rule_head = parts[i].Substring(0, partition); // UntilIllegalLetter(wt.Cells[row, col + offset].Value.ToString().ToCharArray());
                    rule_tail = Regex.Replace(parts[i].Substring(partition), @"[^a-zA-Z0-9,]", ""); // .Replace("[()]", "");
                    //parameters = rule_tail.Split(',').ToList();
                    //Console.WriteLine("Head = " + rule_head + "; Tail = " + rule_tail + "; Parameters# = " + parameters.Count);

                    List<string> rule_body = new List<string> { rule_head, rule_tail };
                    keywords[k][i] = rule_body;
                    Console.WriteLine("Rule Body = " + rule_body.ElementAt(0) + ", " + rule_body.ElementAt(1));
                }
            }
            printKeys(keywords);
            return keywords;
            //Console.WriteLine("Head = " + rule_head + "; Tail = " + rule_tail);
        }

        private static void printKeys(List<List<List<string>>> key)
        {
            for (int i = 0; i < key.Count; i++)
            {
                for (int j = 0; j < key[i].Count; j++)
                {
                    for (int k = 0; k < key[i][j].Count; k++)
                    {
                        Console.WriteLine("key[" + i + "][" + j + "][" + k + "] = " + key[i][j][k]);
                    }
                }
            }
        }

        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string NormalizeWhiteSpace(string input)
        {
            int len = input.Length,
                index = 0,
                i = 0;
            var src = input.ToCharArray();
            bool skip = false;
            char ch;
            for (; i < len; i++)
            {
                ch = src[i];
                switch (ch)
                {
                    case '\u0020':
                    case '\u00A0':
                    case '\u1680':
                    case '\u2000':
                    case '\u2001':
                    case '\u2002':
                    case '\u2003':
                    case '\u2004':
                    case '\u2005':
                    case '\u2006':
                    case '\u2007':
                    case '\u2008':
                    case '\u2009':
                    case '\u200A':
                    case '\u202F':
                    case '\u205F':
                    case '\u3000':
                    case '\u2028':
                    case '\u2029':
                    case '\u0009':
                    case '\u000A':
                    case '\u000B':
                    case '\u000C':
                    case '\u000D':
                    case '\u0085':
                        if (skip) continue;
                        src[index++] = ch;
                        skip = true;
                        continue;
                    default:
                        skip = false;
                        src[index++] = ch;
                        continue;
                }
            }
            return new string(src);
        }

        private static void SpecificContents()
        {
            // LOCATION
            int sheet = 1; // content worksheet index

            // TIMERS
            Stopwatch fileWatch = new Stopwatch();
            Stopwatch jobWatch = new Stopwatch();

            // TRACKING & RESETTING
            String line;
            int row = 2;

            String value = "";
            String reg_body = "";
            String sub_id = "";
            String file_name = "";
            String content_id = "";
            String dc_id = ""; // content id from dc tag, uncomment call below
            String cite_normalized = "";
            String cite_type = "";
            String cite_id = ""; // citation id from dc tag, uncomment call below

            StreamReader r;
            int start = 0;
            Boolean f = false; // tracking start of new file
            int cid = 0;
            String p_value = "";

            // CITATION ELEMENTS
            String desig = "";
            String desig_section = "";
            String desig_lvl1 = "";
            String desig_lvl2 = "";
            String desig_lvl3 = "";
            String desig_lvl4 = "";
            String desig_lvl5 = "";
            String desig_lvl6 = "";
            char[] romans = { 'i', 'v', 'x' }; // special characters for subsection desigs

            // BODY ELEMENTS
            string title_1 = "";
            string title_2 = "";
            string topic_1 = "";
            string topic_2 = ""; 
            string section_1 = ""; 
            string section_2 = ""; 
            string subsection_1 = ""; 
            string source = "";
            string publishType = "";
            string action = "";
            string last_updated = "";
            string jurisdiction = "";

            // HELPERS
            bool switchTitle = false;
            string[] val_options = { "p", "fullCaseName", "docketNumber", "dateText", "page", "adjudicators", "emphasis", "span", "keyValue" };
            int status = 0;
            bool prop = false; // uncomment call below for subsection-subsection based propagation

            string dateType = "";
            string dateValue = "";
            
            foreach (String file in Directory.EnumerateFiles(content, "*.txt"))
            {
                // document in file log, start of new file
                Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 5, row, sheet - 1, p);
                Write(Guid.NewGuid().ToString(), 1, row, sheet - 1, p);

                // int matchingSource = MatchSource(Path.GetFileNameWithoutExtension(file)); // uncomment for XML<>XLSX lang interpreter
                // if (matchingSource > -1) { List<List<List<string>>> keys = getKeywords(matchingSource); } // read corresponding template rule, and ?apply the rule to the data pull

                r = new StreamReader(file);
                Scanner sc = new Scanner(r);

                // document in file log, corresponding subscription and file information
                sub_id = Path.GetFileNameWithoutExtension(file).Substring(0, GetNthIndex(Path.GetFileNameWithoutExtension(file), '-', Path.GetFileNameWithoutExtension(file).Count(x => x == '-') - 2));
                file_name = Path.GetFileNameWithoutExtension(file).Substring(GetNthIndex(Path.GetFileNameWithoutExtension(file), '-', Path.GetFileNameWithoutExtension(file).Count(x => x == '-') - 2) + 1);

                while ((line = sc.ReadLine()) != null)
                {
                    value = ""; // reset each line
                    prop = false;
                    try
                    {
                        String peekLine = sc.PeekLine(); // scan ahead by adding lines to a queue
                        if (line.Substring(0, line.IndexOf(":")).Equals("feed")) // values to reset at the occurence of each feed portion
                        {
                            publishType = "";
                            action = "";
                                
                        }
                        else if (line.Substring(0, line.IndexOf(":")).Equals("xml")) // values to reset at the occurence of each content portion
                        {
                            dc_id = "";
                            f = false;
                            jurisdiction = ""; 
                            content_id = "";
                        }
                        else
                        {
                            if (line.Substring(0, line.IndexOf(":")).Equals("administrativeDoc") || line.Substring(0, line.IndexOf(":")).Equals("legislativeDoc")) // values to reset at the occurence of each content child
                            {
                                f = true; // write only content, not feed data
                                source = "";
                                title_1 = "";
                                title_2 = "";
                                topic_1 = "";
                                topic_2 = "";
                                section_1 = "";
                                section_2 = "";
                                subsection_1 = "";
                                dateType = "";
                                dateValue = "";
                            }

                            else if (line.Substring(0, line.IndexOf(":")).Equals("citeForThisResource"))
                            {
                                reg_body = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                start = row;
                            }
                            else if (line.Contains("lnpub"))
                            {
                                if (line.Contains(":action"))
                                {
                                    action = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                }
                                else if (line.Contains(":publishType"))
                                {
                                    publishType = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                }
                            }
                            else if (line.Substring(0, line.IndexOf(":")).Equals("updated"))
                            {
                                last_updated = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            }
                            else if (line.Contains("jurisSystem"))
                            {
                                string jurisdiction_attribute = GetNthAttribute(line, 1);
                                jurisdiction = jurisdiction_attribute.Substring(jurisdiction_attribute.IndexOf("=") + 1).Trim();
                            }
                            else if (line.Contains("publicationName"))
                            {
                                source = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            }
                            // DETERMINE SUBSECTION DESIG
                            else if (line.Substring(0, line.IndexOf(":")).Equals("desig"))
                            {
                                String val = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                if (val.Contains("§"))
                                {
                                    desig = "";
                                    desig_section = val;
                                    desig_lvl1 = "";
                                    desig_lvl2 = "";
                                    desig_lvl3 = "";
                                    desig_lvl4 = "";
                                    desig_lvl5 = "";
                                    desig_lvl6 = "";
                                }
                                else if (int.TryParse(val.Substring(0, val.Length - 1), out int n))
                                {
                                    desig_lvl1 = val;
                                    desig_lvl2 = "";
                                    desig_lvl3 = "";
                                    desig_lvl4 = "";
                                    desig_lvl5 = "";
                                    desig_lvl6 = "";
                                }
                                else if (val.Substring(0, val.Length - 1).All(c => Char.IsLetter(c)))
                                {
                                    desig_lvl2 = val;
                                    desig_lvl3 = "";
                                    desig_lvl4 = "";
                                    desig_lvl5 = "";
                                    desig_lvl6 = "";
                                }
                                else if (val.Contains("("))
                                {
                                    String temp_val = Regex.Replace(val, "[^A-Za-z0-9]", "");
                                    bool b = OnlyContains(temp_val, romans);

                                    if (int.TryParse(temp_val, out int p)) // Example: (1)
                                    {
                                        desig_lvl3 = val;
                                        desig_lvl4 = "";
                                        desig_lvl5 = "";
                                        desig_lvl6 = "";
                                    }
                                    else if (!b && temp_val.All(c => Char.IsLetter(c))) // Examples: (A) or (a)
                                    {
                                        desig_lvl4 = val;
                                        desig_lvl5 = "";
                                        desig_lvl6 = "";
                                    }
                                    else if (b) // Examples: (i) or (iv) or (x)
                                    {
                                        desig_lvl5 = val;
                                        desig_lvl6 = "";
                                    }
                                    else if (temp_val.All(c => Char.IsLetterOrDigit(c))) // Examples: (a-1) or (b-4)
                                    {
                                        desig_lvl6 = val;
                                    }
                                    else
                                    {   // beyond known desig boundaries
                                        Console.WriteLine("Unexpected desig: " + val + "; consider revising citation");
                                    }
                                }
                                desig = desig_section + desig_lvl1 + desig_lvl2 + desig_lvl3 + desig_lvl4 + desig_lvl5 + desig_lvl6;
                                if (reg_body.Contains("§") && f) // recheck for other content types
                                {
                                    reg_body = reg_body.Substring(0, reg_body.IndexOf(" §")) + " " + desig;
                                }

                                // DETERMINE HIERARCHY LEVEL
                                if (val.Substring(0, val.IndexOf(" ")).Equals("TITLE"))
                                {
                                    title_1 = val;
                                    switchTitle = true;
                                    status = 1;
                                }
                                else if (val.Substring(0, val.IndexOf(" ")).Equals("SUBTITLE"))
                                {
                                    title_2 = val;
                                    switchTitle = true;
                                    status = 2;
                                }
                                else if (val.Substring(0, val.IndexOf(" ")).Equals("CHAPTER"))
                                {
                                    topic_1 = val.Substring(val.IndexOf(":") + 1).Trim();
                                    switchTitle = true;
                                    status = 3;
                                }
                                else if (val.Contains("SUBCHAPTER"))
                                {
                                    topic_2 = val.Substring(val.IndexOf(":") + 1).Trim();
                                    switchTitle = true;
                                    status = 4;
                                }
                                else if (val.Substring(0, val.IndexOf(" ")).Equals("PART"))
                                {
                                    section_1 = val.Substring(val.IndexOf(":") + 1).Trim();
                                    switchTitle = true;
                                    status = 5;
                                }
                                else if (val.Substring(0, val.IndexOf(" ")).Equals("SUBPART"))
                                {
                                    section_2 = val.Substring(val.IndexOf(":") + 1).Trim();
                                    switchTitle = true;
                                    status = 6;
                                }
                                else if (!isNullOrEmpty(desig) && !val.Contains("[")) // a citation without attributes
                                {
                                    subsection_1 = val.Substring(val.IndexOf(":") + 1).Trim();
                                    switchTitle = true;
                                    status = 7;
                                }
                                else
                                {
                                    status = 0;
                                    switchTitle = false;
                                }
                            }

                            // DETERMINE HIERARCHY LEVEL FULL TITLE
                            else if (line.Substring(0, line.IndexOf(":")).Equals("title") && switchTitle)
                            {
                                switch (status)
                                {
                                    case 1:
                                        title_1 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    case 2:
                                        title_2 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    case 3:
                                        topic_1 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    case 4:
                                        topic_2 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    case 5:
                                        section_1 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    case 6:
                                        section_2 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    case 7:
                                        subsection_1 += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                        break;
                                    default:
                                        Console.WriteLine("STATUS OUT OF BOUNDS:  " + status); // throw an error status code, corresponding to level
                                        break;
                                }
                                switchTitle = false;
                            }

                            // CASE/OPINION-BASED REGULATORY BODIES
                            else if (file.Contains(PUC) && line.Substring(0, line.IndexOf(":")).Equals("docketNumber"))
                            {
                                topic_2 = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            }
                            else if (file.Contains(PUC) && line.Substring(0, line.IndexOf(":")).Equals("governmentBodyName"))
                            {
                                title_1 = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            }
                            else if (file.Contains(PUC) &&line.Substring(0, line.IndexOf(":")).Equals("fullCaseName"))
                            {
                                topic_1 = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            }
                            
                            // ADDRESSING (DATE, CITATION) INFORMATION FOR A REGULATION
                            else if (line.Contains("dc:date:"))
                            {
                                var attr_date = GetNthAttribute(line, 1);
                                dateType = attr_date.Substring(attr_date.IndexOf("=") + 1).Trim();
                                dateValue = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            }
                            else if (line.Substring(0, line.IndexOf(":")).Equals("citation"))
                            {
                                cite_normalized = GetNthAttribute(line, 3).Substring(GetNthAttribute(line, 3).IndexOf("= ") + 1).Trim();
                                cite_type = GetNthAttribute(line, 1).Substring(GetNthAttribute(line, 1).IndexOf("= ") + 1).Trim();
                            }
                            /*if (line.Substring(0, line.IndexOf(":")).Equals("content") && line.Substring(line.IndexOf(":")).Length > 5)
                            {
                                int pos1 = line.IndexOf("src = cid:") + 10;
                                int pos2 = line.Substring(pos1).IndexOf(" ]");
                                content_id = line.Substring(pos1, pos2).Trim();
                                //f = true;
                            }*/
                        }
                        
                        // GENERAL CASE FOR WRITING CONTENT LINES
                        if (val_options.Contains(line.Substring(0, line.IndexOf(":")).Trim()) && (!peekLine.Contains(']') || !peekLine.Contains('[')))
                        {
                            p_value += " " + peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                            //p_value += peekLine.Substring(line.IndexOf(":") + 1).Trim();
                        }
                        if (line.Contains("urn:contentItem:"))
                        {
                        content_id = line.Substring(line.IndexOf("urn:contentItem:") + 16).Trim(); 
                        }
                        if (line.Contains(" [ "))
                        {
                            value = line.Substring(line.IndexOf(":") + 1).Trim();
                        }
                        if (peekLine.Substring(0, peekLine.IndexOf(":")).Contains("#text"))
                        {
                            value = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                        }
                        if (isNullOrEmpty(cite_type))
                        {
                            cite_type = "obligation";
                        }
                        cid++;
                    }
                    catch (Exception e) { /*Console.WriteLine(e);*/ };
                }

                if (!isNullOrEmpty(p_value.Trim()))
                {
                    // ADDRESSING INFORMATION
                    Write(cid.ToString(), 1, row, sheet, p);    // content log line #
                    Write(sub_id, 2, row, sheet, p);            // subscription id
                    Write(file_name, 3, row, sheet, p);         // original file name
                    Write(content_id, 4, row, sheet, p);        // content id for content file
                    Write(content_id, 4, row, sheet - 1, p);    // content id for file log

                    // OBLIGATIONS DATES
                    Write(last_updated, 5, row, sheet, p);      // last updated date
                    Write(dateType, 6, row, sheet, p);          // date type
                    Write(dateValue, 7, row, sheet, p);         // date value

                    // CITATION INFORMATION
                    Write(reg_body, 8, row, sheet, p);          // parent node/citation
                    Write(cite_type, 9, row, sheet, p);         // obligation type
                    Write(cite_normalized, 10, row, sheet, p);  // reference node/citation

                    // FEED-BASED METADATA
                    Write(publishType, 11, row, sheet, p);      // feed's publishType
                    Write(action, 12, row, sheet, p);           // feed's action
                    Write(jurisdiction, 13, row, sheet, p);     // feed's jurisdiction
                    Write(source, 14, row, sheet, p);           // source name

                    // FOOTER-BASED INFORMATION
                    Write(title_1, 15, row, sheet, p);          // title
                    Write(title_2, 16, row, sheet, p);          // subtitle
                    Write(topic_1, 17, row, sheet, p);          // chapter
                    Write(topic_2, 18, row, sheet, p);          // subchapter
                    Write(section_1, 19, row, sheet, p);        // part
                    Write(section_2, 20, row, sheet, p);        // subpart
                    Write(subsection_1, 21, row, sheet, p);     // section
                                                                // subsection + design overwritten

                    //Write(Regex.Replace(value, @"\s+", " "), 22, row);    // raw node text/attribute(s)

                    // OBLIGATION CONTENT
                    Write(NormalizeWhiteSpace(p_value).Trim(), 22, row, sheet, p);

                    Write(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"), 6, row, sheet - 1, p);    // PARSING END
                    Write(sub_id, 2, row, sheet - 1, p);        // subscription id for file log
                    Write(file_name, 3, row, sheet - 1, p);     // file name for file log

                    row++;                                      // row tracking for both content and log
                    p_value = "";                               // reset obligation content
                }
                r.Close(); // close streamwriter for particular file
                p.SaveAs(new FileInfo("test_files/specific-test-" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx")); //** file name contingent on current date-time, not recommended
            }
            //job_id++; // parsing stage completed
            if (!getContentLog) { DeleteContentLogs(); }        // delete content logs
        }

        private static void DeleteContentLogs()
        {
            Directory.Delete(content, true);
        }
        
        public static void NarrowHeaders()
        {
            ws.Row(1).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            ws.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MidnightBlue);
            ws.Row(1).Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Row(1).Style.Font.Bold = true;
            ws.Row(1).Style.Font.Name = "Calibri Light";
            ws.Row(1).Style.Font.Size = 10;

            ws.Cells.Style.Font.Name = "Calibri Light";
            ws.Cells.Style.Font.Size = 10;

            ws.Cells[1, 1].Value = "#";
            ws.Cells[1, 2].Value = "Source Subtitle";
            ws.Cells[1, 3].Value = "Content ID";
            ws.Cells[1, 4].Value = "Date Last Updated";
            ws.Cells[1, 5].Value = "Parent Node";
            ws.Cells[1, 6].Value = "Reference ID";
            ws.Cells[1, 7].Value = "Reference Type";
            ws.Cells[1, 8].Value = "Reference Citation";
            //ws.Cells[1, 8].Value = "Desig"; // append to parent node formatted citation
            ws.Cells[1, 9].Value = "Node Name";
            ws.Cells[1, 10].Value = "Node Value";
        }

        private static void NarrowContents()
        {
            int sheet = 2;
            String line;
            int row = 2;

            String value = "";
            String reg_body = "";
            String content_id = "";
            String dc_id = "";
            String cite_normalized = "";
            String cite_type = "";
            String cite_id = "";

            StreamReader r;

            int start = 0;
            Boolean prop;
            Boolean f = false;
            int cid = 0;

            String desig = "";
            String desig_section = "";
            String desig_lvl1 = "";
            String desig_lvl2 = "";
            String desig_lvl3 = "";
            String desig_lvl4 = "";
            String desig_lvl5 = "";
            String desig_lvl6 = "";
            char[] romans = { 'i', 'v', 'x' };

            foreach (String file in Directory.EnumerateFiles(content, "*.txt"))
            {
                r = new StreamReader(file);
                Scanner sc = new Scanner(r);

                while ((line = sc.ReadLine()) != null)
                {
                    value = "";
                    prop = false;

                    try
                    {
                        String peekLine = sc.PeekLine();
                        if (line.Substring(0, line.IndexOf(":")).Equals("xml"))
                        {
                            dc_id = "";
                            f = false;
                        }
                        else
                        {
                            if (line.Substring(0, line.IndexOf(":")).Equals("citeForThisResource"))
                            {
                                reg_body = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                start = row;
                                f = true;
                            }
                            if (line.Substring(0, line.IndexOf(":")).Equals("desig") )
                            {
                                String val = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                                if (val.Contains("§"))
                                {
                                    desig = "";
                                    desig_section = val;
                                    desig_lvl1 = "";
                                    desig_lvl2 = "";
                                    desig_lvl3 = "";
                                    desig_lvl4 = "";
                                    desig_lvl5 = "";
                                    desig_lvl6 = "";
                                }
                                else if (int.TryParse(val.Substring(0, val.Length-1), out int n)) //does not contain paren
                                {
                                    desig_lvl1 = val;
                                    desig_lvl2 = "";
                                    desig_lvl3 = "";
                                    desig_lvl4 = "";
                                    desig_lvl5 = "";
                                    desig_lvl6 = "";
                                }
                                else if (val.Substring(0, val.Length-1).All(c => Char.IsLetter(c)))
                                {
                                    desig_lvl2 = val;
                                    desig_lvl3 = "";
                                    desig_lvl4 = "";
                                    desig_lvl5 = "";
                                    desig_lvl6 = "";
                                }
                                else if (val.Contains("("))
                                {
                                    String temp_val = Regex.Replace(val, "[^A-Za-z0-9]", "");
                                    bool b = OnlyContains(temp_val, romans);

                                    if (int.TryParse(temp_val, out int p))
                                    {
                                        desig_lvl3 = val;
                                        desig_lvl4 = "";
                                        desig_lvl5 = "";
                                        desig_lvl6 = "";
                                    }
                                    else if (!b && temp_val.All(c => Char.IsLetter(c)))
                                    {
                                        desig_lvl4 = val;
                                        desig_lvl5 = "";
                                        desig_lvl6 = "";
                                    }
                                    else if (b)
                                    {
                                        desig_lvl5 = val;
                                        desig_lvl6 = "";
                                    }
                                    else if (temp_val.All(c => Char.IsLetterOrDigit(c)))
                                    {
                                        desig_lvl6 = val;
                                    }
                                }

                                desig = desig_section + desig_lvl1 + desig_lvl2 + desig_lvl3 + desig_lvl4 + desig_lvl5 + desig_lvl6;
                                if(reg_body.Contains("§") && f)
                                {
                                    reg_body = reg_body.Substring(0, reg_body.IndexOf(" §")) + " " + desig;
                                }
                            }
                            if (line.Substring(0, line.IndexOf(":")).Equals("citation"))
                            {
                                cite_normalized = GetNthAttribute(line, 3).Substring(GetNthAttribute(line, 3).IndexOf("= ") + 1).Trim();
                                cite_type = GetNthAttribute(line, 1).Substring(GetNthAttribute(line, 1).IndexOf("= ") + 1).Trim();
                                cite_id = GetNthAttribute(line, 2).Substring(GetNthAttribute(line, 2).IndexOf("= ") + 1).Trim();
                            }
                            if (line.Substring(0, line.IndexOf(":")).Equals("content") && line.Substring(line.IndexOf(":")).Length > 5)
                            {
                                int pos1 = line.IndexOf("src = cid:") + 10;
                                int pos2 = line.Substring(pos1).IndexOf(" ]");
                                content_id = line.Substring(pos1, pos2).Trim();
                                f = true;
                            }
                        }
                        if (line.Contains("dc:identifier:[ identifierScheme = LNI ]") || line.Contains("dc:date:"))
                        {
                            dc_id += peekLine.Substring(peekLine.IndexOf(":") + 1).Trim() + "; ";
                        }
                        if (line.Contains(" [ "))
                        {
                            value = line.Substring(line.IndexOf(":") + 1).Trim();
                        }
                        if (peekLine.Substring(0, peekLine.IndexOf(":")).Contains("#text"))
                        {
                            value = peekLine.Substring(peekLine.IndexOf(":") + 1).Trim();
                        }
                        cid++;
                        if (!value.Equals("") && !value.Equals(null))
                        {
                            Write(cid.ToString(), 1, row, sheet, p); //row #
                            Write(Path.GetFileNameWithoutExtension(file), 2, row, sheet, p); // source file
                            if (f)
                            {
                                int count = 0;
                                foreach (char c in dc_id)
                                {
                                    if (c == ';') count++;
                                }
                                if (count >= 2)
                                {
                                    //Write(dc_id, 3, row);   //content id
                                    Write(dc_id.Substring(0, dc_id.IndexOf(";") - 1).Trim(), 3, row, sheet, p);   //content id
                                    Write(dc_id.Substring(dc_id.IndexOf(";") + 2).Replace(";", "").Trim(), 4, row, sheet, p);   //content id
                                    if (!prop)
                                    {
                                        //prop = Propagate(dc_id, 3, row, start);
                                        prop = Propagate(dc_id.Substring(0, dc_id.IndexOf(";") - 1).Trim(), 3, row, start, sheet, p);
                                        prop = Propagate(dc_id.Substring(dc_id.IndexOf(";") + 2).Replace(";", "").Trim(), 4, row, start, sheet, p);
                                    }
                                }
                                
                                Write(reg_body, 5, row, sheet, p); // parent node
                                Write(cite_id, 6, row, sheet, p); //subcontent id
                                Write(cite_type, 7, row, sheet, p); //subcontent type
                                Write(cite_normalized, 8, row, sheet, p); //subcontent normalized
                            }
                            //Write(desig, 8, row); // desig
                            Write(line.Substring(0, line.IndexOf(":")), 9, row, sheet, p); // node name
                            Write(Regex.Replace(value, @"\s+", " "), 10, row, sheet, p); // node text/attribute(s)

                            row++;
                        }
                    }
                    catch (Exception e) { /*Console.WriteLine(e);*/ }
                }
                r.Close();
                var date = DateTime.Now.ToString("yyyy-MM-dd");
                p.SaveAs(new FileInfo("test_files/narrow_output " + date + ".xlsx"));
            }
        }

        private static Boolean OnlyContains(String s, char[] numbers)
        {
            int count = 0;
            foreach (char c in s)
            {
               foreach (var n in numbers)
                {
                    if (c == n)
                    {
                        count++;
                    }
                }
            }
            bool b = (count == s.Length && !s.Trim().Equals(""));
            return b;
        }

        private static void Write(String value, int col, int row, int sheet, ExcelPackage wb)
        {
            wb.Workbook.Worksheets[sheet].Cells[row, col].Value = value;
        }

        private static String GetNthAttribute(String value, int num)
        {
            int start = GetNthIndex(value, '[', num) + 1;
            int end = GetNthIndex(value, ']', num) - 1;
            return value.Substring(start, end - start);
        }

        public static int GetNthIndex(String s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private static Boolean Propagate(String value, int column, int current, int until, int sheet, ExcelPackage wb)
        {
            if (current > until) //upwards
            {
                while (current >= until)
                {
                    Write(value, column, current, sheet, wb);
                    current--;
                }
            }
            else
            {
                while (current <= until)
                {
                    Write(value, column, current, sheet, wb);
                    current++;
                }
            }
            return true;
        }

        public static void TraverseNodes(XmlNodeList nodes, StreamWriter s)
        {
            foreach (XmlNode node in nodes)
            {
                s.Write((node.Name + ": " + node.Value).Trim());

                XmlAttributeCollection attributes = node.Attributes;
                try
                {
                    String values = "";
                    foreach (XmlAttribute attribute in attributes)
                        values += "[ " + attribute.Name + " = " + attribute.Value + " ] ";
                    s.Write(values);
                }
                catch (Exception e) { /*Console.WriteLine(e);*/ } //tag has no attributes
                s.Write("\n");
                TraverseNodes(node.ChildNodes, s);
            }
        }

        public static void ReadDir()
        {
            Directory.CreateDirectory(generated);
            StreamWriter heads = null;
            if (getMeta) { Directory.CreateDirectory(meta); heads = new StreamWriter(meta + "meta.txt"); }
            foreach (String file in Directory.EnumerateFiles(dir, "*.txt", SearchOption.AllDirectories)) // reads all folders in the given directory
                ReadFile(file, heads);
            try { heads.Close(); } catch (Exception e) { /*Console.WriteLine(e);*/ }
        }

        private static void ReadFile(String file, StreamWriter heads)
        {
            String source = file;
            String content = File.ReadAllText(source);
            String[] contents = content.Split("--yytet00pubSubBoundary00tetyy");
            String destination = "";
            StreamWriter dest = null;

            String header = "";
            String description = "";

            for (int i = 1; i < contents.Length; i++)
            {
                destination = Path.GetDirectoryName(file).Split('\\').LastOrDefault() + "-" + Path.GetFileNameWithoutExtension(file) + "-" + i.ToString("D2");
                Console.Write("Reading > " + source);
                int positionOfXML = contents[i].IndexOf("<?xml");
                Console.Write(" > " + destination + "\n");
                try
                {
                    header += destination + ".txt " + contents[i].Substring(0, positionOfXML);
                    description = contents[i].Substring(positionOfXML);
                }
                catch (Exception e) { /*Console.WriteLine(e);*/ }

                dest = new StreamWriter(generated + destination + ".xml");
                dest.WriteLine(description);
                dest.Close();
            }
            if (heads != null) { heads.Write(header); }
            try
            {
                dest.Close();
            }
            catch(Exception e) { /*Console.WriteLine(destination + " produced error " + e);*/  }
        }
    }
}
