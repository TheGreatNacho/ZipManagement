using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace ZipManagement
{
    /// <summary>
    /// This class reads ZIP files as offers methods for extracting files and folders.
    /// </summary>
    public class ZipReader : BinaryReader
    {
        /// <summary>
        /// This is the default constructor for constructing <see cref="ZipReader"/>s.
        /// </summary>
        /// <param name="stream">An open stream with read access.</param>
        /// <example>
        /// In this example we create a <see cref="Stream"/> and then a <see cref="ZipReader"/> from that stream.
        /// <code>
        /// Stream stream = new FileStream("Example.zip", FileMode.Open);
        /// 
        /// using (ZipReader reader = new ZipReader(stream)){
        ///     // Do stuff
        /// }
        /// </code>
        /// </example>
        public ZipReader(Stream stream) : base(stream) { }

        /// <summary>
        /// This method returns a <see cref="LocalFileHeader"/> from a <see cref="CentralDirectoryRecord"/>.
        /// </summary>
        /// <param name="record">A CentralDirectoryRecord, usually obtained from the <see cref="GetCentralDirectoryRecords(EOCDRecord)"/> method.</param>
        /// <returns>A constructed <see cref="LocalFileHeader"/>.</returns>
        /// <exception cref="ArgumentException">This exception is thrown when the <see cref="CentralDirectoryRecord.FileOffset"/> does not point to the Int32 <c>0x04034b50</c>.</exception>
        /// <remarks>
        /// It sets BaseStream.Position to the <see cref="CentralDirectoryRecord"/>'s <see cref="CentralDirectoryRecord.FileOffset"/>,
        /// then reads the <see cref="LocalFileHeader"/> based on the structure of a PKZip file.
        /// </remarks>
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
        /// <summary>
        /// This method returns the End Of Central Directory Record of the ZipFile.
        /// </summary>
        /// <returns>The EOCDRecord of the Zip File</returns>
        /// <exception cref="EndOfStreamException">This exception is thrown if the EOCDRecord has not been found due to searching all possible bytes.</exception>
        /// <remarks>
        /// It scans from the bottom of the PKZip up until it reads an Int16 equal to the offset from the end - 2.
        /// Once it reads that Int16, it assumes it's the end of the EOCDRecord and moves the stream position 20 bytes back.
        /// If the Int32 it then reads is equal to 0x06054b50 it knows it's read the EOCDRecord.
        /// Otherwise it will continue scanning the file from the last offset up.
        /// 
        /// Due to the maximum size of an Int16 being 65535, the EOCDRecord can only be within 65535 + 20 bytes from the end of the file.
        /// </remarks>
        public EOCDRecord GetEOCDRecord()
        {
            long offset = BaseStream.Length - 1;
            while (BaseStream.Length - offset < 65535 & offset > 0)
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
        /// <summary>
        /// This method returns an array of <see cref="CentralDirectoryRecord"/>s from the <see cref="EOCDRecord"/>. 
        /// </summary>
        /// <param name="eocd"></param>
        /// <returns>An array of <see cref="CentralDirectoryRecord"/>s.</returns>
        /// <exception cref="IndexOutOfRangeException">This exception is thrown if any of the <see cref="CentralDirectoryRecord"/>s read from the <see cref="EOCDRecord"/> do not start with <c>0x02014b50</c></exception>
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
        /// <summary>
        /// This method returns a file from the <see cref="CentralDirectoryRecord"/> provided.
        /// The file may or may not be encrypted. See the remarks.
        /// </summary>
        /// <param name="record">The <see cref="CentralDirectoryRecord"/> of the file</param>
        /// <returns>An array of bytes containing the file data.</returns>
        /// <remarks>
        /// If the <see cref="LocalFileHeader.Compression"/> associated with the <see cref="CentralDirectoryRecord"/> is equal to 0, the file will not be encrypted.
        /// Otherwise the file will be encrypted with some kind of compression algorithm. The most common being the DEFLATE algorithm.
        /// </remarks>
        public byte[] GetEncryptedFile(CentralDirectoryRecord record)
        {
            LocalFileHeader local_header = GetLocalFileHeader(record);
            return ReadBytes((int)record.CompressedSize);
        }
    }
}