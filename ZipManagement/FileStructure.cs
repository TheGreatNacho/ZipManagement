using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipManagement
{
    /// <summary>
    /// This class represents a Local File Header.
    /// </summary>
    public class LocalFileHeader
    {
        private ushort _version; // Version needed to extract
        private ushort _flag; // General purpose bit flag
        private ushort _compression; // Compression Method
        private ushort _file_modified_time; // File last modification time
        private ushort _file_modified_date; // File last modification date
        private uint _crc_32; // CRC-32 of uncompressed data
        private uint _compressed_size; // Compressed size (or 0xffffffff for ZIP64)
        private uint _uncompressed_size; // Uncompressed size (or 0xffffffff for ZIP64)
        private string _name;
        private byte[] _extra;
        public ushort Version
        {
            get { return _version; }
        }
        public ushort Flag
        {
            get { return _flag; }
        }
        public ushort Compression
        {
            get { return _compression; }
        }
        public ushort FileModifiedTime
        {
            get { return _file_modified_time; }
        }
        public ushort FileModifiedDate
        {
            get { return _file_modified_date; }
        }
        public uint CRC
        {
            get { return _crc_32; }
        }
        public uint CompressedSize
        {
            get { return _compressed_size; }
        }
        public uint UncompressedSize
        {
            get { return _uncompressed_size; }
        }
        public uint FileNameLength
        {
            get { return (uint)FileName.Length; }
        }
        public uint ExtraLength
        {
            get { return (uint)ExtraField.Length; }
        }
        public string FileName
        {
            get { return _name; }
        }
        public byte[] ExtraField
        {
            get { return _extra; }
        }

        public LocalFileHeader(ushort version, ushort flag, ushort compression,
                               ushort file_modified_time, ushort file_modified_date, uint crc_32,
                               uint compressed_size, uint uncompressed_size, string name, byte[] extra)
        {
            _version = version;
            _flag = flag;
            _compression = compression;
            _file_modified_time = file_modified_time;
            _file_modified_date = file_modified_date;
            _crc_32 = crc_32;
            _compressed_size = compressed_size;
            _uncompressed_size = uncompressed_size;
            _name = name;
            _extra = extra;
        }
    }

    public class CentralDirectoryRecord
    {
        private ushort _version_madeby;
        private ushort _version_needed;
        private ushort _flag;
        private ushort _compression;
        private ushort _file_modified_time;
        private ushort _file_modified_date;
        private uint _crc_32;
        private uint _compressed_size;
        private uint _uncompressed_size;
        private ushort _file_start_disk;
        private ushort _int_file_attr;
        private uint _ext_file_attr;
        private uint _file_offset;
        private string _file_name;
        private string _extra_field;
        private string _comment;

        public ushort VersionMadeBy => _version_madeby;
        public ushort VersionNeeded => _version_needed;
        public ushort Flag => _flag;
        public ushort Compression => _compression;
        public ushort FileModifiedTime => _file_modified_time;
        public ushort FileModifiedDate => _file_modified_date;
        public uint CRC => _crc_32;
        public uint CompressedSize => _compressed_size;
        public uint UncompressedSize => _uncompressed_size;
        public ushort FileStartDisk => _file_start_disk;
        public ushort IntFileAttr => _int_file_attr;
        public uint ExtFileAttr => _ext_file_attr;
        public uint FileOffset => _file_offset;
        public string FileName => _file_name;
        public string ExtraField => _extra_field;
        public string Comment => _comment;

        public CentralDirectoryRecord(ushort version_madeby, ushort version_needed, ushort flag, ushort compression,
                                      ushort file_modified_time, ushort file_modified_date, uint crc_32, uint compressed_size,
                                      uint uncompressed_size, ushort file_start_disk, ushort int_file_attr, uint ext_file_attr,
                                      uint file_offset, string file_name, string extra_field, string comment)
        {
            _version_madeby = version_madeby;
            _version_needed = version_needed;
            _flag = flag;
            _compression = compression;
            _file_modified_time = file_modified_time;
            _file_modified_date = file_modified_date;
            _crc_32 = crc_32;
            _compressed_size = compressed_size;
            _uncompressed_size = uncompressed_size;
            _file_start_disk = file_start_disk;
            _int_file_attr = int_file_attr;
            _ext_file_attr = ext_file_attr;
            _file_offset = file_offset;
            _file_name = file_name;
            _extra_field = extra_field;
            _comment = comment;
        }
    }

    /// <summary>
    /// This class represents the End of Central Directory
    /// </summary>
    public class EOCDRecord
    {
        private ushort _disk_count;
        private ushort _cd_disk;
        private ushort _cd_records_on_disk;
        private ushort _cd_record_count;
        private uint _cd_size;
        private uint _cd_offset;
        private string _comment;

        public ushort DiskCount => _disk_count;
        public ushort CentralDirectoryDisk => _cd_disk;
        public ushort CentralDirectoriesOnDisk => _cd_records_on_disk;
        public ushort CentralDirectoryCount => _cd_record_count;
        public uint CentralDirectorySize => _cd_size;
        public uint CentralDirectoryOffset => _cd_offset;
        public uint CommentLength => (uint)Comment.Length;
        public string Comment => _comment;

        public EOCDRecord(ushort disk_count, ushort cd_disk, ushort cd_records_on_disk,
                          ushort cd_record_count, uint cd_size, uint cd_offset, string comment)
        {
            _disk_count = disk_count;
            _cd_disk = cd_disk;
            _cd_records_on_disk = cd_records_on_disk;
            _cd_record_count = cd_record_count;
            _cd_size = cd_size;
            _cd_offset = cd_offset;
            _comment = comment;
        }
    }
}
