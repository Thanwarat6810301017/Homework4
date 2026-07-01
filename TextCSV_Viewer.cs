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
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The event data.</param>
		private void btRead_Click(object sender, EventArgs e)
		{			
            string content = File.ReadAllText(tbFileName.Text);
            rtbShow.Text = content;
		}
        /// <summary>
        /// Handles the Click event of the btReadCSV button, reading CSV data from the specified file and populating the
        /// DataGridView with its contents.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
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
                // ---------------------------------------------------------------
                // [ตั้งค่าตัวแปรตรงนี้เพื่อทดสอบส่งอาจารย์ได้เลยจ้า]
                int m = 10;                // แถวเริ่มต้นที่ต้องการโหลด (โจทย์ข้อ 2)
                int n = 50;                // แถวสิ้นสุดที่ต้องการโหลด (โจทย์ข้อ 2)
                string fileTypeFilter = "exe"; // ประเภทไฟล์ที่ต้องการกรอง (โจทย์ข้อ 3)
                                               // ---------------------------------------------------------------

                int currentLine = 0; // ตัวนับแถวปัจจุบัน

                using (StreamReader srReader = new StreamReader(tbFileName.Text))
                {
                    string strLine;
                    bool bHeaderRead = false;

                    // ลูปอ่านทีละบรรทัด (โจทย์ข้อ 1: ใช้ StreamReader เครื่องจะไม่ค้างแม้ไฟล์มี 1 ล้านแถว)
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

                        // 2. ตรวจสอบเงื่อนไขช่วงแถว m ถึง n (โจทย์ข้อ 2)
                        if (currentLine >= m && currentLine <= n)
                        {
                            // 3. ตรวจสอบตัวกรองประเภทไฟล์ (โจทย์ข้อ 3)
                            // (โปรแกรมจะเช็กว่าในบรรทัดนั้นมีคำว่า exe หรือพิมพ์เล็กพิมพ์ใหญ่ตรงกันไหม)
                            if (strLine.ToLower().Contains(fileTypeFilter.ToLower()))
                            {
                                // ถ้าตรงเงื่อนไขทั้งหมด ให้เพิ่มแถวลงตาราง DataGridView
                                dgvData.Rows.Add(strValues_arr);
                            }
                        }

                        // ถ้าอ่านเกินแถวที่ n แล้ว ให้หยุดอ่านทันทีเพื่อประหยัดความเร็ว
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
	}   // End of frmTextView class
}
