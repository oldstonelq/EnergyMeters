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
    }
}
