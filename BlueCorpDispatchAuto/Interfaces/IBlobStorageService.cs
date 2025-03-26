using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueCorpDispatchAuto
{
    public interface IBlobStorageService
    {
        Task UploadCsvToBlobAsync(string fileName, string csvData);
    }

}
