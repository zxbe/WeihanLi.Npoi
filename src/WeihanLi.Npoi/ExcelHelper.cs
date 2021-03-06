﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using WeihanLi.Extensions;
using WeihanLi.Npoi.Configurations;
using WeihanLi.Npoi.Settings;

namespace WeihanLi.Npoi
{
    /// <summary>
    /// ExcelHelper
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// 验证excel文件路径是否可用
        /// </summary>
        /// <param name="excelPath">路径</param>
        /// <param name="msg">错误信息</param>
        /// <param name="isExport">是否是导出操作，导出不需要验证是否存在</param>
        /// <returns>是否可用</returns>
        private static bool ValidateExcelFilePath(string excelPath, out string msg, bool isExport = false)
        {
            if (isExport || File.Exists(excelPath))
            {
                var ext = Path.GetExtension(excelPath);
                if (ext.EqualsIgnoreCase(".xls") || ext.EqualsIgnoreCase(".xlsx"))
                {
                    msg = string.Empty;
                    return true;
                }
                msg = Resource.InvalidFile;
                return false;
            }

            msg = Resource.FileNotFound;
            return false;
        }

        /// <summary>
        /// 根据excel路径加载excel
        /// </summary>
        /// <param name="excelPath">excel路径</param>
        /// <returns>workbook</returns>
        public static IWorkbook LoadExcel(string excelPath)
        {
            if (!ValidateExcelFilePath(excelPath, out var msg))
            {
                throw new ArgumentException(msg);
            }

            using (var stream = File.OpenRead(excelPath))
            {
                return Path.GetExtension(excelPath).EqualsIgnoreCase(".xlsx") ? (IWorkbook)new XSSFWorkbook(stream) : new HSSFWorkbook(stream);
            }
        }

        /// <summary>
        /// 为导出准备 workbook
        /// </summary>
        /// <param name="excelPath">excelPath</param>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook(string excelPath) => PrepareWorkbook(excelPath, null);

        /// <summary>
        /// 为导出准备 workbook
        /// </summary>
        /// <param name="excelPath">excelPath</param>
        /// <param name="excelSetting">excelSetting</param>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook(string excelPath, ExcelSetting excelSetting)
        {
            if (!ValidateExcelFilePath(excelPath, out var msg, true))
            {
                throw new ArgumentException(msg);
            }
            return PrepareWorkbook(Path.GetExtension(excelPath).EqualsIgnoreCase(".xlsx"), excelSetting);
        }

        /// <summary>
        /// 为导出准备 workbook
        /// </summary>
        /// <param name="excelFormat">excelFormat</param>
        /// <param name="excelSetting">excelSetting</param>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook(ExcelFormat excelFormat, ExcelSetting excelSetting)
        {
            return PrepareWorkbook(excelFormat == ExcelFormat.Xlsx, excelSetting);
        }

        /// <summary>
        /// 获取一个 Excel Workbook（xlsx格式）
        /// </summary>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook() => PrepareWorkbook(true);

        /// <summary>
        /// 获取一个 Excel Workbook（xlsx格式）
        /// </summary>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook(ExcelFormat excelFormat) => PrepareWorkbook(excelFormat == ExcelFormat.Xlsx);

        /// <summary>
        /// 获取一个Excel workbook
        /// </summary>
        /// <param name="isXlsx">是否是 Xlsx 格式</param>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook(bool isXlsx) =>
            PrepareWorkbook(isXlsx, null);

