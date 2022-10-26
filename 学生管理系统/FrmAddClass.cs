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
    public partial class FrmAddClass : Form
    {
        public FrmAddClass()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 页面加载初始化年级列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmAddClass_Load(object sender, EventArgs e)
        {
            InitGradeList(); //加载年级列表
        }

        //一个班级，必须属于某个年级
        private void InitGradeList()
        {
            string sql = "select GradeID ,GradeName  from GradeInfo";
            DataTable dtGradeList = SqlHelper.GetDataTable(sql);
            
            cboGrades.DataSource = dtGradeList;
            //年级名称-----项
            cboGrades.DisplayMember = "GradeName";//显示
            cboGrades.ValueMember = "GradeID";//值

            cboGrades.SelectedIndex = 0;
        }


        /// <summary>
        /// 添加班级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            //信息获取
            string className = txtClassName.Text.Trim();
            int gradeId = (int)cboGrades.SelectedValue;
            string remark = txtRemark.Text.Trim();
            //判断是否为空
            if(string.IsNullOrEmpty(className))
            {
                MessageBox.Show("班级名称不能为空!","添加班级提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            //判断是否已经存在  去数据库里去检查---与数据库交互
            {
                string sqlExists = "select count(1) from ClassInfo where ClassName=@ClassName and GradeID=@GradeID";
                SqlParameter[] paras =
                {
                    new SqlParameter("@ClassName",className),
                    new SqlParameter("@GradeID",gradeId)
                };
                object oCount = SqlHelper.ExecuteScalar(sqlExists, paras);//count为查询后一个返回值
                if(oCount==null||oCount==DBNull.Value||((int)oCount)==0)
                {
                    //添加操作
                    string sqlAdd = "insert into ClassInfo (ClassName,GradeID,Remark) values (@ClassName,@GradeID,@Remark)";
                    SqlParameter[] parasAdd =
                    {
                        new SqlParameter("@ClassName",className),
                        new SqlParameter("@GradeID",gradeId),
                        new SqlParameter("@Remark",remark)
                    };
                    //执行返回值
                    int count = SqlHelper.ExecuteNonQuery(sqlAdd, parasAdd);
                    if(count>0)
                    {
                        MessageBox.Show($"班级:{className}添加成功！", "添加班级提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("班级添加失败!", "添加班级提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("班级名称已经存在!", "添加班级提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
