﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using JetBrains.Annotations;
using NPOI.SS.UserModel;
using WeihanLi.Extensions;

namespace WeihanLi.Npoi
{
    public static class NpoiExtensions
    {
        /// <summary>
        /// Workbook2EntityList
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="workbook">excel workbook</param>
        /// <param name="sheetIndex">sheetIndex</param>
        /// <returns>entity list</returns>
        public static List<TEntity> ToEntityList<TEntity>([NotNull]this IWorkbook workbook) where TEntity : new() => workbook.ToEntityList<TEntity>(0);

        /// <summary>
        /// Workbook2EntityList
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="workbook">excel workbook</param>
        /// <param name="sheetIndex">sheetIndex</param>
        /// <returns>entity list</returns>
        public static List<TEntity> ToEntityList<TEntity>([NotNull]this IWorkbook workbook, int sheetIndex) where TEntity : new()
        {
            if (workbook.NumberOfSheets <= sheetIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(sheetIndex), string.Format(Resource.IndexOutOfRange, nameof(sheetIndex), workbook.NumberOfSheets));
            }
            var sheet = workbook.GetSheetAt(sheetIndex);
            return new NpoiHelper<TEntity>().SheetToEntityList(sheet);
        }

        /// <summary>
        /// Sheet2EntityList
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="sheet">excel sheet</param>
        /// <returns>entity list</returns>
        public static List<TEntity> ToEntityList<TEntity>([NotNull]this ISheet sheet) where TEntity : new() => new NpoiHelper<TEntity>().SheetToEntityList(sheet);

        /// <summary>
        /// Workbook2ToDataTable
        /// </summary>
        /// <param name="workbook">excel workbook</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable([NotNull]this IWorkbook workbook) => workbook.ToDataTable(0, 0);

