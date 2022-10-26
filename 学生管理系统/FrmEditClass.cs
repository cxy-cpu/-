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
    public partial class FrmEditClass : Form
    {
        public FrmEditClass()
        {
            InitializeComponent();
        }

        private int classId = 0;
        private string oldName = "";
        private int oldGradeId = 0;
        private Action reLoad = null;//刷新列表页所用
        /// <summary>
        /// 打开页面，加载年级列表，加载班级的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmEditClass_Load(object sender, EventArgs e)
        {
            InitGradeList();  //加载年级列表
            InitClassInfo();
        }

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

        private void InitClassInfo()
        {
            //获取到StuId
            if (this.Tag != null)
            {
                TagObject tagObject = (TagObject)this.Tag;
                classId = tagObject.EditId;
                reLoad = tagObject.ReLoad;//赋值
            }
            //查询数据
            string sql = "select  ClassName,GradeId,Remark from ClassInfo where ClassId=@ClassId";
            SqlParameter paraId = new SqlParameter("@ClassId", classId);
            SqlDataReader dr = SqlHelper.ExecuteReader(sql, paraId);
            //读取数据  只能向前 不能后退 读一条丢一条
            if (dr.Read())
            {
                txtClassName.Text = dr["ClassName"].ToString();
                oldName = txtClassName.Text.Trim();
                txtRemark.Text = dr["Remark"].ToString();
                int gradeId = (int)dr["GradeId"];
                oldGradeId = gradeId;
                cboGrades.SelectedValue = gradeId;
            }
            dr.Close();

        }


        /// <summary>
        /// 提交修改信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            //获取页面输入
            string className = txtClassName.Text.Trim();
            int gradeId = (int)cboGrades.SelectedValue;
            string remark = txtRemark.Text.Trim();

            //判断是否为空
            if (string.IsNullOrEmpty(className))
            {
                MessageBox.Show("班级名称不能为空!", "修改班级提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //判断是否已经存在，在同一年级下，除了这个班级自己，其他的班级信息中，班级名称不能重名
            string sqlExists = "select count(1) from ClassInfo where ClassName=@ClassName and GradeID=@GradeID ";
            if(className==oldName&&gradeId==oldGradeId)
            {
                sqlExists += "and ClassId<>@ClassId";
            }
            SqlParameter[] paras =
            {
                    new SqlParameter("@ClassName",className),
                    new SqlParameter("@GradeID",gradeId),
                    new SqlParameter("@ClassId",classId)
            };
            object oCount = SqlHelper.ExecuteScalar(sqlExists, paras);//count为查询后一个返回值
            if (oCount != null &&((int)oCount) > 0)
            {
                MessageBox.Show("班级名称已经存在!", "修改班级提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //修改提交
            string sqlEdit = "Update ClassInfo set ClassName=@ClassName,GradeId=@GradeId,Remark=@Remark where ClassId=@ClassId";
            SqlParameter[] parasEdit =
            {
                        new SqlParameter("@ClassName",className),
                        new SqlParameter("@GradeID",gradeId),
                        new SqlParameter("@Remark",remark),
                        new SqlParameter("@ClassId",classId)
            };
            //执行返回值
            int count = SqlHelper.ExecuteNonQuery(sqlEdit, parasEdit);
            if (count > 0)
            {
                MessageBox.Show($"班级:{className}修改成功！", "修改班级提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                reLoad.Invoke();
            }
            else
            {
                MessageBox.Show("班级修改失败!", "修改班级提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
