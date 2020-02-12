using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BlogSystem.MVCSite.Tools
{
    public static class FilterHtml
    {
        public static string StripHTML(string strHtml)
        {
            string[] aryReg =
            {
                @"<script[^>]*?>.*?</script>",
                @"<img\s+[^>]*\s*src\s*=\s*([']?)(?<url>\S+)'?[^>]*>",
                @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
                @"([\r\n])[\s]+",
                @"&(quot|#34);",
                @"&(amp|#38);",
                @"&(lt|#60);",
                @"&(gt|#62);",
                @"&(nbsp|#160);",
                @"&(iexcl|#161);",
                @"&(cent|#162);",
                @"&(pound|#163);",
                @"&(copy|#169);",
                @"&#(\d+);",
                @"-->",
                @"<!--.*\n"               
             };

            string[] aryRep =
            {
                "",
                "[图片]",
                " ",
                "",
                "\"",
                "&",
                "<",
                ">",
                "   ",
                "\xa1",  //chr(161),
                "\xa2",  //chr(162),
                "\xa3",  //chr(163),
                "\xa9",  //chr(169),
                "",
                "\r\n",
                ""
             };

            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, aryRep[i]);
            }
            //去除多余空格，合并剩一个
            strOutput = strOutput.Trim();
            Regex regex1 = new Regex("\\s+", RegexOptions.IgnoreCase);
            strOutput = regex1.Replace(strOutput, " ");
            //strOutput.Replace("<", "");
            //strOutput.Replace(">", "");
            //strOutput.Replace("\r\n", "");
            return strOutput;
        }


    }
}