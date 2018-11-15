/* Copyright (c) 2018 Rebecca Ramnauth */

using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Specialized;

namespace xml_converter
{
    class Program
    {
        // DIRECTORIES 
        public static readonly String parent_folder = ConfigurationManager.AppSettings.Get("ContainerDir");
        public static readonly String start_folder = ConfigurationManager.AppSettings.Get("StartDir");
        public static readonly String end_folder = ConfigurationManager.AppSettings.Get("EndDir");
        public static readonly String processed = ConfigurationManager.AppSettings.Get("ProcessedDir");

        // CONTROL SWITCHES
        public static Boolean transferProcessed = AppSettings.Get<bool>("TransferProcessed");
        public static Boolean keepRegulationContent = AppSettings.Get<bool>("KeepXMLContent");

        // SQL DATA CLIENT
        public static String connectionString = ConfigurationManager.ConnectionStrings["ConnectionKey"].ConnectionString;
        public static SqlConnection cnn = new SqlConnection(connectionString);
        public static String job_table = ConfigurationManager.AppSettings.Get("JobLog");
        public static String file_table = ConfigurationManager.AppSettings.Get("FileLog");
        public static String content_table = ConfigurationManager.AppSettings.Get("ContentLog");

        // SUBSCRIPTION IDs
        public static readonly String CFR = ConfigurationManager.AppSettings.Get("CFR");
        public static readonly String PUC = ConfigurationManager.AppSettings.Get("PUC");
        public static readonly String NCS = ConfigurationManager.AppSettings.Get("NCS");
        public static readonly String NSL = ConfigurationManager.AppSettings.Get("NSL");
        public static readonly String NYCR = ConfigurationManager.AppSettings.Get("NYCR");
        public static readonly String NYSL = ConfigurationManager.AppSettings.Get("NYSL");

