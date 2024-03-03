namespace BlogDotNet8.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile file);
        //used with images stored in project when refering to a path
        Task<byte[]> EncodeImageAsync(string fileName);
        string DecodeImage(byte[] data, string type);
        string ContentType(IFormFile file);
        int Size(IFormFile file);
    }
}
