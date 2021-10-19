using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisaSupport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Getting Connection ...");
            var conn = connectToDb();

            TableWithPrimaryKey model = new TableWithPrimaryKey();
            List<TableWithPrimaryKey> list = new List<TableWithPrimaryKey>();
            List<AllTable> allTables = new List<AllTable>();
            AllTable allTable = new AllTable();
            //tables without Primary KEY!
            List<AllTable> tablesNoPrimaryKey = new List<AllTable>();


            try
            {
                Console.WriteLine("Openning Connection ...");
                conn.Open();
                Console.WriteLine("Connection successful!");
                SqlDataAdapter adapter = new SqlDataAdapter("", conn);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                //getting all tables from connected database with a primary key!

                string sqlQuery = @"select 
                                        s.name as SchemaName,
                                        t.name as TableName,
                                        tc.name as ColumnName,
                                        ic.key_ordinal as KeyOrderNr
                                    from 
                                        sys.schemas s 
                                        inner join sys.tables t   on s.schema_id=t.schema_id
                                        inner join sys.indexes i  on t.object_id=i.object_id
                                        inner join sys.index_columns ic on i.object_id=ic.object_id 
                                                                       and i.index_id=ic.index_id
                                        inner join sys.columns tc on ic.object_id=tc.object_id 
                                                                 and ic.column_id=tc.column_id
                                    where i.is_primary_key=1 
                                    order by t.name, ic.key_ordinal ;";



                using (var command = new SqlCommand(sqlQuery, conn))

                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        model = new TableWithPrimaryKey();
                        model.TableName = reader.GetValue(1).ToString();
                        model.ColumnName = reader.GetValue(2).ToString();
                        list.Add(model);
                    }


                }


                //getting all tables from connected Database!
                string sqlQuery2 = @"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE';";

                using (var command = new SqlCommand(sqlQuery2, conn))

                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        allTable = new AllTable();
                        allTable.TableName = reader.GetValue(2).ToString();

                        allTables.Add(allTable);
                    }


                }



            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            //displaying results
            Display(list, allTables, tablesNoPrimaryKey);




            Console.WriteLine("Altering Primary Keys to Tables which has no Primary Key, If sure SAY: Yes");
            if (Console.ReadLine() == "Yes")  //do it!
            {
                Console.WriteLine("IfSuccess? : " + AddPMKeys(tablesNoPrimaryKey));
            }


         



            Console.Read();
        }

        public static SqlConnection connectToDb()
        {

            string dataSource = @"192.168.34.200";
            string database = @"SRSDB";
            string connString = @"Data source=" + dataSource + ";initial catalog=" + database + ";user id=sa;password=;multipleactiveresultsets=True;application name=EntityFramework&quot;";

            SqlConnection conn = new SqlConnection(connString);

            return conn; 
        }

        private static bool AddPMKeys(List<AllTable> tablesNoPrimaryKey)
        {

            var conn = connectToDb(); 

           
                Console.WriteLine("Openning Connection ...");
                conn.Open();
                Console.WriteLine("Connection successful!");
                SqlDataAdapter adapter = new SqlDataAdapter("", conn);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                //getting all tables from connected database with a primary key!

                string sqlQuery ="";


                foreach (var item in tablesNoPrimaryKey)
                {
                      

                    Console.WriteLine("updating Table:  " + item.TableName);

                   //  Alter Table StudentProgressStatusLog Add srsPK int Identity(1,1) 
                   //  ALTER TABLE StudentProgressStatusLog ADD CONSTRAINT srs_pk PRIMARY KEY(srsPK);


                    sqlQuery = @"ALTER TABLE " + item.TableName+ @" ADD srsPK int Identity(1,1)  "
                                + @" ALTER TABLE " + item.TableName + " ADD CONSTRAINT srs_pk_"+item.TableName+ " PRIMARY KEY(srsPK);";

                try
                {
                    using (var command = new SqlCommand(sqlQuery, conn))
                    {

                        var reader = command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                    continue; 
                }

            }

               

                return true;
            }
            

                    
        

        private static void Display(List<TableWithPrimaryKey> list, List<AllTable> allTables, List<AllTable> tablesNoPrimaryKey)
        {
            Console.WriteLine("Table Name                Column Name");
            int number = 1;
            foreach (var item in list)
            {
                Console.WriteLine(number++ + ":" + item.TableName + " - " + item.ColumnName);
            }
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("All Tables");

            number = 1;
            foreach (var item in allTables)
            {
                Console.WriteLine(number++ + ":" + item.TableName);
            }

           

            foreach (var table in allTables)
            {
                if (!list.Exists(x => x.TableName == table.TableName))
                {
                    tablesNoPrimaryKey.Add(table);
                }
            }


            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Tables Without Primary Key!");
            number = 1;
            foreach (var item in tablesNoPrimaryKey)
            {
                Console.WriteLine(number++ + "  " + item.TableName);
            }
        }

        public class TableWithPrimaryKey
        {       
            public string TableName { get; set; }
            public string ColumnName { get; set; }
        }

        public class AllTable {    
            public string TableName { get; set; }
        }


        public class VisaSupport
        {
            public string Email { get; set; }
            public string UserName { get; set; }
            public DateTime? DateOfIssue { get; set; }
            public DateTime? DateOfExpiration { get; set; }
            public DateTime? DateOfRegistration { get; set; }
            public int DayCount { get; set; }
        }
    }
}
