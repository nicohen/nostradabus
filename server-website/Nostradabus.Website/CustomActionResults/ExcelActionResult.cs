using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nostradabus.WebSite.CustomActionResults
{
    public class ExcelActionResult : ViewResult
    {
        #region Private Members

        private readonly byte[] excelArray;

        private readonly string fileName;

        #endregion

        #region Constructors

        public ExcelActionResult(byte[] excelByteArray, string fileName)
        {
            excelArray = excelByteArray;

            this.fileName = fileName;
        }

        #endregion

        #region Methods

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.BinaryWrite(excelArray);

            context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            context.HttpContext.Response.AddHeader("content-disposition", string.Concat("attachment;  filename=", fileName, ".xlsx"));
        }

        #endregion
    }
}