namespace AssetMon.Infrastructure.Persistence;

public interface IBlobService
{
    bool UploadBlob(string fileName, string contents, string container);

    string GetBlobString(string blobName, string container);

}
