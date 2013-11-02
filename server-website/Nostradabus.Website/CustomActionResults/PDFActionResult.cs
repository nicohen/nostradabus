using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nostradabus.WebSite.CustomActionResults
{
    public class PDFActionResult : ViewResult
    {
        #region Private Members

        private readonly byte[] pdfArray;

        private readonly string fileName;

        #endregion

        #region Constructors

        public PDFActionResult(byte[] pdfByteArray, string fileName)
        {
            pdfArray = pdfByteArray;

            this.fileName = fileName;
        }

        #endregion

        #region Methods

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.BinaryWrite(pdfArray);

            context.HttpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Pdf;

            context.HttpContext.Response.AddHeader("content-disposition", string.Concat("attachment;  filename=", fileName, ".pdf"));
        }

        #endregion
    }
}