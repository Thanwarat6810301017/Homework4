/*
MIT License

Copyright (c) 2026 Sarayut Chaisuriya

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

Note on dataset:
The included MalwareBazaar sample CSV has been modified:
- Limited to first 500 rows
- Header format adjusted for teaching purposes
See README.md for full details.
*/

using System;
using System.IO;
using System.Windows.Forms;

namespace FileProcessing
{
    public partial class frmTextView : Form
    {
        /// <summary>
        /// Initializes a new instance of the frmTextView class.
        /// </summary>
        public frmTextView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the Read button by loading the contents of the specified file into the display area.
        /// </summary>
        private void btRead_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbFileName.Text) || !File.Exists(tbFileName.Text))
            {
                MessageBox.Show("กรุณาเลือกไฟล์ก่อนจ้า!", "แจ้งเตือน");
                return;
            }

            string content = File.ReadAllText(tbFileName.Text);
            rtbShow.Text = content;
        }

        /// <summary>
        /// Handles the Click event of the btReadCSV button, reading CSV data from the specified file and populating the
        /// DataGridView with its contents.
        /// </summary>
        private void btReadCSV_Click(object sender, EventArgs e)
        {
            // ตรวจสอบว่าเลือกไฟล์หรือยัง
            if (string.IsNullOrEmpty(tbFileName.Text) || !File.Exists(tbFileName.Text))
            {
                MessageBox.Show("กรุณาเลือกไฟล์ CSV ก่อนจ้า!", "แจ้งเตือน");
                return;
            }

            // เคลียร์ข้อมูลในตารางเก่าออกก่อนทุกครั้งที่กดโหลดใหม่
            dgvData.Rows.Clear();
            dgvData.Columns.Clear();

            try
            {
                // ดึงค่าตัวเลขจากช่อง m และ n บนหน้าจอ (ถ้าช่องว่างจะตั้งค่า Default เป็น 1 และ 500)
                int m = string.IsNullOrEmpty(txtM.Text) ? 1 : int.Parse(txtM.Text);
                int n = string.IsNullOrEmpty(txtN.Text) ? 500 : int.Parse(txtN.Text);

                // ดึงข้อความประเภทไฟล์จากช่องกรอง ตัดช่องว่าง และแปลงเป็นอักษรพิมพ์เล็ก
                string fileTypeFilter = txtFilter.Text.Trim().ToLower();

                int currentLine = 0; // ตัวนับแถวปัจจุบัน

                using (StreamReader srReader = new StreamReader(tbFileName.Text))
                {
                    string strLine;
                    bool bHeaderRead = false;

                    // ลูปอ่านทีละบรรทัด (ใช้ StreamReader เครื่องจะไม่ค้างแม้ไฟล์มีข้อมูลเยอะ)
                    while ((strLine = srReader.ReadLine()) != null)
                    {
                        string[] strHeaders_arr = null;

                        // ข้ามบรรทัดที่เป็น Comment ของอาจารย์ (#)
                        if (strLine.StartsWith("#"))
                        {
                            if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("#HEADER"))
                            {
                                strHeaders_arr = strLine.Substring(8).Split(',');
                            }
                            continue;
                        }

                        string[] strValues_arr = strLine.Split(',');

                        // 1. จัดการหัวตารางก่อน (ให้แสดงหัวตารางเสมอ)
                        if (!bHeaderRead)
                        {
                            foreach (string strHeader in strValues_arr)
                            {
                                if (strHeaders_arr == null)
                                    dgvData.Columns.Add(strHeader.Trim(), strHeader.Trim());
                                else
                                    dgvData.Columns.Add(strHeader.Trim(), strHeaders_arr[dgvData.Columns.Count].Trim());
                            }
                            bHeaderRead = true;
                            continue; // วิ่งไปอ่านแถวถัดไปที่เป็นข้อมูลจริง
                        }

                        // นับแถวข้อมูลจริงหลังจากผ่านหัวตารางมาแล้ว
                        currentLine++;

                        // 2. ตรวจสอบเงื่อนไขช่วงแถว m ถึง n
                        if (currentLine >= m && currentLine <= n)
                        {
                            // 3. ตรวจสอบตัวกรองประเภทไฟล์ 
                            // (ถ้าไม่ได้กรอกตัวกรองไว้ หรือข้อมูลในแถวนั้นมีคำที่ค้นหาอยู่ ก็ให้แสดงผล)
                            if (string.IsNullOrEmpty(fileTypeFilter) || strLine.ToLower().Contains(fileTypeFilter))
                            {
                                dgvData.Rows.Add(strValues_arr);
                            }
                        }

                        // ถ้าอ่านเกินแถวที่ n แล้ว ให้หยุดอ่านทันทีเพื่อประหยัดทรัพยากรเครื่อง
                        if (currentLine > n)
                        {
                            break;
                        }
                    }
                }
                MessageBox.Show($"โหลดข้อมูลแถวที่ {m} ถึง {n} (กรองเฉพาะ: {fileTypeFilter}) เสร็จแล้วจ้า!", "สำเร็จ");
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการโหลดไฟล์: " + ex.Message, "ข้อผิดพลาด");
            }
        }

        /// <summary>
        /// Handles the Click event of the Browse button to select a file from computer.
        /// </summary>
        private void btBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbFileName.Text = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the new Filter button by triggering the main CSV reader.
        /// </summary>
        private void btnFilterRun_Click(object sender, EventArgs e)
        {
            // สั่งให้ฟังก์ชันหลัก (btReadCSV_Click) ทำงานประมวลผลทันที โดยใช้ค่า m, n, filter ที่พิมบนหน้าจอ UI
            btReadCSV_Click(sender, e);
        }
    }   // End of frmTextView class
}