using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCorpDispatchAuto
{
    public interface IRawJsonBlobStorageService
    {
        Task UploadJsonAsync(string fileName, string jsonData);
        Task<string?> DownloadJsonAsync(string fileName);
        Task<bool> JsonExistsAsync(string fileName);
    }
}
