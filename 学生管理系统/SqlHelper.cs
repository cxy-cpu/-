using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace 学生管理系统
{
    public class SqlHelper
    {
        //连接字符串
        public static readonly string connString = "server=.;database=StudentDB;uid=sa;pwd=123456;";//Sql server身份验证
                                                                                                    //标准写法        Data Source Initial Catalog   User ID Password
        public static object ExecuteScalar(string sql, params SqlParameter[] paras)
        {
            object o = null;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //创建Command对象
                SqlCommand cmd = new SqlCommand(sql, conn);
                // cmd.CommandType = CommandType.StoredProcedure;//存储过程
                cmd.Parameters.Clear();
                //cmd.Parameters.Add(paraUName);
                //cmd.Parameters.Add(paraUPwd);
                cmd.Parameters.AddRange(paras);
                //打开连接
                conn.Open();//最晚打开 最早关闭
                o = cmd.ExecuteScalar();  //执行查询，返回结果集第一行第一列的值，忽略其他行其他列
                cmd.Parameters.Clear();
                //关闭连接
                //conn.Close();
            }
            return o;
        }

        /// <summary>
        /// 执行查询，返回SqlDataReader
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static SqlDataReader ExecuteReader(string sql, params SqlParameter[] paras)
        {
            SqlConnection conn = new SqlConnection(connString);

            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(paras);
                SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return dr;
            }
            catch (SqlException ex)
            {
                conn.Close();
                throw new Exception("执行查询异常", ex);
            }

        }

        /// <summary>
        /// 返回DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sql, params SqlParameter[] paras)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //创建Command对象
                SqlCommand cmd = new SqlCommand(sql, conn);
                // cmd.CommandType = CommandType.StoredProcedure;//存储过程
                if (paras != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(paras);
                }

                //打开连接
                conn.Open();//这里打开conn也可以，如果da就不会去关闭（但是da可以自动完成这件事）
                            //断开式连接
                            //执行命令 一定是Command完成的
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                //SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                //打开conn Opened
                //数据填充
                da.Fill(dt);
                //关闭conn
                //关闭连接
                //conn.Close();
            }
            return dt;
        }

        /// <summary>
        /// 返回受影响的行数  insert update  delete
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paras"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, params SqlParameter[] paras)
        {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //创建Command对象
                SqlCommand cmd = new SqlCommand(sql, conn);
                // cmd.CommandType = CommandType.StoredProcedure;//存储过程
                cmd.Parameters.Clear();
                //cmd.Parameters.Add(paraUName);
                //cmd.Parameters.Add(paraUPwd);
                cmd.Parameters.AddRange(paras);
                //打开连接
                conn.Open();//最晚打开 最早关闭
                count = cmd.ExecuteNonQuery();  //执行T-SQL语句，返回受影响的行数(executenonquery 增删改通用）
                //关闭连接
                //conn.Close();
            }
            return count;
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="comList"></param>
        /// <returns></returns>
        public static bool ExecuteTrans(List<CommandInfo> comList)
        {
            using (SqlConnection conn = new SqlConnection(SqlHelper.connString))
            {
                //事务是通过conn连接对象来开启的，conn.open()
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                //SqlCommand 事务的执行 cmd
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Transaction = trans;

                try
                {
                    int count = 0;
                    for (int i=0; i<comList.Count;i++)
                    {
                        cmd.CommandText = comList[i].CommandText;
                        if (comList[i].IsProc)
                            cmd.CommandType = CommandType.StoredProcedure;
                        else
                            cmd.CommandType = CommandType.Text;
                        if(comList[i].Parameters.Length>0)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(comList[i].Parameters);
                        }
                        
                        count += cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                    trans.Commit();
                    return true;
                }
                catch (SqlException ex)
                {
                    trans.Rollback();
                    throw new Exception("执行事务出现异常", ex);
                }
            }
        }
    }
}
