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
    public partial class FrmAddStudent : Form
    {
        public FrmAddStudent()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //获取页面信息输入
            string stuName = txtStuName.Text.Trim();
            int classId = (int)cboClasses.SelectedValue;
            string sex = rbtMale.Checked ? rbtMale.Text : rbtFemale.Text;//三目运算符，只能选中一个性别
            string phone = txtPhone.Text.Trim();
            string adress = txtAdress.Text.Trim();
            //判空处理  姓名不能为空 电话不可以为空
            if(string.IsNullOrEmpty(stuName))
            {
                MessageBox.Show("姓名不能为空!", "添加学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("电话不能为空!", "添加电话提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrEmpty(adress))
            {
                MessageBox.Show("地址不能为空!", "添加地址提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            //判断电话+姓名是否在数据库里已经存在
            string sql = "select count(1) from StudentInfo where StuName=@StuName  and Phone=@phone and Adress=@Adress";
            SqlParameter[] paras =
            {
                new SqlParameter("@StuName",stuName),
                new SqlParameter("@phone",phone),
                new  SqlParameter("Adress",adress)
            };
            object o = SqlHelper.ExecuteScalar(sql, paras);
            if(o!=null&&o!=DBNull.Value&&((int)o)>0)
            {
                MessageBox.Show("该学生已经存在!", "添加学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //添加入库 参数 执行 完成返回受影响行数
            string sqlAdd = "insert into StudentInfo( StuName,ClassID,Sex,Phone,Adress) values(@StuName,@ClassID,@Sex,@Phone,@Adress)";
            SqlParameter[] parasAdd =
            {
                new SqlParameter("@StuName",stuName),
                new SqlParameter("@ClassID",classId),
                new SqlParameter("@Sex",sex),
                new SqlParameter("@phone",phone),
                new SqlParameter("@Adress",adress)
            };
            int count = SqlHelper.ExecuteNonQuery(sqlAdd, parasAdd);
            if(count>0)
            {
                MessageBox.Show($"学生:{stuName}添加成功!", "添加学生提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {
                MessageBox.Show("该学生添加失败，请检查!", "添加学生提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// 加载班级列表、初始化性别（默认为男）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmAddStudent_Load(object sender, EventArgs e)
        {
            InitClasses();//获取班级信息
            rbtMale.Checked = true;//设置性别默认为男
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
                cboClasses.SelectedIndex = 0;//设置班级列表的默认项为第一项
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
