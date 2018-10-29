/* Copyright (c) 2018 Rebecca Ramnauth */

using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;

namespace xml_converter
{
    class Program
    {
        // DIRECTORIES 
        public static readonly String parent_folder = "test_files/";
        public static readonly String start_folder = parent_folder + "inflow_content/";
        public static readonly String end_folder = parent_folder + "regulation_content/";
        public static readonly String processed = parent_folder + "processed_content/";
        public static Boolean transferProcessed = false;

        // SQL DATA CLIENT
        public static String connectionString = @"Data Source=GIRLSWHOCODE;Initial Catalog=LexisExtract;Integrated Security=true";
        public static SqlConnection cnn = new SqlConnection(connectionString);
        public static String job_table = "dbo.Reg_Change_Job_Logs";
        public static String file_table = "dbo.Reg_Change_Extract_Tracker";
        public static String content_table = "dbo.Reg_Change_Content";
        
        // SUBSCRIPTION IDs
        public static readonly String CFR = "c528100b-a50e-4ffc-8c8b-f3ebbfe25e52";
        public static readonly String PUC = "64632b65-fbc2-47ab-bf6c-136322eec66a";
        public static readonly String NYR = "8a12c008-8fdd-45e6-ab31-2499837a542f";
        public static readonly String NSL = "2ca4e673-0bea-41a4-9908-cfad48dbff0a";
        public static readonly String NYL = "9a889fc7-5825-4062-b181-d432beb1b247";
        public static readonly String NCS = "e8331a8f-4d07-4166-a1e6-f3e2ed4aaaae";

        public static void Main(string[] args)
        {
            // read inflow directory
            ReadDir(start_folder, end_folder);

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

            // interpret / parse content data as regulations
            List<List<Regulation>> regs = new List<List<Regulation>>();
            regs.Add(c.ParseReg());
            regs.Add(p.ParseReg());
            regs.Add(s.ParseReg());

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
                        SqlComm.Parameters.AddWithValue("@content", reg.getMeta(5));
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

            // move processed content
            if (transferProcessed)
            {
                Directory.Move(start_folder, processed);
                Directory.CreateDirectory(start_folder);
            }
        }

        public static void ReadDir(String from, String to)
        {
            StreamWriter heads = null;
            
            foreach (String d in Directory.EnumerateDirectories(from))
            {
                string toDir = to + "/" + Path.GetFileNameWithoutExtension(d) + "/";
                Directory.CreateDirectory(toDir);
                Console.WriteLine(d);
                foreach (String file in Directory.EnumerateFiles(d))
                {
                    Console.WriteLine("Should be reading > " + file);
                    ReadFile(file, heads, toDir);
                }
            }
            try { heads.Close(); } catch (Exception e) { Console.WriteLine(e); }
        }

        private static void ReadFile(String file, StreamWriter heads, String to)
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
                destination = Path.GetFileNameWithoutExtension(file) + "-" + i.ToString("D2");
                Console.Write("Reading > " + source);
                int positionOfXML = contents[i].IndexOf("<?xml");
                Console.Write(" > " + destination + "\n");
                try
                {
                    header += destination + ".txt " + contents[i].Substring(0, positionOfXML);
                    description = contents[i].Substring(positionOfXML);
                }
                catch (Exception e) { Console.WriteLine(e); }
                
                dest = new StreamWriter(to + destination + ".xml");
                dest.WriteLine(description);
                dest.Close();
            }
            if (heads != null) { heads.Write(header); }
            try
            {
                dest.Close();
            }
            catch (Exception e) { Console.WriteLine(destination + " produced error: " + e); }
        }
    }
}