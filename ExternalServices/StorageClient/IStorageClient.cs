using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.StorageClient
{
    public interface IStorageClient
    {
        public Task<string?> UploadProfilePicture(string encodedPicture);
    }
}
