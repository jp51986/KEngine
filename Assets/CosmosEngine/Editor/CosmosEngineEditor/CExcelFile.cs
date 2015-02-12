﻿using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

class CExcelFile
{
    //private Workbook Workbook_;
    //private Worksheet Worksheet_;
    public Dictionary<string, int> ColName2Index;
    //private DataTable DataTable_;
    private string Path;
    private IWorkbook Workbook;
    private ISheet Worksheet;
    public bool IsLoadSuccess = true;
    public CExcelFile(string excelPath)
    {
        Path = excelPath;

        using (var file = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
        {
            try
            {
                Workbook = WorkbookFactory.Create(file);
            }
            catch (Exception e)
            {
                CDebug.LogError("无法打开Excel: {0}, 可能原因：正在打开？或是Office2007格式（尝试另存为）？ {1}", excelPath, e.Message);
                IsLoadSuccess = false;
            }
            
        }
        if (IsLoadSuccess)
        {
            CDebug.Assert(Workbook);

            //var dt = new DataTable();

            Worksheet = Workbook.GetSheetAt(0);
            ColName2Index = new Dictionary<string, int>();
            var headerRow = Worksheet.GetRow(0);
            int columnCount = headerRow.LastCellNum;

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var cell = headerRow.GetCell(columnIndex);
                if (cell == null)
                {
                    //CDebug.LogError("Null Cel on Column: {0}, File: {1}", columnIndex, excelPath);
                    // 可能是空字符串的格子，忽略！
                    continue;
                }
                ColName2Index[cell.ToString()] = columnIndex;
            }
        }
        
    }

    public void RemoveRow(int row)
    {
        var theRow = Worksheet.GetRow(row);
        Worksheet.RemoveRow(theRow);
    }

    public float GetFloat(string columnName, int row)
    {
        return GetString(columnName, row).ToFloat();
    }
    public int GetInt(string columnName, int row)
    {
        return GetString(columnName, row).ToInt32();
    }
    public string GetString(string columnName, int row)
    {
        var theRow = Worksheet.GetRow(row);
        if (theRow == null)
            theRow = Worksheet.CreateRow(row);

        var colIndex = ColName2Index[columnName];
        var cell = theRow.GetCell(colIndex);
        if (cell == null)
            cell = theRow.CreateCell(colIndex);
        return cell.ToString();
    }

    public int GetRows()
    {
        return Worksheet.LastRowNum;
    }

    public void SetRowColor(int row, short colorIndex)
    {
        var theRow = Worksheet.GetRow(row);
        foreach (var cell in theRow.Cells)
        {
            var style = cell.CellStyle;
            style.FillBackgroundColor = colorIndex;
        }
    }

    public void SetRow(string columnName, int row, string value)
    {
        if (!ColName2Index.ContainsKey(columnName))
        {
            CDebug.LogError("No Column: {0} of File: {1}", columnName, Path);
            return;
        }
        var theRow = Worksheet.GetRow(row);
        if (theRow == null)
            theRow = Worksheet.CreateRow(row);
        var cell = theRow.GetCell(ColName2Index[columnName]);
        if (cell == null)
            cell = theRow.CreateCell(ColName2Index[columnName]);
        cell.SetCellValue(value);
    }

    public void Save()
    {
        //try
        {
            using (var memStream = new MemoryStream())
            {
                Workbook.Write(memStream);
                memStream.Flush();
                memStream.Position = 0;

                using (var fileStream = new FileStream(Path, FileMode.Create, FileAccess.Write))
                {
                    var data = memStream.ToArray();
                    fileStream.Write(data, 0, data.Length);
                    fileStream.Flush();
                }
            }

        }
        //catch (Exception e)
        //{
        //    CDebug.LogError(e.Message);
        //    CDebug.LogError("是否打开了Excel表？");
        //}

    }
}