        /// <summary>
        /// Workbook2ToDataTable
        /// </summary>
        /// <param name="workbook">excel workbook</param>
        /// <param name="sheetIndex">sheetIndex</param>
        /// <param name="headerRowIndex">headerRowIndex</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable([NotNull]this IWorkbook workbook, int sheetIndex, int headerRowIndex)
        {
            if (workbook.NumberOfSheets <= sheetIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(sheetIndex), string.Format(Resource.IndexOutOfRange, nameof(sheetIndex), workbook.NumberOfSheets));
            }
            return workbook.GetSheetAt(sheetIndex).ToDataTable(headerRowIndex);
        }

        /// <summary>
        /// Sheet2DataTable
        /// </summary>
        /// <param name="sheet">excel sheet</param>
        /// <param name="headerRowIndex">headerRowIndex</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable([NotNull]this ISheet sheet) => sheet.ToDataTable(0);

        /// <summary>
        /// Sheet2DataTable
        /// </summary>
        /// <param name="sheet">excel sheet</param>
        /// <param name="headerRowIndex">headerRowIndex</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable([NotNull]this ISheet sheet, int headerRowIndex)
        {
            if (sheet.PhysicalNumberOfRows <= headerRowIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(headerRowIndex), string.Format(Resource.IndexOutOfRange, nameof(headerRowIndex), sheet.PhysicalNumberOfRows));
            }
            var dataTable = new DataTable(); var rowEnumerator = sheet.GetRowEnumerator();
            while (rowEnumerator.MoveNext())
            {
                var row = (IRow)rowEnumerator.Current;
                if (row.RowNum < headerRowIndex)
                {
                    continue;
                }

                if (row.RowNum == headerRowIndex)
                {
                    foreach (var cell in row.Cells)
                    {
                        dataTable.Columns.Add(cell.StringCellValue.Trim());
                    }
                }
                else
                {
                    var dataRow = dataTable.NewRow();
                    for (var i = 0; i < row.Cells.Count; i++)
                    {
                        dataRow[i] = row.Cells[i].GetCellValue(typeof(string));
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }
            return dataTable;
        }

        /// <summary>
        /// import entityList to workbook first sheet
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="workbook">workbook</param>
        /// <param name="list">entityList</param>
        public static int ImportData<TEntity>([NotNull]this IWorkbook workbook, IReadOnlyList<TEntity> list) where TEntity : new() => workbook.ImportData(list, 0);

        /// <summary>
        /// import entityList to workbook sheet
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="workbook">workbook</param>
        /// <param name="list">entityList</param>
        /// <param name="sheetIndex">sheetIndex</param>
        public static int ImportData<TEntity>([NotNull]this IWorkbook workbook, IReadOnlyList<TEntity> list, int sheetIndex) where TEntity : new()
        {
            if (sheetIndex >= ExcelConstants.MaxSheetNum)
            {
                throw new ArgumentException(string.Format(Resource.IndexOutOfRange, nameof(sheetIndex), ExcelConstants.MaxSheetNum), nameof(sheetIndex));
            }

            while (workbook.NumberOfSheets <= sheetIndex)
            {
                workbook.CreateSheet();
            }
            new NpoiHelper<TEntity>().EntityListToSheet(workbook.GetSheetAt(sheetIndex), list);
            return 1;
        }

        /// <summary>
        /// import entityList to sheet
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="sheet">sheet</param>
        /// <param name="list">entityList</param>
        public static ISheet ImportData<TEntity>([NotNull]this ISheet sheet, IReadOnlyList<TEntity> list)
            where TEntity : new() => new NpoiHelper<TEntity>().EntityListToSheet(sheet, list);

        /// <summary>
        /// import datatable to workbook first sheet
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="workbook">workbook</param>
        /// <param name="dataTable">dataTable</param>
        public static int ImportData<TEntity>([NotNull]this IWorkbook workbook, DataTable dataTable) where TEntity : new() => workbook.ImportData<TEntity>(dataTable, 0);

        /// <summary>
        /// import datatable to workbook first sheet
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="workbook">workbook</param>
        /// <param name="dataTable">dataTable</param>
        /// <param name="sheetIndex">sheetIndex</param>
        public static int ImportData<TEntity>([NotNull]this IWorkbook workbook, DataTable dataTable, int sheetIndex) where TEntity : new()
        {
            if (sheetIndex >= ExcelConstants.MaxSheetNum)
            {
                throw new ArgumentException(string.Format(Resource.IndexOutOfRange, nameof(sheetIndex), ExcelConstants.MaxSheetNum), nameof(sheetIndex));
            }
            while (workbook.NumberOfSheets <= sheetIndex)
            {
                workbook.CreateSheet();
            }
            new NpoiHelper<TEntity>().DataTableToSheet(workbook.GetSheetAt(sheetIndex), dataTable);
            return 1;
        }

        /// <summary>
        /// import datatable to sheet
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="sheet">sheet</param>
        /// <param name="dataTable">dataTable</param>
        public static ISheet ImportData<TEntity>([NotNull]this ISheet sheet, DataTable dataTable)
            where TEntity : new() => new NpoiHelper<TEntity>().DataTableToSheet(sheet, dataTable);

        /// <summary>
        /// EntityList2ExcelFile
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="entityList">entityList</param>
        /// <param name="excelPath">excelPath</param>
        public static int ToExcelFile<TEntity>([NotNull] this IReadOnlyList<TEntity> entityList, [NotNull]string excelPath)
            where TEntity : new()
        {
            TypeCache.TypeExcelConfigurationDictionary.TryGetValue(typeof(TEntity), out var configuration);
            var workbook = ExcelHelper.PrepareWorkbook(excelPath, configuration?.ExcelSetting);
            workbook.ImportData(entityList);
            workbook.WriteToFile(excelPath);
            return 1;
        }

        /// <summary>
        /// EntityList2ExcelStream
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="entityList">entityList</param>
        /// <param name="stream">stream where to write</param>
        public static int ToExcelStream<TEntity>([NotNull] this IReadOnlyList<TEntity> entityList, [NotNull]Stream stream)
            where TEntity : new()
        {
            TypeCache.TypeExcelConfigurationDictionary.TryGetValue(typeof(TEntity), out var configuration);
            var workbook = ExcelHelper.PrepareWorkbook(true, configuration?.ExcelSetting);
            workbook.ImportData(entityList);
            workbook.Write(stream);
            return 1;
        }

        /// <summary>
        /// 将DataTable内容导出到Excel
        /// </summary>
        /// <param name="dataTable">dataTable</param>
        /// <param name="excelPath">excelPath</param>
        /// <returns></returns>
        public static int ToExcelFile([NotNull] this DataTable dataTable, [NotNull] string excelPath)
        {
            var workbook = ExcelHelper.PrepareWorkbook(excelPath);
            var sheet = workbook.CreateSheet();
            var headerRow = sheet.CreateRow(0);
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                headerRow.CreateCell(i, CellType.String).SetCellValue(dataTable.Columns[i].ColumnName);
            }

            for (var i = 1; i <= dataTable.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i);
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    row.CreateCell(j, CellType.String).SetCellValue(dataTable.Rows[i - 1][j]);
                }
            }

            workbook.WriteToFile(excelPath);
            return 1;
        }

        /// <summary>
        /// DataTable2ExcelStream
        /// </summary>
        /// <param name="dataTable">datatable</param>
        /// <param name="stream">stream</param>
        /// <returns></returns>
        public static int ToExcelStream([NotNull] this DataTable dataTable, [NotNull] Stream stream)
        {
            var workbook = ExcelHelper.PrepareWorkbook();
            var sheet = workbook.CreateSheet();
            var headerRow = sheet.CreateRow(0);
            for (var i = 0; i < dataTable.Columns.Count; i++)
            {
                headerRow.CreateCell(i, CellType.String).SetCellValue(dataTable.Columns[i].ColumnName);
            }

            for (var i = 1; i <= dataTable.Rows.Count; i++)
            {
                var row = sheet.CreateRow(i);
                for (var j = 0; j < dataTable.Columns.Count; j++)
                {
                    row.CreateCell(j, CellType.String).SetCellValue(dataTable.Rows[i][j]);
                }
            }

            workbook.Write(stream);
            return 1;
        }

        /// <summary>
        /// SetCellValue
        /// </summary>
        /// <param name="cell">ICell</param>
        /// <param name="value">value</param>
        public static void SetCellValue([NotNull]this ICell cell, object value) => cell.SetCellValue(value, null);

        /// <summary>
        /// SetCellValue
        /// </summary>
        /// <param name="cell">ICell</param>
        /// <param name="value">value</param>
        /// <param name="formatter">formatter</param>
        public static void SetCellValue([NotNull]this ICell cell, object value, string formatter)
        {
            if (null == value)
            {
                cell.SetCellType(CellType.Blank);
                return;
            }
            if (value is DateTime time)
            {
                cell.SetCellType(CellType.String);
                cell.SetCellValue(string.IsNullOrWhiteSpace(formatter) ?
                    (time.Date == time ? time.ToStandardDateString() : time.ToStandardTimeString())
                    : time.ToString(formatter));
            }
            else
            {
                var type = value.GetType();
                if (
                    type == typeof(double) ||
                    type == typeof(int) ||
                    type == typeof(long) ||
                    type == typeof(float) ||
                    type == typeof(decimal)
                )
                {
                    cell.SetCellType(CellType.Numeric);
                    cell.SetCellValue(Convert.ToDouble(value));
                }
                else if (type == typeof(bool))
                {
                    cell.SetCellType(CellType.Boolean);
                    cell.SetCellValue(Convert.ToBoolean(value));
                }
                else
                {
                    cell.SetCellType(CellType.String);
                    cell.SetCellValue(value.ToString());
                }
            }

            if (formatter.IsNotNullOrWhiteSpace())
            {
                var style = cell.Row.Sheet.Workbook.CreateCellStyle();
                style.DataFormat = cell.Row.Sheet.Workbook.CreateDataFormat().GetFormat(formatter);
                cell.CellStyle = style;
            }
        }

        /// <summary>
        /// GetCellValue
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="propertyType">propertyType</param>
        /// <returns>cellValue</returns>
        public static object GetCellValue([NotNull]this ICell cell, Type propertyType)
        {
            if (string.IsNullOrEmpty(cell.ToString()))
            {
                return propertyType.GetDefaultValue();
            }
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        if (propertyType == typeof(DateTime))
                        {
                            return cell.DateCellValue;
                        }
                        return cell.DateCellValue == cell.DateCellValue.Date
                            ? cell.DateCellValue.ToStandardDateString().ToOrDefault(propertyType)
                            : cell.DateCellValue.ToStandardTimeString().ToOrDefault(propertyType);
                    }
                    if (propertyType == typeof(double))
                    {
                        return cell.NumericCellValue;
                    }
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    // HACK:直接 ToOrDefault() 时，double 转换为 decimal 转换后还是 double
                    return cell.NumericCellValue.ToString().ToOrDefault(propertyType);

                case CellType.String:
                    if (propertyType == typeof(string))
                    {
                        return cell.StringCellValue;
                    }
                    return cell.StringCellValue.ToOrDefault(propertyType);

                case CellType.Boolean:
                    if (propertyType == typeof(bool))
                    {
                        return cell.BooleanCellValue;
                    }
                    return cell.BooleanCellValue.ToString().ToOrDefault(propertyType);

                case CellType.Blank:
                    return propertyType.GetDefaultValue();

                default:
                    return cell.ToString().ToOrDefault(propertyType);
            }
        }

        /// <summary>
        /// GetCellValue
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="cell">cell</param>
        /// <returns></returns>
        public static T GetCellValue<T>([NotNull] this ICell cell)
        {
            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            {
                return cell.DateCellValue.Date == cell.DateCellValue ? cell.DateCellValue.ToStandardDateString().ToOrDefault<T>() : cell.DateCellValue.ToStandardTimeString().ToOrDefault<T>();
            }
            return cell.ToString().ToOrDefault<T>();
        }

        /// <summary>
        /// Write workbook to excel file
        /// </summary>
        /// <param name="workbook">workbook</param>
        /// <param name="filePath">file path</param>
        public static void WriteToFile([NotNull] this IWorkbook workbook, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                workbook.Write(fileStream);
            }
        }

        /// <summary>
        /// ToExcelBytes
        /// </summary>
        /// <param name="entityList">workbook</param>
        public static byte[] ToExcelBytes([NotNull] this IWorkbook workbook)
        {
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                return ms.ToArray();
            }
        }
    }
}
