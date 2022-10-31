using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 学生管理系统
{
    public partial class FrmStudentList : Form
    {
        public FrmStudentList()
        {
            InitializeComponent();
        }
        //内置委托 Action<int>：不带返回值 可以不带参数，带参数最多16个
        //Func<int> Func<string ,string>：带一个返回值  可以不带参数，带参数最多16个
        private Action reLoad = null;


        //单例  只有一个实例
        private static FrmStudentList frmStudentList = null;
        public static FrmStudentList CreateInstance()
        {
            if(frmStudentList==null||frmStudentList.IsDisposed)
               frmStudentList = new FrmStudentList();
            return frmStudentList;
        }

        /// <summary>
        /// 加载班级列表、加载所有的学生信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmStudentList_Load(object sender, EventArgs e)
        {
            LoadClasses();//加载班级列表
            LoadAllStudentList(); //加载所有学生信息
        }

        private void LoadAllStudentList()
        {
            string sql = "select StuId,StuName,ClassName,GradeName,Sex,Phone from StudentInfo s inner join ClassInfo c on c.ClassID = s.ClassID" +
                " inner join GradeInfo g on g.GradeID = c.GradeID " ;
            //"select StuID,StuName,ClassName,GradeName,Sex,Phone,Adress from StudentInfo s, ClassInfo c,GradeInfo g  " +
            // " where c.ClassID=s.ClassID and c.GradeID = g.GradeID ";


            //加载数据
            DataTable dtStudents = SqlHelper.GetDataTable(sql);
            //组装
            if (dtStudents.Rows.Count > 0)
            {
                foreach (DataRow dr in dtStudents.Rows)
                {
                    string className = dr["ClassName"].ToString();
                    string gradeName = dr["GradeName"].ToString();
                    dr["ClassName"] = className + "--" + gradeName;
                }

            }
            //只想显示固定的列
            //dgvStudents.AutoGenerateColumns = false;
            dtStudents.Columns.Remove(dtStudents.Columns[3]);
            //绑定数据
            dgvStudents.DataSource = dtStudents;
           
        }

        private void LoadClasses()
        {
            //获取数据  ---查询  ---sql语句
            string sql = "select ClassID,ClassName,GradeName from ClassInfo c,GradeInfo g where c.GradeID=g.GradeID";

            DataTable dtClasses = SqlHelper.GetDataTable(sql);
            //组合班级列表显示项的过程
            if(dtClasses.Rows.Count>0)
            {
                foreach(DataRow dr in dtClasses.Rows)
                {
                    string className = dr["ClassName"].ToString();
                    string gradeName = dr["GradeName"].ToString();
                    dr["ClassName" ]= className + "--" + gradeName;
                }

            }
            //添加默认选择项
            DataRow drNew = dtClasses.NewRow();
            drNew["ClassID"] = 0;
            drNew["ClassName"] = "请选择";

            dtClasses.Rows.InsertAt(drNew, 0);
            //指定数据源
            cboClasses.DataSource = dtClasses;
            cboClasses.DisplayMember = "ClassName";
            cboClasses.ValueMember = "ClassID";
        }

        /// <summary>
        /// 查询学生信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, EventArgs e)
        {
            //接收条件设置信息
            int classId = (int)cboClasses.SelectedValue;
            string stuName = txtStuName.Text.Trim();

            string sql = "select StuID,StuName,ClassName,GradeName,Sex,Phone from StudentInfo s, ClassInfo c,GradeInfo g  " +
                " where c.ClassID=s.ClassID and c.GradeID = g.GradeID ";
            //sql += "where 1=1";
            if(classId>0)
            {
                sql += " and s.ClassId=@ClassId ";
            }
            if(!string.IsNullOrEmpty(stuName))
            {
                sql += " and StuName like @StuName";
            }
            
            sql += " order by StuId ";

            SqlParameter[] paras =
            {
                new SqlParameter("@ClassId",classId),
                new SqlParameter("@StuName","%"+stuName+"%")
            };
            //加载数据
            DataTable dtStudents = SqlHelper.GetDataTable(sql,paras);
            //组装
            if (dtStudents.Rows.Count > 0)
            {
                foreach (DataRow dr in dtStudents.Rows)
                {
                    string className = dr["ClassName"].ToString();
                    string gradeName = dr["GradeName"].ToString();
                    dr["ClassName"] = className + "--" + gradeName;
                }

            }
            //只想显示固定的列
            //dgvStudents.AutoGenerateColumns = false;
            dtStudents.Columns.Remove(dtStudents.Columns[3]);
            //绑定数据
            dgvStudents.DataSource = dtStudents;
        }

        /// <summary>
        /// 修改或删除功能的实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvStudents_CellContentClick(object sender, DataGridViewCellEventArgs e)//点击单元格内容时触发
        {
            if(e.RowIndex!=-1)
            {
                DataRow dr = (dgvStudents.Rows[e.RowIndex].DataBoundItem as DataRowView).Row;//获取行数据的绑定对象
                //获取点击的单元格
                DataGridViewCell cell = dgvStudents.Rows[e.RowIndex].Cells[e.ColumnIndex];
                //判断点击的是修改还是删除列
                if (cell is DataGridViewLinkCell && cell.FormattedValue.ToString() == "修改")
                {
                    //修改操作 打开修改页面，并把stuID给传过去
                    //传值 1.构造函数 2.Tag（最优）   3.公有变量（尽量不用)
                    reLoad = LoadAllStudentList;//赋值给委托
                    int stuId = (int)dr["StuId"];
                    FrmEditStudent frmEdit = new FrmEditStudent();
                    //传值
                    frmEdit.Tag = new TagObject()
                    {
                        EditId = stuId,
                        ReLoad = reLoad
                    };
                    
                    //frmEdit.pubStuId = stuId;
                    frmEdit.MdiParent = this.MdiParent;  //指定修改页面的父容器
                    frmEdit.Show();//顶级窗体   要MDI窗体  

                }
                else if (cell is DataGridViewLinkCell && cell.FormattedValue.ToString() == "删除")
                {
                    //删除操作
                    if (MessageBox.Show("您确定要删除该学生信息吗？", "删除学生提示",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                       
                        int stuId = int.Parse(dr["StuId"].ToString());
                        //假删除    IsDeleted  0 1 数据信息还在
                        //string sqlDel0 = "update StudentInfo set IsDeleted=1 where StuId=@StuId";
                        //SqlParameter para = new SqlParameter("@StuId",stuId);
                        //int count=SqlHelper.ExecuteNonQuery(sqlDel0,para);
                        //if(count>0)
                        //{
                        //    MessageBox.Show("该学生信息删除成功！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //    //DataGridView的数据没有刷新，手动刷新
                        //    DataTable dtStudents = (DataTable)dgvStudents.DataSource;
                        //    //dgvStudents.DataSource = null;
                        //    dtStudents.Rows.Remove(dr);
                        //    dgvStudents.DataSource = dtStudents;
                        //}
                        //else
                        //{
                        //    MessageBox.Show("该学生信息删除失败！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        //    return;
                        //}
                        //真删除 delete  where StuId
                        string sqlDel0 = "delete StudentInfo where StuId=@StuId";
                        SqlParameter para = new SqlParameter("@StuId", stuId);
                        int count = SqlHelper.ExecuteNonQuery(sqlDel0, para);
                        if (count > 0)
                        {
                            MessageBox.Show("该学生信息删除成功！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //DataGridView的数据没有刷新，手动刷新
                            DataTable dtStudents = (DataTable)dgvStudents.DataSource;
                            //dgvStudents.DataSource = null;
                            dtStudents.Rows.Remove(dr);
                            dgvStudents.DataSource = dtStudents;
                        }
                        else
                        {
                            MessageBox.Show("该学生信息删除失败！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
           
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //选择
            //获取要删除的数据stuId
            //判断选择的编号个数，=0 没有选择 提示用户选择要删除 的数据>0 继续
            //删除操作  事务  sql事务 代码里启动事务、
            List<int> listIds = new List<int>();
            for (int i = 0; i < dgvStudents.Rows.Count; i++)
            {
                DataGridViewCheckBoxCell cell = dgvStudents.Rows[i].Cells["colCheck"] as DataGridViewCheckBoxCell;
                bool chk = Convert.ToBoolean(cell.Value);
                if(chk)
                {
                    DataRow dr = (dgvStudents.Rows[i].DataBoundItem as DataRowView).Row;
                    int stuId = (int)dr["StuId"];
                    listIds.Add(stuId);
                }
            }

            //真删除
            if(listIds.Count==0)
            {
                MessageBox.Show("请选择要删除的学生信息！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                if (MessageBox.Show("您确定要删除该学生信息吗？", "删除学生提示",
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int count = 0;
                    //启动事务进行操作
                    using(SqlConnection conn=new SqlConnection(SqlHelper.connString))
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
                            foreach (int id in listIds)
                            {
                                cmd.CommandText = "delete from StudentInfo where StuId=@StuId";
                                SqlParameter para = new SqlParameter("@StuId", id);
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(para);
                                count += cmd.ExecuteNonQuery();
                            }
                            trans.Commit();
                        }
                        catch(SqlException ex)
                        {
                            trans.Rollback();
                            MessageBox.Show("删除学生出现异常！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    if(count==listIds.Count)
                    {
                        MessageBox.Show("这批学生删除成功！", "删除学生提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //手动刷新
                        DataTable dtStudents = (DataTable)dgvStudents.DataSource;
                        //dgvStudents.DataSource = null;
                        string idStr = string.Join(",", listIds);
                        DataRow[] rows = dtStudents.Select("StuId in(" + idStr + ")");
                        foreach(DataRow dr in rows)
                        {
                            dtStudents.Rows.Remove(dr);
                        }
                        dgvStudents.DataSource = dtStudents;
                    }
                }
                }
            }
        }
    }


