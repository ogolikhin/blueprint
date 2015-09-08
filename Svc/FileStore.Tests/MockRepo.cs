using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileStore;
using FileStore.Models;
using System.Globalization;
using FileStore.Repositories;

namespace FileStore.Tests
{
    public class MockRepo : Repositories.IFilesRepository
    {
        private Dictionary<Guid, File> _files = new Dictionary<Guid, File>();

        public MockRepo()
        {
            File file = new File();
            file.FileId = new Guid("11111111-1111-1111-1111-111111111111");
            file.FileName = "Test1.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test1 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = "text/html";
            _files.Add(file.FileId, file);

            file = new File();
            file.FileId = new Guid("22222222-2222-2222-2222-222222222222");
            file.FileName = "Test2.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test2 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = "text/html";
            _files.Add(file.FileId, file);

            file = new File();
            file.FileId = new Guid("33333333-3333-3333-3333-333333333333");
            file.FileName = "Test3.txt";
            file.FileContent = Encoding.UTF8.GetBytes("Test3 content");
            file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
            file.FileType = "text/html";
            _files.Add(file.FileId, file);
        }

        public async Task<Guid?> PostFile(File file)
        {
            await Task.Delay(100);
            try
            {
                file.FileId = Guid.NewGuid();
                file.StoredTime = DateTime.ParseExact("2015-09-05T22:57:31.7824054-04:00", "o", CultureInfo.InvariantCulture);
                _files.Add(file.FileId, file);
                return file.FileId;
            }
            catch
            {
            }
            return Guid.Empty;
        }

        public async Task<Guid?> DeleteFile(Guid guid)
        {
            await Task.Delay(100);
            try
            {
                _files.Remove(guid);
                return guid;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public async Task<File> GetFile(Guid fileId)
        {
            await Task.Delay(100);
            File file = null;
            try
            {
                file = _files[fileId];
            }
            catch
            {
            }
            return file;
        }

        public async Task<File> HeadFile(Guid guid)
        {
            await Task.Delay(100);
            File file = null;
            try
            {
                file = _files[guid];
                file.FileContent = Encoding.UTF8.GetBytes("");
            }
            catch
            {
            }
            return file;
        }

        public Task<bool> GetStatus()
        {
            throw new NotImplementedException();
        }
    }
}
