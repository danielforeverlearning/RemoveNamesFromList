using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoveNamesFromList
{
    public class CHECKLIST_STRUCT
    {
        public string firstname;
        public string lastname;
        public string student; //yes or no
    }

    class Program
    {
        static bool DoMatch(ref List<string> list_firstname, ref List<string> list_lastname, string searchfirstname, string searchlastname)
        {
            for (int ii=0; ii < list_lastname.Count; ii++)
            {
                if (list_lastname[ii].Equals(searchlastname) || list_lastname[ii].Contains(searchlastname))
                {
                    if (list_firstname[ii].Equals(searchfirstname) || list_firstname[ii].StartsWith(searchfirstname))
                        return true;
                }
            }
            return false;
        }


        static List<string> GetFirstnamesWhereLastnameIsOrHasDash(ref List<string> list_firstname, ref List<string> list_lastname, string searchlastname)
        {
            List<string> results = new List<string>();

            for (int ii=0; ii < list_lastname.Count; ii++)
            {
                if (list_lastname[ii].Equals(searchlastname))
                    results.Add(list_firstname[ii]);
                else if (list_lastname[ii].StartsWith(searchlastname))
                {
                    //ok check if there is a dash after, for example: Hernandez-Castro
                    string tempstr = list_lastname[ii].Replace(searchlastname,"");
                    if (tempstr[0] == '-')
                        results.Add(list_firstname[ii]);

                }
            }

            return results;
        }

        static void Main(string[] args)
        {
            //checklist_csv - contains csv of rows with (firstname, lastname, student==yes or no) to check against roster_csv
            //roster_csv - contains csv of rows with (firstname, lastname) of staff
            //output_csv - the output file which is same as input.csv without rows where firstname and lastname were found in checklist.csv
            //adjunct_csv - every person name in adjunct_csv MUST BE inside output_csv

            string adjunct_csv = "./Summer 2018 - Adjuncts Added to PowerCampus.csv";
            int adjunct_firstname_index = 0;
            int adjunct_lastname_index = 1;

            string checklist_csv = "./Book2.csv";
            int checklist_firstname_index = 1;
            int checklist_lastname_index = 2;
            int checklist_9454_student_index = 28;
            int checklist_skip = 1;

            string roster_csv = "./Copy of Active Roster 5.7.18 for IT.csv";
            int roster_lastname_index = 0;
            int roster_firstname_index = 1;
            int roster_job_index = 2;
            int roster_skip = 2;

            string output_csv = "./2018_summer_alertmedia_final.csv";

            char space_delimiter = ' ';
            char comma_delimiter = ',';

            //ok let us read in the adjunct_csv 
            List<string> adjunct_firstname = new List<string>();
            List<string> adjunct_lastname  = new List<string>();
            StreamReader adjunct_rdr       = new StreamReader(adjunct_csv);
            string adjunct_line = adjunct_rdr.ReadLine();
            while (adjunct_line != null)
            {
                string[] substrings = adjunct_line.Split(comma_delimiter);

                string temp_firstname = substrings[adjunct_firstname_index].Replace("\"", "").Trim();
                string[] first_subs = temp_firstname.Split(space_delimiter); //sometimes they put space with middle initial or middle name , so chop it off
                string firstname = first_subs[0];

                string temp_lastname = substrings[adjunct_lastname_index].Replace("\"", "").Trim();
                string[] last_subs = temp_lastname.Split(space_delimiter);  //sometimes they put space with Jr or Sr, so chop it off
                string lastname;
                if (last_subs.Length == 1)
                    lastname = last_subs[0];
                else //there was a space
                {
                    if (last_subs.Length > 2) //for example "De La Cruz"
                    {
                        lastname = temp_lastname;
                    }
                    else
                    {
                        //ok only 1 space, check if after space is jr or sr
                        string teststr = last_subs[1].ToLower();
                        if (teststr.StartsWith("jr") || teststr.StartsWith("sr"))
                            lastname = last_subs[0];
                        else
                            lastname = string.Format("{0} {1}", last_subs[0], last_subs[1]);
                    }
                }

                adjunct_firstname.Add(firstname);
                adjunct_lastname.Add(lastname);

                adjunct_line = adjunct_rdr.ReadLine();
            }


            //Enter checklist_csv into a List<CHECKLIST_STRUCT>
            List<string> original_check = new List<string>();
            List<CHECKLIST_STRUCT> check_list = new List<CHECKLIST_STRUCT>();
            StreamReader check_rdr = new StreamReader(checklist_csv);
            StreamWriter out_writer = new StreamWriter(output_csv);

            //skip header in checklist
            for (int ii = 0; ii < checklist_skip; ii++)
            {
                string header_line = check_rdr.ReadLine();
                out_writer.WriteLine(header_line);
            }

            //Ok.....inside checklist_csv, we have to strip doublequotes from index 0 == last name and index 1 == first name and trim spaces
            string check_line = check_rdr.ReadLine();
            while (check_line != null)
            {
                original_check.Add(check_line);
                string[] substrings    = check_line.Split(comma_delimiter);

                string student         = substrings[checklist_9454_student_index].Trim(); //yes or no
                
                string temp_firstname  = substrings[checklist_firstname_index].Replace("\"", "").Trim();
                string firstname       = temp_firstname.Replace(".", ""); //sometimes firstname is just an initial with a period, so chop off the period

                string lastname        = substrings[checklist_lastname_index].Replace("\"", "").Trim();
                

                CHECKLIST_STRUCT item = new CHECKLIST_STRUCT();
                item.firstname        = firstname;
                item.lastname         = lastname;
                item.student          = student;

                check_list.Add(item);

                check_line = check_rdr.ReadLine();
            }

            //Enter roster_csv into 2 lists: (1) List<firstname> (2) List<lastname>
            List<string> roster_firstname = new List<string>();
            List<string> roster_lastname  = new List<string>();
            StreamReader roster_rdr = new StreamReader(roster_csv);

            //skip header in roster
            for (int ii=0; ii < roster_skip; ii++)
                roster_rdr.ReadLine();

            //only need to save non-students from roster
            string roster_line = roster_rdr.ReadLine();
            while (roster_line != null)
            {
                string[] substrings    = roster_line.Split(comma_delimiter);
                string job_str         = substrings[roster_job_index].Trim().ToLower();
                if (job_str.Equals("student") == false)
                {
                    string temp_firstname = substrings[roster_firstname_index].Replace("\"", "").Trim();
                    string[] first_subs   = temp_firstname.Split(space_delimiter); //sometimes in roster firstname they put space with middle initial or middle name , so chop it off
                    string firstname      = first_subs[0];

                    string temp_lastname  = substrings[roster_lastname_index].Replace("\"", "").Trim();
                    string[] last_subs    = temp_lastname.Split(space_delimiter); //sometimes in roster lastname they put space with Jr or Sr, so chop it off
                    string lastname;
                    if (last_subs.Length == 1)
                        lastname = last_subs[0];
                    else //there was a space
                    {
                        if (last_subs.Length > 2) //for example "De La Cruz"
                        {
                            lastname = temp_lastname;
                        }
                        else
                        {
                            //ok only 1 space, check if after space is jr or sr
                            string teststr = last_subs[1].ToLower();
                            if (teststr.StartsWith("jr") || teststr.StartsWith("sr"))
                                lastname = last_subs[0];
                            else
                                lastname = string.Format("{0} {1}", last_subs[0], last_subs[1]);
                        }
                    }

                    roster_firstname.Add(firstname);
                    roster_lastname.Add(lastname);
                }
                roster_line = roster_rdr.ReadLine();
            }

            //***************************************
            //Ok....now we do the checking logic
            //***************************************
            for (int xx=0; xx < check_list.Count; xx++)
            {
                CHECKLIST_STRUCT check_item = check_list[xx];
                if (check_item.student.Equals("yes"))
                {
                    Console.WriteLine("(1) STUDENT, GO TO OUTPUT - {0} {1}", check_item.firstname, check_item.lastname);
                    out_writer.WriteLine(original_check[xx]);
                }
                else //faculty or staff
                {
                    //Example: adjust list has firstname="Mercedes" lastname="Nelson Coffman", checklist says Mercedes Coffman, we need to match on Coffman
                    if (DoMatch(ref adjunct_firstname, ref adjunct_lastname, check_item.firstname, check_item.lastname))
                    {
                        Console.WriteLine("(2) ADJUNCT MATCH, GO TO OUTPUT - {0} {1}", check_item.firstname, check_item.lastname);
                        out_writer.WriteLine(original_check[xx]);
                    }
                    else if (DoMatch(ref roster_firstname, ref roster_lastname, check_item.firstname, check_item.lastname))
                    {
                        Console.WriteLine("(3) ROSTER MATCH, GO TO OUTPUT - {0} {1}", check_item.firstname, check_item.lastname);
                        out_writer.WriteLine(original_check[xx]);
                    }
                    else
                        Console.WriteLine("(4) DO NOT GO TO OUTPUT - {0} {1}", check_item.firstname, check_item.lastname);
                }
            }

            adjunct_rdr.Close();
            check_rdr.Close();
            roster_rdr.Close();
            out_writer.Close();
            
            Console.WriteLine("DONE");
        }
    }
}
