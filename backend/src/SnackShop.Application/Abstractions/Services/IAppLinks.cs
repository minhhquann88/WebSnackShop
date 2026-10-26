namespace SnackShop.Application.Abstractions.Services;

/// <summary>Builds absolute links to frontend pages for emails.</summary>
public interface IAppLinks
{
    string ShopName { get; }

    string Frontend(string relativePath);
}
