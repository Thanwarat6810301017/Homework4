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
        public frmTextView()
        {
            InitializeComponent();
        }

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

        private void btReadCSV_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbFileName.Text) || !File.Exists(tbFileName.Text))
            {
                MessageBox.Show("กรุณาเลือกไฟล์ CSV ก่อนจ้า!", "แจ้งเตือน");
                return;
            }

            dgvData.Rows.Clear();
            dgvData.Columns.Clear();

            try
            {
                int m = string.IsNullOrEmpty(txtM.Text) ? 1 : int.Parse(txtM.Text);
                int n = string.IsNullOrEmpty(txtN.Text) ? 500 : int.Parse(txtN.Text);
                string fileTypeFilter = txtFilter.Text.Trim().ToLower();

                int currentLine = 0;

                using (StreamReader srReader = new StreamReader(tbFileName.Text))
                {
                    string strLine;
                    bool bHeaderRead = false;

                    while ((strLine = srReader.ReadLine()) != null)
                    {
                        string[] strHeaders_arr = null;

                        if (strLine.StartsWith("#"))
                        {
                            if (strLine.Length > 8 && strLine.Substring(0, 8).Equals("#HEADER"))
                            {
                                strHeaders_arr = strLine.Substring(8).Split(',');
                            }
                            continue;
                        }

                        string[] strValues_arr = strLine.Split(',');

                        // 1. สร้างหัวตารางในแถวแรกเสมอ
                        if (!bHeaderRead)
                        {
                            foreach (string strHeader in strValues_arr)
                            {
                                string cleanHeader = strHeader.Replace("\"", "").Trim();
                                dgvData.Columns.Add(cleanHeader, cleanHeader);
                            }
                            bHeaderRead = true;
                            continue;
                        }

                        currentLine++;

                        // 2. เช็กช่วงแถว m ถึง n
                        if (currentLine >= m && currentLine <= n)
                        {
                            // 3. กรองแบบอัจฉริยะ: ค้นหาคำแบบเป๊ะๆ ในทุกๆ ช่องของแถว
                            if (string.IsNullOrEmpty(fileTypeFilter))
                            {
                                // ถ้าไม่ได้กรอกช่องกรอง ให้แสดงผลตามปกติ
                                dgvData.Rows.Add(strValues_arr);
                            }
                            else
                            {
                                bool isMatch = false;
                                foreach (string value in strValues_arr)
                                {
                                    // ลบเครื่องหมายคำพูดออกก่อน แล้วเช็กว่าคำตรงกับที่พิมเป๊ะๆ ไหม (ป้องกันโดนคำว่า abuse_ch หลอก)
                                    string cleanValue = value.Replace("\"", "").Trim().ToLower();
                                    if (cleanValue.Equals(fileTypeFilter))
                                    {
                                        isMatch = true;
                                        break; // เจอแล้วให้หยุดลูปทันที
                                    }
                                }

                                // ถ้าเจอช่องที่มีคำตรงเป๊ะๆ ให้แอดขึ้นตาราง
                                if (isMatch)
                                {
                                    dgvData.Rows.Add(strValues_arr);
                                }
                            }
                        }

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

        private void btnFilterRun_Click(object sender, EventArgs e)
        {
            btReadCSV_Click(sender, e);
        }
    }
}