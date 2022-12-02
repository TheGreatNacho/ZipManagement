using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace ZipManagement
{
   
    public class ZipReader : BinaryReader
    {
        public ZipReader(Stream stream) : base(stream) { }

        public LocalFileHeader GetLocalFileHeader(CentralDirectoryRecord record)
        {
            BaseStream.Position = record.FileOffset;
            if (ReadUInt32() != 0x04034b50)
                throw new ArgumentException($"Record address {record.FileOffset.ToString("x")} is not  valid.");


            ushort version = ReadUInt16();
            ushort flag = ReadUInt16();
            ushort compression = ReadUInt16();
            ushort file_modified_time = ReadUInt16();
            ushort file_modified_date = ReadUInt16();
            uint crc_32 = ReadUInt32();
            uint compressed_size = ReadUInt32();
            uint uncompressed_size = ReadUInt32();
            ushort filename_length = ReadUInt16();
            ushort extrafield_length = ReadUInt16();
            string name = new string(ReadChars(filename_length));
            byte[] extra = ReadBytes(extrafield_length);
            LocalFileHeader header = new LocalFileHeader(
                    version,
                    flag,
                    compression,
                    file_modified_time,
                    file_modified_date,
                    crc_32,
                    compressed_size,
                    uncompressed_size,
                    name,
                    extra
             );

            return header;
        }

        public EOCDRecord GetEOCDRecord()
        {
            long offset = BaseStream.Length - 1;
            while (offset > 0)
            {
                offset -= 1;
                BaseStream.Position = offset;
                byte lower = this.ReadByte();
                byte upper = this.ReadByte();
                int size = upper << 8 | lower;
                if (size == (BaseStream.Length - offset) - 2)
                {
                    BaseStream.Position = offset - 20;
                    uint signature = ReadUInt32();
                    if (signature == 0x06054b50)
                    {
                        ushort disk_count = ReadUInt16();
                        ushort cd_disk = ReadUInt16();
                        ushort cds_on_disk = ReadUInt16();
                        ushort cd_record_count = ReadUInt16();
                        uint cd_size = ReadUInt32();
                        uint cd_offset = ReadUInt32();
                        ushort comment_length = ReadUInt16();
                        string comment = new string(ReadChars(comment_length));
                        return new EOCDRecord(disk_count, cd_disk, cds_on_disk, cd_record_count, cd_size, cd_offset, comment);
                    }
                }
            }
            throw new EndOfStreamException();
        }

        public CentralDirectoryRecord[] GetCentralDirectoryRecords(EOCDRecord eocd)
        {
            CentralDirectoryRecord[] records = new CentralDirectoryRecord[eocd.CentralDirectoryCount];
            BaseStream.Position = eocd.CentralDirectoryOffset;
            for (int i=0; i < eocd.CentralDirectoryCount; i++)
            {
                if (ReadInt32() != 0x02014b50)
                    throw new IndexOutOfRangeException($"Record {i}/{eocd.CentralDirectoryCount} is not valid at offset {eocd.CentralDirectoryOffset.ToString("x")}");


                ushort version_madeby = ReadUInt16();
                ushort version_needed = ReadUInt16();
                ushort flag = ReadUInt16();
                ushort compression = ReadUInt16();
                ushort file_modified_time = ReadUInt16();
                ushort file_modified_date = ReadUInt16();
                uint crc_32 = ReadUInt32();
                uint compressed_size = ReadUInt32();
                uint uncompressed_size = ReadUInt32();
                ushort file_name_length = ReadUInt16();
                ushort extra_field_length = ReadUInt16();
                ushort file_comment_length = ReadUInt16();
                ushort file_start_disk = ReadUInt16();
                ushort int_file_attr = ReadUInt16();
                uint ext_file_attr = ReadUInt32();
                uint file_offset = ReadUInt32();
                string file_name = new string(ReadChars(file_name_length));
                string extra_field = new string(ReadChars(extra_field_length));
                string comment = new string(ReadChars(file_comment_length));

                records[i] = new CentralDirectoryRecord(version_madeby, version_needed, flag, compression,
                                                        file_modified_time, file_modified_date, crc_32, compressed_size,
                                                        uncompressed_size, file_start_disk, int_file_attr, ext_file_attr,
                                                        file_offset, file_name, extra_field, comment);
            }

            return records;
        }
        public byte[] GetEncryptedFile(CentralDirectoryRecord record)
        {
            LocalFileHeader local_header = GetLocalFileHeader(record);
            return ReadBytes((int)record.CompressedSize);
        }
    }
}