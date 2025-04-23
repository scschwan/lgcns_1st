using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinanceTool
{
    public partial class uc_clusteringPopup : UserControl
    {

        DataTable originDataTable;
        DataTable transformDataTable;

        public uc_clusteringPopup()
        {
            InitializeComponent();
        }

        public void initUI()
        {
            //1.데이터 불러오기
            originDataTable = DataHandler.firstClusteringData;

            Debug.WriteLine("originDataTable 데이터 확인");
            Debug.WriteLine($"originDataTable Table count :{originDataTable.Rows.Count}");


            transformDataTable = DataHandler.secondClusteringData;

            Debug.WriteLine("transformDataTable 데이터 확인");
            Debug.WriteLine($"transformDataTable Table count :{transformDataTable.Rows.Count}");

            //2.DataGridView 설정
            DataHandler.SetupDataGridView(dataGridView1, originDataTable);
            DataHandler.SetupDataGridView(dataGridView2, transformDataTable);
        }

        //regacy-code
        private void SetupDataGridView(DataGridView dgv, DataTable dt)
        {
            dgv.DataSource = dt;

            // ID 컬럼 숨기기
            if (dgv.Columns["ID"] != null)
            {
                dgv.Columns["ID"].Visible = false;
            }

            // ClusterID 컬럼 숨기기
            dgv.Columns["ClusterID"].Visible = false;

            // dataIndex 컬럼 숨기기
            dgv.Columns["dataIndex"].Visible = false;

            // Count 컬럼 형식 지정
            if (dgv.Columns["Count"] != null)
            {
                dgv.Columns["Count"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["Count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // 합산금액 컬럼 형식 지정
            if (dgv.Columns["합산금액"] != null)
            {
                dgv.Columns["합산금액"].DefaultCellStyle.Format = "N0"; // 천 단위 구분자
                dgv.Columns["합산금액"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            // 클러스터명 컬럼 수정 가능 설정 (transformDataTable인 경우에만)
            //해당 기능 삭제
            if (dt == DataHandler.secondClusteringData && dgv.Columns["클러스터명"] != null)
            {
                //dgv.Columns["클러스터명"].ReadOnly = false;
                //dgv.CellEndEdit += DataGridView_CellEndEdit;
                dgv.Columns["클러스터명"].ReadOnly = true;
            }
            else if (dgv.Columns["클러스터명"] != null)
            {
                dgv.Columns["클러스터명"].ReadOnly = true;
            }

            // 나머지 컬럼들은 읽기 전용
            if (dgv.Columns["키워드목록"] != null)
            {
                dgv.Columns["키워드목록"].ReadOnly = true;
            }

            // 기본 설정
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv != null && e.ColumnIndex == dgv.Columns["클러스터명"].Index)
            {
                // 수정된 값 가져오기
                string newValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? "";

                // DataHandler.secondClusteringData 업데이트
                int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["ID"].Value);
                DataRow[] rows = DataHandler.secondClusteringData.Select($"ID = {id}");
                if (rows.Length > 0)
                {
                    rows[0]["클러스터명"] = newValue;
                }

                // 변경 사항 저장
                DataHandler.secondClusteringData.AcceptChanges();
            }
        }
    }
}
