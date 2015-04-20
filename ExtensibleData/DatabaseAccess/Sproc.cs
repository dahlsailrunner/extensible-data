using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using CoreInfrastructure.Logging;

namespace DatabaseAccess
{
    public class Sproc
    {
        public SqlCommand Command { get; private set; }        

        public Sproc(string storedProcName, SqlConnection db)
        {
            Command = new SqlCommand(storedProcName, db)
            {
                CommandType = CommandType.StoredProcedure
            };            
        }

        public void SetParam(string columnNm, object value)
        {
            Command.Parameters.Add(new SqlParameter(columnNm, value ?? Convert.DBNull));
        }

        public string InputsString
        {
            get
            {
                var inString = new StringBuilder();
                foreach (SqlParameter param in Command.Parameters)
                {
                    inString.Append(string.Format("{0}=", param.ParameterName));
                    var dt = param.Value as DataTable;
                    if (dt == null)
                        inString.Append(string.Format("{0}|", param.Value));
                    else
                    {
                        // assert -- must be a data table
                        var cols = (from DataColumn col in dt.Columns select col.ColumnName).ToList();

                        var ind = 0;
                        var rowDtls = new StringBuilder();
                        foreach (DataRow row in dt.Rows)
                        {
                            rowDtls.Clear();
                            rowDtls.Append(string.Format(@"[{0}]::", ind++));
                            foreach (var col in cols)
                            {
                                rowDtls.Append(string.Format("{0}:{1},", col, row[col]));
                            }
                            inString.Append(rowDtls);
                        }
                    }
                }

                return inString.Length > 2000 ? inString.ToString().Substring(0, 2000) : inString.ToString();
            }
        }

        public void Execute(out DataSet resultSet, out int affectedRows)
        {
            resultSet = new DataSet();
            affectedRows = 0;

            try
            {
                using (var sqlDataAdapter = new SqlDataAdapter(Command))
                {
                    affectedRows = sqlDataAdapter.Fill(resultSet);                    
                }
            }
            catch (SqlException sqlErr)
            {
                throw new StoredProcException("SQL Exception occurred!!", Command.CommandText, InputsString, sqlErr);
            }
            catch (Exception ex) // some other kind of exception occurred!!
            {
                throw new StoredProcException("General Exception occurred!!", Command.CommandText, InputsString, ex);
            }
        }

        public void Execute(out DataTable resultSet)
        {
            DataSet ds;
            int affectedRows;

            Execute(out ds, out affectedRows);
            resultSet = ds.Tables[0];
            ds.Dispose();            
        }

        public void Execute<T>(out List<T> outList)
        {
            DataTable dt;
            Execute(out dt);
            outList = CollectionHelper.BuildCollection<T>(typeof (T), dt);
        }

        public int ExecNonQuery()
        {
            try
            {
                return Command.ExecuteNonQuery();
            }
            catch (SqlException sqlErr)
            {
                throw new StoredProcException("SQL Exception occurred!!", Command.CommandText, InputsString, sqlErr);
            }
            catch (Exception ex) // some other kind of exception occurred!!
            {
                throw new StoredProcException("General Exception occurred!!", Command.CommandText, InputsString, ex);
            }
        }
    }
}
