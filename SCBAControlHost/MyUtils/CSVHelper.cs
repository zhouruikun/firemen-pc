using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace MyUtils
{
	class CSVHelper
	{
		/// <summary>
		/// 将DataTable中数据写入到CSV文件中
		/// </summary>
		/// <param name="dt">提供保存数据的DataTable</param>
		/// <param name="fileName">CSV的文件路径</param>
		public static bool SaveCSV(DataTable dt, string fullPath)
		{
			try
			{
				FileInfo fi = new FileInfo(fullPath);
				if (!fi.Directory.Exists)
				{
					fi.Directory.Create();
				}
				FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				//StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
				StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
				string data = "";
				//写出列名称
				for (int i = 0; i < dt.Columns.Count; i++)
				{
					data += "\"" + dt.Columns[i].ColumnName.ToString() + "\"";
					if (i < dt.Columns.Count - 1)
					{
						data += ",";
					}
				}
				sw.WriteLine(data);
				//写出各行数据
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					data = "";
					for (int j = 0; j < dt.Columns.Count; j++)
					{
						string str = dt.Rows[i][j].ToString();
						str = string.Format("\"{0}\"", str);
						data += str;
						if (j < dt.Columns.Count - 1)
						{
							data += ",";
						}
					}
					sw.WriteLine(data);
				}
				sw.Close();
				fs.Close();
				return true;
			}
			catch
			{
				return false;
			}
		}
		/// <summary>
		/// 读取CSV文件到DataTable中
		/// </summary>
		/// <param name="filePath">CSV的文件路径</param>
		/// <returns></returns>
		public static List<List<string>> ReadCSV(string filePath)
		{
			List<List<string>> list = new List<List<string>>();
			using (CsvFileReader reader = new CsvFileReader(filePath))
			{
				CsvRow row = new CsvRow();
				while (reader.ReadRow(row))
				{
					List<string> listRow = new List<string>();
					foreach (string s in row)
						listRow.Add(s.Replace("\"", ""));
					list.Add(listRow);
				}
			}
			return list;
		}

		
	}

	public class CsvRow : List<string>
	{
		public string LineText { get; set; }
	}
	public class CsvFileReader : StreamReader
	{
		public CsvFileReader(Stream stream)
			: base(stream)
		{
		}

		public CsvFileReader(string filename)
			: base(filename, Encoding.Default)
		{
		}

		/// <summary>  
		/// Reads a row of data from a CSV file  
		/// </summary>  
		/// <param name="row"></param>  
		/// <returns></returns>  
		public bool ReadRow(CsvRow row)
		{
			row.LineText = ReadLine();
			if (String.IsNullOrEmpty(row.LineText))
				return false;

			int pos = 0;
			int rows = 0;

			while (pos < row.LineText.Length)
			{
				string value;

				// Special handling for quoted field  
				if (row.LineText[pos] == '"')
				{
					// Skip initial quote  
					pos++;

					// Parse quoted value  
					int start = pos;
					while (pos < row.LineText.Length)
					{
						// Test for quote character  
						if (row.LineText[pos] == '"')
						{
							// Found one  
							pos++;

							// If two quotes together, keep one  
							// Otherwise, indicates end of value  
							if (pos >= row.LineText.Length || row.LineText[pos] != '"')
							{
								pos--;
								break;
							}
						}
						pos++;
					}
					value = row.LineText.Substring(start, pos - start);
					value = value.Replace("\"\"", "\"");
				}
				else
				{
					// Parse unquoted value  
					int start = pos;
					while (pos < row.LineText.Length && row.LineText[pos] != ',')
						pos++;
					value = row.LineText.Substring(start, pos - start);
				}

				// Add field to list  
				if (rows < row.Count)
					row[rows] = value;
				else
					row.Add(value);
				rows++;

				// Eat up to and including next comma  
				while (pos < row.LineText.Length && row.LineText[pos] != ',')
					pos++;
				if (pos < row.LineText.Length)
					pos++;
			}
			// Delete any unused items  
			while (row.Count > rows)
				row.RemoveAt(rows);

			// Return true if any columns read  
			return (row.Count > 0);
		}
	}

}
