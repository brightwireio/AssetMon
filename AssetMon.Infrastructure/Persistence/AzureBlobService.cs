namespace AssetMon.Infrastructure.Persistence;

public class AzureBlobService : IBlobService
{
    private readonly string _connectionString;

    public AzureBlobService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public bool UploadBlob(string fileName, string contents, string container)
    {
        try
        {
            BlobContainerClient c = new BlobContainerClient(_connectionString, container);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(contents)))
            {
                var response = c.UploadBlob(fileName, stream);
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public string GetBlobString(string blobName, string container)
    {
        BlobContainerClient c = new BlobContainerClient(_connectionString, container);
        BlobClient blob = c.GetBlobClient(blobName);
        using (var stream = blob.OpenRead())
        {
            StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }
    }
}
