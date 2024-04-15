
using System.Text.Json;

public class HelperFunctions
{

    public static bool IsValidFile(IFormFile file)
    {
        return file is not null && file.Length > 0;
    }

    public static bool IsValidExtension (string filename)
    {
        string fileExtension = Path.GetExtension(filename).ToLower();
        return fileExtension == ".jpeg" || fileExtension == ".jpg" || fileExtension == ".png" || fileExtension == ".gif";
    }

    public static async Task<ImgDetails> HandleImageUpload (IFormFile file, IWebHostEnvironment env, string title)
    {
        var imageFolder = Path.Combine(env.ContentRootPath, "UploadedImages");

        if (!Directory.Exists(imageFolder))
            Directory.CreateDirectory(imageFolder);

        var id = Guid.NewGuid().ToString();

        var path = Path.Combine(imageFolder, id + Path.GetExtension(file.FileName));
        Console.WriteLine(path);
        using var stream = System.IO.File.OpenWrite(path);
        await file.CopyToAsync(stream);

        ImgDetails img = new ImgDetails
        {
            Id = id,
            Title = title,
            Path = Path.Combine("UploadedImages", id),
            Extension = Path.GetExtension(file.FileName)
        };

        return img;
    }


    public static async Task<string> HandleJsonCreation (ImgDetails image, IWebHostEnvironment env)
    {
        List<ImgDetails> imageList;

        var jsonPath = Path.Combine(env.ContentRootPath, "images.json");

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
    }

    public static async Task<ImgDetails> GetImageDetailsFromJson (string id, IWebHostEnvironment env)
    {
        var jsonPath = Path.Combine(env.ContentRootPath, "images.json");
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

    }
};