        public static void Main(string[] args)
        {
            Console.WriteLine(transferProcessed);
            // read inflow directory
            try {
                //ReadDir(start_folder + "/" + CFR, end_folder);
                //ReadDir(start_folder + "/" + PUC, end_folder);
                //ReadDir(start_folder + "/" + NCS, end_folder);
                //ReadDir(start_folder + "/" + NSL, end_folder);
                //ReadDir(start_folder + "/" + NYCR, end_folder);
                //ReadDir(start_folder + "/" + NYSL, end_folder);
            } catch (Exception e) {
                Console.WriteLine("Incorrect start_folder. Verify that subscription id exists.");
            }
            
            // configure nested table of contents [meta] data
            List<string> meta = new List<string> {
                "{http://wwww.w3.org/2005/Atom}title",                                      // 0 - Source / Regulation Body
                "{http://wwww.w3.org/2005/Atom}subtitle",                                   // 1 - File Name
                "{http://wwww.w3.org/2005/Atom}id",                                         // 2 - File Identifier
                "{http://wwww.w3.org/2005/Atom}updated",                                    // 3 - File Date Updated
                "{http://services.lexisnexis.com/interfaces/publish/lnpub/1/}publishType",  // 4 - File Publish Type
                "{http://wwww.w3.org/2005/Atom}entry"                                       // pointer
            }; 
            List<string> metameta = new List<string> {
                "{http://wwww.w3.org/2005/Atom}title",                                      // 5 - Content Identifier
                "{http://wwww.w3.org/2005/Atom}updated",                                    // 6 - Content Date Updated
                "{http://services.lexisnexis.com/interfaces/publish/lnpub/1/}action"        // 7 - Content Action
            };

            // configure content data
            Regulator c = new Regulator(end_folder, CFR, meta, metameta, new List<string> {
                "citations",                                                                // 0 - Content Citation
                "jurisSystem[normalizedLongName]",                                          // 1 - Content Jurisdiction
                "hierarchyLevel[levelType=@title]/heading",                                 // 2 - Content Title 1                  [Title]
                "hierarchyLevel[levelType=@subtitle]/heading",                              // 3 - Content Title 2                  [Subtitle]
                "hierarchyLevel[levelType=@chapter]/heading",                               // 4 - Content Topic 1                  [Chapter]
                "hierarchyLevel[levelType=@subchapter]/heading",                            // 5 - Content Topic 2                  [Subchapter]
                "hierarchyLevel[levelType=@part]/heading",                                  // 6 - Content Section 1                [Part]
                "hierarchyLevel[levelType=@subpart]/heading",                               // 7 - Content Section 2                [Subpart]
                "hierarchyLevel[levelType=@section]/heading",                               // 8 - Content Section Description      [Section]
                "hierarchyLevel[levelType=@subsection]/heading",                            // 9 - Content Subsection 1             [Subsection]
                "hierarchyLevel[levelType=@unclassified]/heading",                          //10 - Content Subsection 2             [Unclassified]
                "administrativeCode",                                                       //11 - Content Description              [Description]
                "historyItem",                                                              //12 - Content Reference Citation       [Reference Citations]
            });
            Regulator p = new Regulator(end_folder, PUC, meta, metameta, new List<string> {
                "citations",
                "jurisSystem[normalizedLongName]",
                "governmentBodyName[normalizedLongName]",
                null,
                "fullCaseName",
                "docketNumber",
                null,
                "decisionDate",
                null,
                null,
                null,
                "administrativeDocBody",
                null,
            });
            Regulator s = new Regulator(end_folder, NCS, meta, metameta, new List<string> {
                "citations",
                "jurisSystem[normalizedLongName]",
                "hierarchyLevel[levelType=@title]/heading",
                "hierarchyLevel[levelType=@subtitle]/heading",
                "hierarchyLevel[levelType=@chapter]/heading",
                "hierarchyLevel[levelType=@subchapter]/heading",
                "hierarchyLevel[levelType=@part]/heading",
                "hierarchyLevel[levelType=@subpart]/heading",
                "hierarchyLevel[levelType=@section]/heading",
                "hierarchyLevel[levelType=@subsection]/heading",
                "hierarchyLevel[levelType=@unclassified]/heading",
                "legislativeDocBody",
                null,
            });
            Regulator statutes = new Regulator(end_folder, NSL, meta, metameta, new List<string> {
                "citations",
                "jurisSystem[normalizedLongName]",
                "hierarchyLevel[levelType=@topic]/heading",
                null,
                "hierarchyLevel[levelType=@article]/heading",
                "hierarchyLevel[levelType=@title]/heading",
                null,
                null,
                null,
                null,
                null,
                "legislativeDocBody",
                "history",
            });
            Regulator city_regs = new Regulator(end_folder, NYCR, meta, metameta, new List<string> {
                "citations",
                "jurisSystem[normalizedLongName]",
                "hierarchyLevel[levelType=@topic]/heading",
                "hierarchyLevel[levelType=@rule]/heading",
                "hierarchyLevel[levelType=@title]/heading",
                null,
                "hierarchyLevel[levelType=@chapter]/heading",
                null,
                "hierarchyLevel[levelType=@section]/heading",
                null,
                null,
                "legislativeDocBody",
                null,
            });
            Regulator state_legis = new Regulator(end_folder, NYSL, meta, metameta, new List<string> {
                "citations",
                "jurisSystem[normalizedLongName]",
                "hierarchyLevel[levelType=@title]/heading",
                "hierarchyLevel[levelType=@subtitle]/heading",
                "hierarchyLevel[levelType=@chapter]/heading",
                "hierarchyLevel[levelType=@subchapter]/heading",
                "hierarchyLevel[levelType=@part]/heading",
                "hierarchyLevel[levelType=@subpart]/heading",
                "hierarchyLevel[levelType=@section]/heading",
                "hierarchyLevel[levelType=@subsection]/heading",
                "hierarchyLevel[levelType=@unclassified]/heading",
                "legislativeDocBody",
                null,
            });

            // interpret / parse content data as regulations
            List<List<Regulation>> regs = new List<List<Regulation>>();

            //regs.Add(c.ParseReg());
            //regs.Add(p.ParseReg());
            //regs.Add(s.ParseReg());
            //regs.Add(statutes.ParseReg());
            //regs.Add(city_regs.ParseReg());
            //regs.Add(state_legis.ParseReg());

            Directory.CreateDirectory(processed + "/" + CFR); // create the processed directory if it does not exist
            if (transferProcessed)
            {
                MoveDir(start_folder + "/" + CFR, processed + "/" + CFR); // move processed content
            }

            try { Directory.Delete(end_folder, !keepRegulationContent); } catch (Exception e) { }

            // write regulation attributes to db
            try
            {
                cnn.Open();
                Guid g = Guid.NewGuid();
                SqlCommand SqlComm = new SqlCommand("INSERT INTO " + job_table + "(Job_Id, Job_Type, Start_Time) VALUES(@guid, @task, @start)", cnn);
                SqlComm.Parameters.AddWithValue("@guid", g);
                SqlComm.Parameters.AddWithValue("@task", "Parsing XML");
                SqlComm.Parameters.AddWithValue("@start", DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"));
                try { SqlComm.ExecuteNonQuery(); } catch (Exception e) { Console.WriteLine(e); }
                //SqlComm.Dispose();

                foreach (List<Regulation> body in regs)
                {
                    foreach (Regulation reg in body)
                    {
                        Guid file_g = Guid.NewGuid();

                        SqlComm = new SqlCommand("INSERT INTO " + file_table + "(Tbl_id, Subscription_id, File_name, Start_Time) VALUES(@guid, @sub, @file, @start)", cnn);
                        SqlComm.Parameters.AddWithValue("@guid", file_g);
                        SqlComm.Parameters.AddWithValue("@sub", reg.getSubscription());
                        SqlComm.Parameters.AddWithValue("@file", reg.getMeta(1));
                        SqlComm.Parameters.AddWithValue("@start", DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"));
                        SqlComm.ExecuteNonQuery();

                        // general configurations -- string manipulation on params should be localized here
                        Console.WriteLine("Generating SQL Command.");
                        SqlComm = new SqlCommand("INSERT INTO " + content_table + "(Subscription_Id, File_Name, Content_Id, " +
                        "Publish_Type, Action, Updated, Jurisdiction, Citation, Regulation_Type, Body, Title1, " +
                        "Title2, Topic1, Topic2, Section1, Section2, Section_Description, SubSection1, SubSection2, Description, RefCitation, " +
                        "Date_Type, Actual_Date) VALUES(@sub, @file, @content, @pub, @action, @updated, @juris, " +
                        "@citation, @type, @source, @title_1, @title_2, @topic_1, @topic_2, @sec_1, @sec_2, @secdescrip," +
                        "@subsec1, @subsec2, @descrip, @refcite, @dateType, @actualDate)", cnn);
                        SqlComm.Parameters.AddWithValue("@sub", reg.getSubscription());
                        SqlComm.Parameters.AddWithValue("@file", reg.getMeta(1));
                        SqlComm.Parameters.AddWithValue("@content", reg.getMeta(5).Substring(reg.getMeta(5).IndexOf("urn:contentItem:") + 16));
                        SqlComm.Parameters.AddWithValue("@pub", reg.getMeta(4));
                        SqlComm.Parameters.AddWithValue("@action", reg.getMeta(7));
                        SqlComm.Parameters.AddWithValue("@updated", reg.getMeta(6));
                        SqlComm.Parameters.AddWithValue("@juris", reg.getColumn(1));
                        SqlComm.Parameters.AddWithValue("@citation", reg.getColumn(0));
                        SqlComm.Parameters.AddWithValue("@type", "");
                        SqlComm.Parameters.AddWithValue("@source", reg.getMeta(0));
                        SqlComm.Parameters.AddWithValue("@title_1", reg.getColumn(2));
                        SqlComm.Parameters.AddWithValue("@title_2", reg.getColumn(3));
                        SqlComm.Parameters.AddWithValue("@topic_1", reg.getColumn(4));
                        SqlComm.Parameters.AddWithValue("@topic_2", reg.getColumn(5));
                        SqlComm.Parameters.AddWithValue("@sec_1", reg.getColumn(6));
                        SqlComm.Parameters.AddWithValue("@sec_2", reg.getColumn(7));
                        SqlComm.Parameters.AddWithValue("@secdescrip", reg.getColumn(10));
                        SqlComm.Parameters.AddWithValue("@subsec1", reg.getColumn(8));
                        SqlComm.Parameters.AddWithValue("@subsec2", reg.getColumn(9));
                        SqlComm.Parameters.AddWithValue("@descrip", reg.getColumn(11));
                        SqlComm.Parameters.AddWithValue("@refcite", reg.getColumn(12));
                        SqlComm.Parameters.AddWithValue("@dateType", "");
                        SqlComm.Parameters.AddWithValue("@actualDate", "");

                        SqlComm.ExecuteNonQuery();

                        SqlComm = new SqlCommand("UPDATE " + file_table + " SET End_Time=@end, Content_Item=@content WHERE Tbl_id=@guid", cnn);
                        SqlComm.Parameters.AddWithValue("@guid", file_g);
                        SqlComm.Parameters.AddWithValue("@content", reg.getMeta(2));
                        SqlComm.Parameters.AddWithValue("@end", DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"));
                        SqlComm.ExecuteNonQuery();
                    }

                    SqlComm = new SqlCommand("UPDATE " + job_table + " SET End_Time=@end WHERE Job_Id=@guid", cnn);
                    SqlComm.Parameters.AddWithValue("@guid", g);
                    SqlComm.Parameters.AddWithValue("@end", DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"));
                    SqlComm.ExecuteNonQuery();
                }
                cnn.Close();
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        public static void MoveDir(String from, String to)
        {
            try
            {
                Directory.Move(from, to + "/" + DateTime.Now.ToString("yyyy-MM-dd h-mm-ss tt"));
                Directory.CreateDirectory(from);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (!Directory.Exists(Path.GetDirectoryName(from)))
                {
                    Console.WriteLine("From file path does not exist: " + from);
                }
                if (!Directory.Exists(Path.GetDirectoryName(to)))
                {
                    Console.WriteLine("To file path does not exist: " + to);
                }
            }
        }

        public static void ReadDir(String from, String to)
        {
            StreamWriter heads = null;
            Console.WriteLine("From identified as " + from);

            int fCount = Directory.GetFiles(from, "*", SearchOption.TopDirectoryOnly).Length;
            int dCount = Directory.GetDirectories(from, "*", SearchOption.TopDirectoryOnly).Length;
            Console.WriteLine("f=" + fCount + " d=" + dCount);

            if (dCount <= 0)
            {
                //Directory.CreateDirectory(to + "/" + from);
                string dirName = new DirectoryInfo(from).Name;
                string toDir = to + "/" + dirName;
                Directory.CreateDirectory(toDir);
                foreach (String file in Directory.EnumerateFiles(from))
                {
                    Console.WriteLine(file);
                    ReadFile(file, heads, toDir);
                }
            }
            else
            {
                foreach (String d in Directory.EnumerateDirectories(from))
                {
                    string toDir = to + "/" + Path.GetFileNameWithoutExtension(d) + "/";
                    Directory.CreateDirectory(toDir);
                    Console.WriteLine(toDir);
                    foreach (String file in Directory.EnumerateFiles(d))
                    {
                        //Console.WriteLine("Should be reading > " + file);
                        ReadFile(file, heads, toDir);
                    }
                }
            }
            try { heads.Close(); } catch (Exception e) { /*Console.WriteLine(e);*/ }
        }

        private static void ReadFile(String file, StreamWriter heads, String to)
        {
            String source = file;
            String content = File.ReadAllText(source);
            //String[] contents = content.Split("--yytet00pubSubBoundary00tetyy");
            String[] contents = content.Split(new[] { "--yytet00pubSubBoundary00tetyy" }, StringSplitOptions.None);
            String destination = "";
            StreamWriter dest = null;

            String header = "";
            String description = "";

            for (int i = 1; i < contents.Length; i++)
            {
                destination = Path.GetFileNameWithoutExtension(file) + "-" + i.ToString("D2");
                //Console.Write("Reading > " + source);
                int positionOfXML = contents[i].IndexOf("<?xml");
                //Console.Write(" > " + destination + "\n");
                try
                {
                    header += destination + ".txt " + contents[i].Substring(0, positionOfXML);
                    description = contents[i].Substring(positionOfXML);
                }
                catch (Exception e) { /*Console.WriteLine(e);*/ }

                //Console.WriteLine("TO: " + to);
                dest = new StreamWriter(to + "/" + destination + ".xml");
                dest.WriteLine(description);
                dest.Close();
            }
            if (heads != null) { heads.Write(header); }
            try
            {
                dest.Close();
            }
            catch (Exception e) { /*Console.WriteLine(destination + " produced error: " + e); */}
        }
    }
}