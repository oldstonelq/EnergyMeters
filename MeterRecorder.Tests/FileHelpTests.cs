using System;
using System.IO;
using System.Text;
using MeterRecorder;
using Xunit;

namespace MeterRecorder.Tests
{
    /// <summary>
    /// 导出用的文本写入：CSV 必须走 GB2312，Excel 直接打开才不乱码；
    /// 写不进去时要返回错误说明，而不是悄悄当成成功。
    /// </summary>
    public class FileHelpTests
    {
        /// <summary>
        /// 逐行写入并以 GB2312 编码落盘，读回后内容与编码都应与写入时一致。
        /// </summary>
        [Fact]
        public void WriteCsv_WritesLinesInGb2312()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("out.csv");
                string error;

                bool written = FileHelp.WriteCsv(path,
                    new[] { "时间,电压(V)", "2026-11-17 09:00:00,220.4" },
                    FileMode.Create, out error);

                Assert.True(written, error);
                Assert.Equal(string.Empty, error);
                Assert.Equal(
                    new[] { "时间,电压(V)", "2026-11-17 09:00:00,220.4" },
                    File.ReadAllLines(path, Encoding.GetEncoding("gb2312")));
            }
        }

        /// <summary>
        /// 目录不存在时返回 false 并给出原因，由界面提示用户换路径。
        /// </summary>
        [Fact]
        public void WriteCsv_InvalidPath_ReturnsFalseWithMessage()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string error;

                bool written = FileHelp.WriteCsv(temp.File(@"missing\out.csv"),
                    new[] { "a" }, FileMode.Create, out error);

                Assert.False(written);
                Assert.False(string.IsNullOrWhiteSpace(error));
            }
        }

        /// <summary>
        /// 追加模式下文件原有内容应保留，导出多段数据时不能把前面的覆盖掉。
        /// </summary>
        [Fact]
        public void WriteCsv_AppendMode_KeepsExistingLines()
        {
            using (TempDirectory temp = new TempDirectory())
            {
                string path = temp.File("append.csv");
                string error;

                FileHelp.WriteCsv(path, new[] { "A" }, FileMode.Create, out error);
                FileHelp.WriteCsv(path, new[] { "B" }, FileMode.Append, out error);

                Assert.Equal(new[] { "A", "B" }, File.ReadAllLines(path, Encoding.GetEncoding("gb2312")));
            }
        }

        /// <summary>
        /// 含逗号、引号、换行的字段要整体加引号、内部引号双写，否则列会错位。
        /// </summary>
        [Fact]
        public void JoinCsvFields_EscapesSpecialCharacters()
        {
            Assert.Equal("a,b", FileHelp.JoinCsvFields(new[] { "a", "b" }));
            Assert.Equal("\"a,b\",c", FileHelp.JoinCsvFields(new[] { "a,b", "c" }));
            Assert.Equal("\"他说\"\"好\"\"\"", FileHelp.JoinCsvFields(new[] { "他说\"好\"" }));
            Assert.Equal("\"第一行\n第二行\"", FileHelp.JoinCsvFields(new[] { "第一行\n第二行" }));
        }

        /// <summary>
        /// 行尾不能多出逗号：多一个逗号就多一个空列，程序与 Excel 都会看到。
        /// </summary>
        [Fact]
        public void JoinCsvFields_HasNoTrailingComma()
        {
            string line = FileHelp.JoinCsvFields(new[] { "时间", "电压" });

            Assert.Equal("时间,电压", line);
            Assert.DoesNotContain("\t", line);
            Assert.False(line.EndsWith(",", StringComparison.Ordinal));
        }

        /// <summary>
        /// 表头加数据行，行数与内容都要对应，表头单独一行。
        /// </summary>
        [Fact]
        public void BuildCsvLines_PutsHeaderFirst()
        {
            string[] lines = FileHelp.BuildCsvLines(
                new[] { "时间", "电压(V)" },
                new[]
                {
                    new[] { "2026-11-18 08:00:00", "220.4" },
                    new[] { "2026-11-18 08:00:01", "220.5" }
                });

            Assert.Equal(3, lines.Length);
            Assert.Equal("时间,电压(V)", lines[0]);
            Assert.Equal("2026-11-18 08:00:00,220.4", lines[1]);
            Assert.Equal("2026-11-18 08:00:01,220.5", lines[2]);
        }
    }
}
