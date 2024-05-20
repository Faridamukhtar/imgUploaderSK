using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery();
WebApplication app = builder.Build();
app.UseAntiforgery();

//Image form (landing page)
app.MapGet("/", (HttpContext context, IAntiforgery antiforgery) =>
{
    var token = antiforgery.GetAndStoreTokens(context);
    var html = $@"
        <html>
        <head>
            <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.0-beta1/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-0evHe/X+R7YkIZDRvuzKMRqM+OrBnVFBL6DOitfPri4tjfHxaWutUpFmBp4vmVor"" crossorigin=""anonymous"">
        </head>
        <body>
            <div class=""container"">
                <h1>Upload Image</h1>
                <div class=""alert alert-info mb-3"">You can find the uploaded file in 'uploaded' directory</div>
                <form action=""/upload"" method=""post"" enctype=""multipart/form-data"">
                    <input name=""{token.FormFieldName}"" type=""hidden"" value=""{token.RequestToken}"" />
                    <input type=""text"" name=""title"" class=""form-control"" placeholder=""enter image title"" required/> <br/><br/>
                    <input type=""file"" name=""file"" class=""form-control"" required/>
                    <br/>
                    <button type=""submit"" class=""btn btn-primary mt-3"">Upload Image</button>
                </form>
            </div>
        </body>
        </html>
        ";
    return Results.Content(html, "text/html");
});

//Uploading submitted image
app.MapPost("/upload", async (
    IFormFile file,
    [FromForm] string title,
    HttpContext context,
    IAntiforgery antiforgery,
    IWebHostEnvironment env
    ) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);

        if (!HelperFunctions.IsValidFile(file))
        {
            return Results.BadRequest("Invalid File");
        }

        if (!HelperFunctions.IsValidExtension(file.FileName))
            return Results.BadRequest("Invalid Extension");


        var img = await HelperFunctions.HandleImageUpload(file, env, title);
        var json = await HelperFunctions.HandleJsonCreation(img, env);
        return Results.Redirect($"picture/{img.Id}");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex?.Message ?? string.Empty);
    }
});

//Post-Submit Redirection page
app.MapGet("/picture/{Id}", async ([FromRoute] string Id, IWebHostEnvironment env) =>
{
    try
    {
        ImgDetails img = await HelperFunctions.GetImageDetailsFromJson(Id, env);

        var html = $@"
        <html>
        <head>
            <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.0-beta1/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-0evHe/X+R7YkIZDRvuzKMRqM+OrBnVFBL6DOitfPri4tjfHxaWutUpFmBp4vmVor"" crossorigin=""anonymous"">
        </head>
        <body>
            <div class=""container"">
                <h1>{img.Title}</h1>
                <img class=""img-fluid"" src=""/{img.Path}"" alt=""{img.Title}"">

            </div>
        </body>
        </html>
        ";
        return Results.Content(html, "text/html");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex?.Message ?? string.Empty);
    }

});

//Get Image from UploadedImages Folder (Endpoint)
app.MapGet("/UploadedImages/{Id}", async ([FromRoute] string Id, IWebHostEnvironment env) =>
{
    ImgDetails img = await HelperFunctions.GetImageDetailsFromJson(Id, env);
    var imagePath = img.Path + img.Extension;

    if (File.Exists(imagePath))
    {
        try
        {
            FileStream file = File.OpenRead(imagePath);
            return Results.File(file, img.Extension);
        }
        catch(Exception ex)
        {
            return Results.Problem(ex.Message ?? string.Empty);
        }
    }
    else
    {
        return Results.NotFound();
    }


});


app.Run();
