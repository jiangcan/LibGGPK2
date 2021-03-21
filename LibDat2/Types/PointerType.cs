using System.IO;

namespace LibDat2.Types {
	public class PointerType : FieldType {

		public override object Value { get => PointTo.FieldValue.Value; }

		public long Pointer;
        public long Offset;
		public PointedValue PointTo;

		public PointerType(BinaryReader Reader, bool x64, DatContainer dat, string PointToType) : base(x64) {
			if (PointToType.StartsWith("list|"))
            {
               
				var Count = x64 ? Reader.ReadInt64() : Reader.ReadUInt32();
                Offset = Reader.BaseStream.Position;
				Pointer = x64 ? Reader.ReadInt64() : Reader.ReadUInt32();
				if (Count > 0 && !dat.PointedDatas.TryGetValue(Pointer, out PointTo)) {
					var tmp = Reader.BaseStream.Position;
					Reader.BaseStream.Seek(dat.DataSectionOffset + Pointer, SeekOrigin.Begin);
					PointTo = new(Pointer, new ListType(Reader, dat, PointToType[5..], Count)){Pointer = this};
					Reader.BaseStream.Seek(tmp, SeekOrigin.Begin);
					dat.PointedDatas.Add(Pointer, PointTo);
				} else if (Count <= 0)
					PointTo = NullList;
			} else {
                Offset = Reader.BaseStream.Position;
				Pointer = x64 ? Reader.ReadInt64() : Reader.ReadUInt32();
				if (!dat.PointedDatas.TryGetValue(Pointer, out PointTo)) {
					var tmp = Reader.BaseStream.Position;
					Reader.BaseStream.Seek(dat.DataSectionOffset + Pointer, SeekOrigin.Begin);
					PointTo = new(Pointer, Create(PointToType, Reader, dat)) { Pointer = this };
					Reader.BaseStream.Seek(tmp, SeekOrigin.Begin);
					dat.PointedDatas.Add(Pointer, PointTo);
				}
			}
			Type = PointTo.FieldValue.Type;
		}

		public static readonly PointedValue NullList = new(0, new ListType(null, null, null, 0));
	}
}