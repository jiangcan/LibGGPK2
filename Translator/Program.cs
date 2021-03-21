using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Xml;
using LibDat2;
using LibDat2.Types;
using OfficeOpenXml;

namespace Translator
{

    class DatContainer2 : DatContainer
    {

        private BinaryWriter _writer;
        private MemoryStream _memoryStream = new MemoryStream();
        private List<PointedValue> _strValueTypes;
        private Dictionary<long, long> _chhangeOffsetDic = new Dictionary<long, long>();
        public DatContainer2(string filePath) : base(filePath)
        {
            var mm = File.ReadAllBytes(filePath);

            _writer = new BinaryWriter(_memoryStream);
            _writer.Write(mm, 0, mm.Length);
        }

        public DatContainer2(byte[] data, string fileName) : base(data, fileName)
        {
        }

        public DatContainer2(Stream stream, long length, string fileName) : base(stream, length, fileName)
        {



        }

        public DatContainer2(BinaryReader reader, long length, string fileName) : base(reader, length, fileName)
        {
        }

        public void ChangeOffset(PointerType pointerType, long newOffset)
        {

            _writer.Seek((int)pointerType.Offset, SeekOrigin.Begin);
            _writer.Write(x64 ? Convert.ToInt64(newOffset) : Convert.ToInt32(newOffset));

        }
        public void ChangeString(Dictionary<string, string> translatorDictionary)
        {

            foreach (var pd in PointedDatas)
            {
                if (pd.Value.FieldValue is ValueType<string> strValue)
                {


                    if (translatorDictionary.TryGetValue((string)strValue.Value, out var str))
                    {
                        _writer.Seek(0, SeekOrigin.End);
                        var newOffset = _writer.BaseStream.Position - DataSectionOffset;
                        Console.WriteLine(newOffset);
                        //newOffset = 682000;
                        ChangeOffset(pd.Value.Pointer, newOffset);
                        _writer.Seek(0, SeekOrigin.End);
                        

                        _writer.Write(Encoding.Unicode.GetBytes(str));

                       
                        _writer.Write(0);
                        Console.WriteLine(_writer.BaseStream.Position );
                        return;

                    }
                }
            }


        }

        public void Save(string folder)
        {

            var fullPath = Path.Combine(folder, DatName + (x64 ? ".dat64" : ".dat"));
            File.WriteAllBytes(fullPath, _memoryStream.ToArray());
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //var input = @"D:\Bundles2\ActiveSkills.dat64";
            var input = @"D:\ActiveSkills.dat64";

            var dic = new Dictionary<string, string>();

            using (var pak = new ExcelPackage(new FileInfo("D:\\ActiveSkills.xlsx")))
            {
                var sht = pak.Workbook.Worksheets.First();
                var start = sht.Dimension.Start;
                var end = sht.Dimension.End;
                for (int row = start.Row; row <= end.Row; row++)
                {
                    dic.TryAdd(sht.Cells[row, 1].Text, sht.Cells[row, 2].Text);
                    Console.WriteLine($"{sht.Cells[row, 1].Text}, {sht.Cells[row, 2].Text}");
                    break;

                }
            }

            var dat = new DatContainer2(input);
            dat.ChangeString(dic);
            dat.Save("D:\\新建文件夹");
            //foreach (var fieldDatas in dat.FieldDefinitions)
            //{

            //        Console.WriteLine($"{fieldDatas.Key},{fieldDatas.Value}");

            //}

            //KeyValuePair<long, PointedValue> last=new KeyValuePair<long, PointedValue>();
            //var index = 0;
            //Console.WriteLine(dat.PointedDatas.Count);
            //var b = dat.Save();

            //File.WriteAllBytes(oout, b);


            //if (index > 0)
            //{
            //    var l = fieldDatas.Key - last.Key;
            //    //Console.WriteLine($"{l}");
            //    var chars = new byte[l];
            //    stream.BaseStream.Seek(dat.DataSectionOffset + last.Key, SeekOrigin.Begin);
            //    stream.BaseStream.Read(chars, 0, (int) l);

            //    Console.WriteLine(Encoding.Unicode.GetString(chars));
            //}

            //last = fieldDatas;
            //index++;
            //Console.WriteLine($"{fieldDatas.Key},{fieldDatas.Value},{Encoding.UTF8.GetByteCount(fieldDatas.Value.ToString().ToCharArray().Append('\0').ToArray())}");


            //File.WriteAllText("D:\\1.csv", dat.ToCsv());


        }

    }
}
