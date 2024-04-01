using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAntiforgery();
WebApplication app = builder.Build();
app.UseAntiforgery();

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
                    <input type=""text"" name=""title"" class=""form-control"" placeholder=""enter image title"" /> <br/><br/>
                    <input type=""file"" name=""file"" class=""form-control""/>
                    <br/>
                    <button type=""submit"" class=""btn btn-primary mt-3"">Upload Image</button>
                </form>
            </div>
        </body>
        </html>
        ";
    return Results.Content(html, "text/html");
});

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

        HelperFunctions helperFunctions = new HelperFunctions();

        await antiforgery.ValidateRequestAsync(context);

        if (!helperFunctions.IsValidExtension(file.FileName))
            return Results.BadRequest("Invalid Extension");

        if (!helperFunctions.IsValidFile(file))
        {
            return Results.BadRequest("Invalid File");
        }

        var path = await helperFunctions.HandleImageUpload(file, env);
        ImgDetails img = helperFunctions.HandleImageObjectCreation(title, path);
        var json = await helperFunctions.HandleJsonCreation(img, env);
        return Results.Redirect($"picture/{img.Id}");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex?.Message ?? string.Empty);
    }
});

app.MapGet("/picture/{Id:string}", async ([FromRoute] string Id) =>
{
    try
    {
        HelperFunctions helperFunctions = new HelperFunctions();

        ImgDetails img = await helperFunctions.GetImageDetailsFromJson(Id);

        var html = $@"
        <html>
        <head>
            <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.2.0-beta1/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-0evHe/X+R7YkIZDRvuzKMRqM+OrBnVFBL6DOitfPri4tjfHxaWutUpFmBp4vmVor"" crossorigin=""anonymous"">
        </head>
        <body>
            <div class=""container"">
                <h1>{img.Title}</h1>
                <img class=""img-fluid"" src=""{img.Path}"" alt=""{img.Title}"">

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

app.Run();
