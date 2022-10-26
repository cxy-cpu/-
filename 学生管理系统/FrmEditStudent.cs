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
    public partial class FrmEditStudent : Form
    {
        public FrmEditStudent()
        {
            InitializeComponent();
        }

        private int stuId = 0;
        private Action reLoad = null;
        //public int pubStuId = 0;//公有变量
        //public FrmEditStudent(int _stuId)
        //{
        //    InitializeComponent();
        //    stuId = _stuId;
        //}
        private void FrmEditStudent_Load(object sender, EventArgs e)
        {
            InitClasses(); //加载班级列表
            //加载学生信息
            InitStuInfo();
        }

        private void InitStuInfo()
        {
           //获取到StuId
           if(this.Tag!=null)
            {
                TagObject tagObject = (TagObject)this.Tag;
                stuId = tagObject.EditId;
                reLoad = tagObject.ReLoad;//赋值
            }
            //查询数据
            string sql = "select StuName,Sex,ClassId,Phone from StudentInfo where StuId=@StuId";
            SqlParameter paraId = new SqlParameter("@StuId", stuId);
            SqlDataReader dr = SqlHelper.ExecuteReader(sql, paraId);
            //读取数据  只能向前 不能后退 读一条丢一条
            if(dr.Read())
            {
                txtStuName.Text = dr["StuName"].ToString();
                txtPhone.Text = dr["Phone"].ToString();
                string sex = dr["Sex"].ToString();
                if (sex == "男")
                    rbtMale.Checked = true;
                else
                    rbtFemale.Checked = true;
                int classId = (int)dr["ClassID"];
                cboClasses.SelectedValue = classId;
            }
            dr.Close();
        }

        private void InitClasses()
        {
            //获取数据  ---查询  ---sql语句
            string sql = "select ClassID,ClassName,GradeName from ClassInfo c,GradeInfo g where c.GradeID=g.GradeID";

            DataTable dtClasses = SqlHelper.GetDataTable(sql);
            //组合班级列表显示项的过程
            if (dtClasses.Rows.Count > 0)
            {
                foreach (DataRow dr in dtClasses.Rows)
                {
                    string className = dr["ClassName"].ToString();
                    string gradeName = dr["GradeName"].ToString();
                    dr["ClassName"] = className + "--" + gradeName;
                }

                //指定数据源
                cboClasses.DataSource = dtClasses;
                cboClasses.DisplayMember = "ClassName";
                cboClasses.ValueMember = "ClassID";
                //cboClasses.SelectedIndex = 0;//设置班级列表的默认项为第一项
            }
        }

        /// <summary>
        /// 修改学生信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            //获取页面信息
            string stuName = txtStuName.Text.Trim();
            int classId = (int)cboClasses.SelectedValue;
            string sex = rbtMale.Checked ? rbtMale.Text : rbtFemale.Text;//三目运算符，只能选中一个性别
            string phone = txtPhone.Text.Trim();
            //判空处理
            if (string.IsNullOrEmpty(stuName))
            {
                MessageBox.Show("姓名不能为空!", "修改学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("电话不能为空!", "修改电话提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //判断是否存在 姓名+电话 除这个同学自己，其他同学中是否存在
            string sql = "select count(1) from StudentInfo where StuName=@StuName  and Phone=@phone and StuId<>@StuId";
            SqlParameter[] paras =
            {
                new SqlParameter("@StuName",stuName),
                new SqlParameter("@phone",phone),
                new SqlParameter("@StuId",stuId)
            };
            object o = SqlHelper.ExecuteScalar(sql, paras);
            if (o != null && o != DBNull.Value && ((int)o) > 0)
            {
                MessageBox.Show("该学生已经存在!", "修改学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //修改
            string sqlUpdate = "Update StudentInfo set StuName=@StuName,Sex=@Sex,ClassId=@ClassId,Phone=@Phone where StuId=@StuId";
            SqlParameter[] parasUpdate =
            {
                new SqlParameter("@StuName",stuName),
                new SqlParameter("@ClassID",classId),
                new SqlParameter("@Sex",sex),
                new SqlParameter("@Phone",phone),
                new SqlParameter("@StuId",stuId)
            };
            int count = SqlHelper.ExecuteNonQuery(sqlUpdate, parasUpdate);
            if (count > 0)
            {
                MessageBox.Show($"学生:{stuName}修改成功!", "修改学生提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //提示成功后，刷新学生列表 跨页面刷新 使用委托   列表页面定义委托，列表页面加载数据列表这个方法赋值给委托，同时传给修改页面；
                //修改页面定义委托，吧传过来的委托赋值给本页面定义的委托，修改成功后，调用委托
                reLoad.Invoke();
            }
            else
            {
                MessageBox.Show("该学生修改失败，请检查!", "修改学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
