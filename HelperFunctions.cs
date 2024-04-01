
using System.Text.Json;

public class HelperFunctions
{
    public static readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images.json");
    public static readonly string imageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedImages");


    public Func<IFormFile, bool> IsValidFile = (file) =>
    {
        return file is not null && file.Length > 0;
    };

    public Func<string, bool> IsValidExtension = (filename) =>
    {
        string fileExtension = Path.GetExtension(filename).ToLower();
        return fileExtension == ".jpeg" || fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif";
    };

    public Func<IFormFile, IWebHostEnvironment, Task<string>> HandleImageUpload = async (file, env) =>
    {
        if (!Directory.Exists(imageFolder))
            Directory.CreateDirectory(imageFolder);

        var path = Path.Combine(imageFolder, file.FileName);
        Console.WriteLine(path);
        using var stream = System.IO.File.OpenWrite(path);
        await file.CopyToAsync(stream);
        return path;
    };

    public Func<string, string, ImgDetails> HandleImageObjectCreation = (title, path) =>
    {
        var id = Guid.NewGuid().ToString();

        ImgDetails img = new ImgDetails
        {
            Id = id,
            Title = title,
            Path = path,
        };

        return img;
    };

    public Func<ImgDetails, IWebHostEnvironment, Task<string>> HandleJsonCreation = async (image, env) =>
    {
        List<ImgDetails> imageList;

        if (File.Exists(jsonPath))
        {
            var json = await File.ReadAllTextAsync(jsonPath);
            imageList = JsonSerializer.Deserialize<List<ImgDetails>>(json)
            ?? new List<ImgDetails>();
        }
        else
        {
            imageList = new List<ImgDetails>();
        }

        imageList.Add(image);

        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(imageList));

        return jsonPath;
    };

    public Func<string, Task<ImgDetails>> GetImageDetailsFromJson = async (id) =>
    {
        var json = await File.ReadAllTextAsync(jsonPath);
        var imageList = JsonSerializer.Deserialize<List<ImgDetails>>(json)
        ?? throw new Exception("Empty Json File");

        foreach (ImgDetails image in imageList)
        {
            if (image.Id == id)
            {
                return image;
            }
        }

        throw new Exception("Image Not Found");

    };
};

