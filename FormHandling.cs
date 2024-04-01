
using System.Text.Json;

public class FormHandling
{
    public FormHandling()
    {

    }

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
        var folder = Path.Combine(env.ContentRootPath, "UploadedImages");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, file.FileName);
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
        var jsonPath = Path.Combine(env.ContentRootPath, "images.json");
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
};