        /// <summary>
        /// 获取一个Excel workbook
        /// </summary>
        /// <param name="isXlsx">是否是 Xlsx 格式</param>
        /// <param name="excelSetting">excelSettings</param>
        /// <returns></returns>
        public static IWorkbook PrepareWorkbook(bool isXlsx, ExcelSetting excelSetting)
        {
            var setting = excelSetting ?? new ExcelSetting();

            if (isXlsx)
            {
                var workbook = new XSSFWorkbook();
                var props = workbook.GetProperties();
                props.CoreProperties.Creator = setting.Author;
                props.CoreProperties.Created = DateTime.Now;
                props.CoreProperties.Modified = DateTime.Now;
                props.CoreProperties.Title = setting.Title;
                props.CoreProperties.Subject = setting.Subject;
                props.CoreProperties.Category = setting.Category;
                props.CoreProperties.Description = setting.Description;
                props.ExtendedProperties.GetUnderlyingProperties().Company = setting.Company;
                props.ExtendedProperties.GetUnderlyingProperties().Application = ExcelConstants.ApplicationName;
                return workbook;
            }
            else
            {
                var workbook = new HSSFWorkbook();
                ////create a entry of DocumentSummaryInformation
                var dsi = PropertySetFactory.CreateDocumentSummaryInformation();
                dsi.Company = setting.Company;
                dsi.Category = setting.Category;
                workbook.DocumentSummaryInformation = dsi;
                ////create a entry of SummaryInformation
                var si = PropertySetFactory.CreateSummaryInformation();
                si.Title = setting.Title;
                si.Subject = setting.Subject;
                si.Author = setting.Author;
                si.CreateDateTime = DateTime.Now;
                si.Comments = setting.Description;
                si.ApplicationName = ExcelConstants.ApplicationName;
                workbook.SummaryInformation = si;
                return workbook;
            }
        }

        /// <summary>
        /// 读取Excel的第一个sheet的内容到一个List中
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="excelPath">excelPath</param>
        /// <returns>List</returns>
        public static List<TEntity> ToEntityList<TEntity>(string excelPath) where TEntity : new() => ToEntityList<TEntity>(excelPath, 0);

        /// <summary>
        /// 读取Excel内容到一个List中
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="excelPath">excelPath</param>
        /// <param name="sheetIndex">sheetIndex</param>
        /// <returns>List</returns>
        public static List<TEntity> ToEntityList<TEntity>(string excelPath, int sheetIndex) where TEntity : new()
        {
            var workbook = LoadExcel(excelPath);
            return workbook.ToEntityList<TEntity>(sheetIndex);
        }

        /// <summary>
        /// 读取Excel的第一个Sheet中的内容到DataTable中
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="excelPath">excelPath</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<TEntity>(string excelPath) where TEntity : new() => ToDataTable<TEntity>(excelPath, 0);

        /// <summary>
        /// 读取Excel内容到DataTable中
        /// </summary>
        /// <typeparam name="TEntity">EntityType</typeparam>
        /// <param name="excelPath">excelPath</param>
        /// <param name="sheetIndex">sheetIndex</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<TEntity>(string excelPath, int sheetIndex) where TEntity : new()
            => ToEntityList<TEntity>(excelPath, sheetIndex).ToDataTable();

        /// <summary>
        /// 读取Excel第一个Sheet中的内容到DataTable中
        /// </summary>
        /// <param name="excelPath">excelPath</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(string excelPath) => ToDataTable(excelPath, 0, 0);

        /// <summary>
        /// 读取Excel内容到DataTable中
        /// </summary>
        /// <param name="excelPath">excelPath</param>
        /// <param name="sheetIndex">sheetIndex，默认是0</param>
        /// <param name="headerRowIndex">列首行 headerRowIndex</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable(string excelPath, int sheetIndex, int headerRowIndex)
        {
            var workbook = LoadExcel(excelPath);
            if (workbook.NumberOfSheets <= sheetIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(sheetIndex), string.Format(Resource.IndexOutOfRange, nameof(sheetIndex), workbook.NumberOfSheets));
            }
            return workbook.GetSheetAt(sheetIndex).ToDataTable();
        }

        /// <summary>
        /// 读取 Excel 内容到 DataSet
        /// </summary>
        /// <param name="excelPath">excelPath</param>
        /// <returns></returns>
        public static DataSet ToDataSet(string excelPath) => ToDataSet(excelPath, 0);

        /// <summary>
        /// 读取 Excel 内容到 DataSet
        /// </summary>
        /// <param name="excelPath">excelPath</param>
        /// <param name="headerRowIndex">headerRowIndex</param>
        /// <returns></returns>
        public static DataSet ToDataSet(string excelPath, int headerRowIndex) => LoadExcel(excelPath).ToDataSet(headerRowIndex);

        /// <summary>
        /// SettingFor
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <returns></returns>
        public static IExcelConfiguration<TEntity> SettingFor<TEntity>() => InternalCache.TypeExcelConfigurationDictionary.GetOrAdd(typeof(TEntity), t => InternalHelper.GetExcelConfigurationMapping<TEntity>()) as IExcelConfiguration<TEntity>;
    }
}
